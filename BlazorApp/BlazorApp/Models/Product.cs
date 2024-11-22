using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        [NotMapped]                
        public int totalRows { get; set; }        
    }
}
