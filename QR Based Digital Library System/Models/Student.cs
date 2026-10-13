using System.ComponentModel.DataAnnotations;

namespace DigitalLibrarySystem.Models
{
    public class Student
    {
        public int StudentID { get; set; }

        [Required] public string UniversityID { get; set; } = "";
        [Required] public string FullName { get; set; } = "";
        [Required] public string Email { get; set; } = "";
        [Required] public string PasswordHash { get; set; } = "";
        [Required] public string Section { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public string Role { get; set; } = "Student";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}