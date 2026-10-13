using DigitalLibrarySystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrarySystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly LibraryContext _db;
        public AdminController(LibraryContext db) { _db = db; }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalIssued = await _db.Transactions.CountAsync(t => t.Status == "Issued");
            ViewBag.Overdue = await _db.Transactions.CountAsync(t => t.Status == "Issued" && t.DueDate < DateTime.Now);
            ViewBag.ActiveBorrowers = await _db.Transactions
                .Where(t => t.Status == "Issued")
                .Select(t => t.StudentID)
                .Distinct()
                .CountAsync();
            ViewBag.TotalFines = await _db.Transactions.SumAsync(t => t.FineAmount);
            ViewBag.TotalBooks = await _db.Books.CountAsync();

            var mostBorrowed = await _db.Transactions
                .Include(t => t.Book)
                .GroupBy(t => t.Book.Title)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();
            ViewBag.MostBorrowed = mostBorrowed;

            var defaulters = await _db.Transactions
                .Include(t => t.Student)
                .Include(t => t.Book)
                .Where(t => t.Status == "Issued" && t.DueDate < DateTime.Now)
                .ToListAsync();
            ViewBag.Defaulters = defaulters;

            var dailyTx = await _db.Transactions
                .Where(t => t.IssueDate >= DateTime.Now.AddDays(-7))
                .GroupBy(t => t.IssueDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();
            ViewBag.DailyTx = dailyTx;

            return View();
        }

        public async Task<IActionResult> Reports()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var allTx = await _db.Transactions
                .Include(t => t.Student)
                .Include(t => t.Book)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();

            return View(allTx);
        }
    }
}