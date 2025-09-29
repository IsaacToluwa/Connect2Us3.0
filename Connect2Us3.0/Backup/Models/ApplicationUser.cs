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
        
        public string PasswordHash { get; set; }
        
        public string Role { get; set; }
        
        public virtual ICollection<PrintingRequest> PrintingRequests { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
        
        public ApplicationUser()
        {
            PrintingRequests = new HashSet<PrintingRequest>();
            Wallets = new HashSet<Wallet>();
        }
    }
}