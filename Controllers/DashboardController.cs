using DigitalLibrarySystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrarySystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly LibraryContext _db;
        public DashboardController(LibraryContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null) return RedirectToAction("Login", "Account");

            var activeLoans = await _db.Transactions
                .Where(t => t.StudentID == sid && t.Status == "Issued")
                .CountAsync();

            var totalBooks = await _db.Books.CountAsync();
            var availableBooks = await _db.Books.CountAsync(b => b.IsAvailable);

            ViewBag.ActiveLoans = activeLoans;
            ViewBag.TotalBooks = totalBooks;
            ViewBag.Available = availableBooks;
            ViewBag.StudentName = HttpContext.Session.GetString("StudentName");
            ViewBag.UniversityID = HttpContext.Session.GetString("UniversityID");

            return View();
        }
    }
}