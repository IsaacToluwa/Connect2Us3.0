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
    }
}