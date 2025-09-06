using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WEB.Models;
using WEB.Constant;
using WEB.CallApi;
using Shared.Products;
using WEB.ConfigClass;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
namespace WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IcallApi _icallApi;
        private readonly ApiSettings _apiSettings;

        public HomeController(
            ILogger<HomeController> logger,
            IcallApi icallApi,
            IOptions<ApiSettings> apiSettings)
        {
            _logger = logger;
            _icallApi = icallApi;
            _apiSettings = apiSettings.Value;
        }

        public async Task<IActionResult> Index()
        {
            using var client = new HttpClient();
            var url = _apiSettings.BaseUrl + Endpoints.PRODUCT_GETBYID;
            var dto = new GetProductsDTO
            {
                ProductId = 6
            };
            var result = await _icallApi.PostAsync(url, dto, null);
            return View();
        }
        public async Task<IActionResult> Privacy()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:29092"
            };

            using var producer = new ProducerBuilder<Null, string>(config).Build();

            var message = "Message gửi đến kafka";
            try
            {
                var result = await producer.ProduceAsync("test-topic", new Message<Null, string> { Value = message });
                _logger.LogInformation("✔️ Sent to partition {Partition}, offset {Offset}", result.Partition, result.Offset);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "❌ Error sending message to Kafka");
            }

            return View();
        }
        public IActionResult Privacy1()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",
                GroupId = "my-consumer-group-v1",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("test-topic");

            var cr = consumer.Consume(TimeSpan.FromSeconds(5));
            if (cr != null)
            {
                ViewBag.Message = $"📥 Nhận được message: {cr.Value}";
            }
            else
            {
                ViewBag.Message = "⏰ Không có message nào trong 5 giây";
            }

            return View("Privacy");
        }

    }
}
