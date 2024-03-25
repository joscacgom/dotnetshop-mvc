using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace dotnetshop.Models
{
    public class ShoppingCart
    {
        [Key]
        public int ShoppingCartId { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        [Range(1, 100, ErrorMessage = "Please enter a value between 1 and 100")]
        public int Quantity { get; set; }

        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        [ValidateNever]
        public AppUser User { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}