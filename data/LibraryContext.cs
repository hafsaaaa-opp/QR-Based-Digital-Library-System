using Microsoft.EntityFrameworkCore;
using DigitalLibrarySystem.Models;

namespace DigitalLibrarySystem.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Transaction PK is string
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.TransactionID);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Student)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.StudentID);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Book)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BookID);
        }
    }
}