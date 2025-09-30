using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace book2us.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ISBN { get; set; }
        public string Condition { get; set; }
        public string Category { get; set; }
        public int SellerId { get; set; }
        public string SellerUserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsAvailable { get; set; }
        public int? PageCount { get; set; }
        public string Language { get; set; }
        public string Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string CoverImageUrl { get; set; }
        public string Tags { get; set; }
        
        public Book()
        {
            IsAvailable = true;
            CreatedDate = DateTime.Now;
        }
    }
}