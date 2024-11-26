using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Product Name is required")]   
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }
        [NotMapped]                
        public int totalRows { get; set; }        
    }
}
