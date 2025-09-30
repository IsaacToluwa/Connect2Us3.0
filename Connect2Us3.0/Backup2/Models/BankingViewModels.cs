using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace book2us.Models
{
    public class BankingViewModel
    {
        public List<BankAccount> BankAccounts { get; set; }
        public List<CardDetails> CardDetails { get; set; }
    }

    public class BankAccountViewModel
    {
        [Required]
        public string BankName { get; set; }
        
        [Required]
        public string AccountHolderName { get; set; }
        
        [Required]
        public string AccountNumber { get; set; }
        
        [Required]
        public string RoutingNumber { get; set; }
        
        [Required]
        public AccountType AccountType { get; set; }
    }

    public class CardViewModel
    {
        [Required]
        public string CardHolderName { get; set; }
        
        [Required]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Invalid card number")]
        public string CardNumber { get; set; }
        
        [Required]
        [Range(1, 12, ErrorMessage = "Invalid expiry month")]
        public int ExpiryMonth { get; set; }
        
        [Required]
        [Range(2023, 2030, ErrorMessage = "Invalid expiry year")]
        public int ExpiryYear { get; set; }
        
        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Invalid CVV")]
        public string CVV { get; set; }
    }
}