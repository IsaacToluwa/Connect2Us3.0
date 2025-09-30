using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace book2us.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(256)]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public string Role { get; set; }
        
        // Profile Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public int? Age { get; set; }
        public string ProfilePicture { get; set; } // Store as base64 string or file path
        
        public virtual ICollection<PrintingRequest> PrintingRequests { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
        
        public ApplicationUser()
        {
            PrintingRequests = new HashSet<PrintingRequest>();
            Wallets = new HashSet<Wallet>();
        }
    }
}