using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace book2us.Models
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
        Refund,
        Payment
    }

    public enum TransactionStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }

    public enum PaymentMethod
    {
        Wallet,
        BankTransfer,
        CardPayment,
        Cash
    }

    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Fee { get; set; }

        [DataType(DataType.Currency)]
        public decimal NetAmount { get; set; }

        [StringLength(100)]
        public string Reference { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string TransactionReference { get; set; }

        public int? RelatedTransactionId { get; set; }

        public int? BankAccountId { get; set; }

        public int? CardId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ProcessedDate { get; set; }

        public string ProcessedBy { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual BankAccount BankAccount { get; set; }

        public virtual CardDetails Card { get; set; }

        public virtual Transaction RelatedTransaction { get; set; }
    }
}