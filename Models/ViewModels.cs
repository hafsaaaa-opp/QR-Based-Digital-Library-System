using System.ComponentModel.DataAnnotations;

namespace DigitalLibrarySystem.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "University ID zaroori hai")]
        public string UniversityID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password zaroori hai")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class QRScanRequest
    {
        public string QRCode { get; set; } = string.Empty;
    }

    public class QRScanResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int BookID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class DashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string UniversityID { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int ActiveLoans { get; set; }
        public int TotalBooks { get; set; }
        public int AvailableBooks { get; set; }
        public List<Transaction> RecentTransactions { get; set; } = new();
    }
}