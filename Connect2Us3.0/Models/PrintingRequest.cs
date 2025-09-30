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

        public int? BookId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        // New fields for admin printing service and delivery
        public string PdfFilePath { get; set; }
        
        public bool RequiresDelivery { get; set; }
        
        public int? AssignedEmployeeId { get; set; }
        
        public string CustomerName { get; set; }
        
        public string CustomerEmail { get; set; }
        
        public string CustomerPhone { get; set; }
        
        public string DeliveryAddress { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }
        
        [ForeignKey("AssignedEmployeeId")]
        public virtual ApplicationUser AssignedEmployee { get; set; }
    }
}