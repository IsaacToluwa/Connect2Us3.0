using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace book2us.Models
{
    public class CardDetails
    {
        [Key]
        public int CardId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string CardHolderName { get; set; }

        [Required]
        [StringLength(20)]
        public string CardNumber { get; set; } // Store only last 4 digits, encrypt full number

        [Required]
        [StringLength(4)]
        public string LastFourDigits { get; set; }

        [Required]
        [StringLength(50)]
        public string ExpiryMonth { get; set; }

        [Required]
        [StringLength(50)]
        public string ExpiryYear { get; set; }

        [Required]
        [StringLength(50)]
        public string CardType { get; set; } // Visa, MasterCard, etc.

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastUsedDate { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}