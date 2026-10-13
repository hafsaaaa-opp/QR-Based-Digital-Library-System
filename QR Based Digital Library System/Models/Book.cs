namespace DigitalLibrarySystem.Models
{
    public class Book
    {
        public int BookID { get; set; }
        public string ISBN { get; set; } = "";
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Publisher { get; set; } = "";
        public string Category { get; set; } = "";
        public string QRCode { get; set; } = "";
        public bool IsAvailable { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.Now;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}