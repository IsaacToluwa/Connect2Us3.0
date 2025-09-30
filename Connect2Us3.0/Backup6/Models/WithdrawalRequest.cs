using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace book2us.Models
{
    public enum WithdrawalStatus
    {
        Pending,
        Processing,
        Approved,
        Rejected,
        Completed,
        Failed
    }

    public class WithdrawalRequest
    {
        [Key]
        public int WithdrawalRequestId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BankAccountId { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Fee { get; set; }

        [DataType(DataType.Currency)]
        public decimal NetAmount { get; set; }

        [Required]
        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(500)]
        public string RejectionReason { get; set; }

        public string RequestedBy { get; set; }

        public string ProcessedBy { get; set; }

        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public DateTime? ProcessedDate { get; set; }

        public int? TransactionId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual BankAccount BankAccount { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}