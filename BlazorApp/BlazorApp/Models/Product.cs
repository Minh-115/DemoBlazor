using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorApp.Ecrypt;

namespace BlazorApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Product Name is required")]   
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Price is required")]
        public string PriceEncrypt { get; set; } = "";
        [NotMapped]
        public decimal Price
        {
            get
            {
                if (string.IsNullOrEmpty(PriceEncrypt))
                    return 0m;                
                var decrypted = EncryptionHelper.Decrypt(PriceEncrypt);
                return decimal.TryParse(decrypted, out var val) ? val : 0m;
            }
            set
            {                
                PriceEncrypt = EncryptionHelper.Encrypt(value.ToString());
            }
        }
        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "ProductCode is required")]
        public string ProductCode { get; set; } = "";
        [NotMapped]                
        public int totalRows { get; set; }        
    }
}
