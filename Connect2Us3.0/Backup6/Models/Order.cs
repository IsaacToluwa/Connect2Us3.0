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
        public decimal TotalAmount { get { return Total; } set { Total = value; } }
        public string Email { get; set; }
        public string UserName { get; set; }
        
        // Order Status
        public string Status { get; set; } // Pending, Paid, Shipped, Delivered, Cancelled
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        
        // Shipping Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        
        // Printing Service Properties
        public bool IsPrintingService { get; set; }
        public string PrintingStatus { get; set; } // Requested, Processing, Ready, Delivered, PickedUp
        public string FulfillmentMethod { get; set; } // Delivery, Pickup
        public string DeliveryAddress { get; set; } // Delivery address for printing services
        public string CustomerNotes { get; set; } // Customer notes for printing services
        public decimal DeliveryCharge { get; set; } // R10 for delivery
        public decimal PrintingCharge { get; set; } // R3 per page
        public int TotalPages { get; set; }
        public DateTime? ReadyForDeliveryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? AssignedEmployeeId { get; set; } // Employee assigned for delivery
        
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        
        public virtual ApplicationUser Customer { get; set; }
        public virtual ApplicationUser AssignedEmployee { get; set; }
        
        public int OrderId { get { return Id; } }
        
        // Helper method to calculate printing charges
        public void CalculatePrintingCharges()
        {
            if (IsPrintingService && TotalPages > 0)
            {
                PrintingCharge = TotalPages * 3.00m; // R3 per page
                if (FulfillmentMethod == "Delivery")
                {
                    DeliveryCharge = 10.00m; // R10 delivery charge
                }
                else
                {
                    DeliveryCharge = 0.00m; // No charge for pickup
                }
            }
        }
    }
}