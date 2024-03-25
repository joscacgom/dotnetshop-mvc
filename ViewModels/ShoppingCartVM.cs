using dotnetshop.Models;

namespace dotnetshop.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> List { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}