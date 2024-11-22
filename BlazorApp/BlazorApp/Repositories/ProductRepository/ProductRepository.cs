using BlazorApp.Data;
using BlazorApp.Models;

namespace BlazorApp.Repositories.ProductRepository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context) { }
    }
}
