using CDCData;

var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddScoped<IElasticService, ElasticService>();
builder.Services.AddSingleton<IElasticProductService, ElasticProductService>();
builder.Services.AddHostedService<ProductKafkaConsumerService>();

var host = builder.Build();
host.Run();
