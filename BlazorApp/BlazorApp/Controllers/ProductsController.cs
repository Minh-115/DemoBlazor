using Microsoft.AspNetCore.Mvc;
using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet()]
        public IActionResult GetById(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
        [HttpGet()]
        public IActionResult GetByCode(string productcode)
        {
            var product = _context.Products.AsNoTracking().FirstOrDefault(x=>x.ProductCode == productcode);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }
}
