using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace book2us.Models
{
    public class PrintingRequest
    {
        [Key]
        public int PrintingRequestId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string Status { get; set; } // Pending, Processing, Ready, Delivered, PickedUp, Cancelled

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? AssignedDate { get; set; }
        
        public DateTime? ReadyDate { get; set; }
        
        public DateTime? DeliveryDate { get; set; }

        public int? AssignedEmployeeId { get; set; }
        
        public int? AssignedSellerId { get; set; }

        public string FulfillmentMethod { get; set; } // Delivery, Pickup

        public int TotalPages { get; set; }

        public decimal PrintingCost { get; set; } // 3 Rand per page

        public decimal DeliveryCommission { get; set; } // 20% of total

        public string DeliveryAddress { get; set; }

        public string CustomerNotes { get; set; }

        public string EmployeeNotes { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }

        [ForeignKey("AssignedEmployeeId")]
        public virtual ApplicationUser AssignedEmployee { get; set; }

        [ForeignKey("AssignedSellerId")]
        public virtual ApplicationUser AssignedSeller { get; set; }

        public void CalculatePrintingCost()
        {
            // Calculate printing cost: 3 Rand per page
            PrintingCost = TotalPages * 3.00m;
        }

        public void CalculateDeliveryCommission()
        {
            // Calculate delivery commission: 20% of total (printing cost + any additional fees)
            var totalAmount = PrintingCost + (Quantity * (Book?.Price ?? 0));
            DeliveryCommission = totalAmount * 0.20m;
        }
    }
}