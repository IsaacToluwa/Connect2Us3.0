using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace book2us.Models
{
    public enum AccountType
    {
        Checking,
        Savings
    }
    public class BankAccount
    {
        [Key]
        public int BankAccountId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountHolderName { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string RoutingNumber { get; set; }

        [Required]
        public AccountType AccountType { get; set; } // Checking, Savings

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastUsedDate { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}