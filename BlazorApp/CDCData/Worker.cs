using Avro.Generic;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Elastic.Clients.Elasticsearch;
using System.Text.Json;
namespace CDCData;
public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ProductCode { get; set; }
}

public class ProductCdcMessage
{
    public Product? Before { get; set; }
    public Product? After { get; set; }
    public string? Op { get; set; }
}


public interface IElasticProductService
{
    Task IndexAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(int productId, CancellationToken cancellationToken = default);
}

public class ElasticProductService : IElasticProductService
{
    private readonly ElasticsearchClient _client;

    public ElasticProductService(IConfiguration configuration)
    {
        var settings = new ElasticsearchClientSettings(
            new Uri(configuration["Elasticsearch:Url"]!))
            .DefaultIndex("products2")
            .DisableDirectStreaming()
            .PrettyJson();

        _client = new ElasticsearchClient(settings);
    }

    public async Task IndexAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _client.IndexAsync(product, idx => idx
            .Index("products2")
            .Id(product.Id), cancellationToken);
    }

    public async Task DeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteRequest("products2", new Id(productId));
        await _client.DeleteAsync(request, cancellationToken);
    }
}

public class ProductKafkaConsumerService : BackgroundService
{
    private readonly ILogger<ProductKafkaConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IElasticProductService _elasticService;

    public ProductKafkaConsumerService(
        ILogger<ProductKafkaConsumerService> logger,
        IConfiguration configuration,
        IElasticProductService elasticService)
    {
        _logger = logger;
        _configuration = configuration;
        _elasticService = elasticService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = "http://localhost:8081"
        };

        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:29092",
            GroupId = "product-sync-group-2",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        using var consumer = new ConsumerBuilder<string, GenericRecord>(consumerConfig)
            .SetValueDeserializer(
            new AvroDeserializer<GenericRecord>(schemaRegistry)
                .AsSyncOverAsync()).Build();
        var topic = _configuration["Kafka:Topic"] ?? "sqlserver.products";
        consumer.Subscribe(topic);

        _logger.LogInformation("Kafka consumer started. Topic: {Topic}", topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var record = result.Message.Value;
                var message = MapGenericRecordToModel(record);
                await HandleCdcMessage(message, stoppingToken);
                //var after = record["after"] as GenericRecord;
                //var op = record["op"]?.ToString();

                //if (after != null)
                //{
                //    var id = Convert.ToInt32(after["Id"]);
                //    var name = after["Name"]?.ToString();

                //    Console.WriteLine($"{id} - {name}");
                //}


                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing Kafka CDC message");
            }
        }

        consumer.Close();
    }
    private async Task HandleCdcMessage(
        ProductCdcMessage? message,
        CancellationToken cancellationToken)
    {
        if (message == null) return;

        switch (message.Op?.ToLower())
        {
            case "c": // create -> INSERT từ SQL Server CDC
            case "u": // update -> UPDATE từ SQL Server CDC (re-index document mới vào Elasticsearch)
            case "r": // snapshot read -> dữ liệu snapshot ban đầu từ Debezium
                if (message.After is not null)
                {
                    await _elasticService.IndexAsync(message.After, cancellationToken);
                    _logger.LogInformation(
                        "Indexed product to Elasticsearch: {ProductId}",
                        message.After.Id);
                }
                break;

            case "d": // delete
                if (message.Before is not null)
                {
                    await _elasticService.DeleteAsync(message.Before.Id, cancellationToken);
                    _logger.LogInformation(
                        "Deleted product from Elasticsearch: {ProductId}",
                        message.Before.Id);
                }
                break;

            default:
                _logger.LogWarning("Unknown CDC operation: {Operation}", message.Op);
                break;
        }
    }
    private ProductCdcMessage? MapGenericRecordToModel(GenericRecord record)
    {
        // Convert GenericRecord -> Dictionary
        var jsonObject = ConvertRecord(record);

        // Dictionary -> JSON string
        var json = JsonSerializer.Serialize(jsonObject);

        // JSON -> C# Model
        var model = JsonSerializer.Deserialize<ProductCdcMessage>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return model;
    }

    private object? ConvertRecord(object? value)
    {
        if (value == null)
            return null;

        if (value is GenericRecord record)
        {
            var dict = new Dictionary<string, object?>();

            foreach (var field in record.Schema.Fields)
            {
                var fieldValue = record[field.Name];
                dict[field.Name] = ConvertRecord(fieldValue);
            }

            return dict;
        }
        if (value is IEnumerable<object> list && value is not string)
        {
            return list.Select(ConvertRecord).ToList();
        }
        return value;
    }
}