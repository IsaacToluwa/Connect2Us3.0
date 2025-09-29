using System;
using System.Collections.Generic;
using System.Linq;

namespace book2us.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        
        public virtual User Customer { get; set; }
        
        public int OrderId { get { return Id; } }
    }
}