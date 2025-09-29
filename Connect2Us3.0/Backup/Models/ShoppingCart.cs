using System.Collections.Generic;

namespace book2us.Models
{
    public class ShoppingCart
    {
        public string ShoppingCartId { get; set; }
        public List<OrderDetail> CartItems { get; set; }
    }
}