namespace DigitalLibrarySystem.Models
{
    public class Transaction
    {
        public string TransactionID { get; set; } = "";
        public int StudentID { get; set; }
        public int BookID { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Issued";
        public decimal FineAmount { get; set; } = 0;

        public Student Student { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}