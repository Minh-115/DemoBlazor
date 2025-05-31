using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WEB.Models;
using WEB.Constant;
using WEB.CallApi;
using Shared.Products;
using WEB.ConfigClass;
using Microsoft.Extensions.Options;

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
            IOptions<ApiSettings> apiSettings )
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
                ProductId = 5
            };
            var result = await _icallApi.PostAsync(url, dto, null);
            return View();
        }
    }
}
