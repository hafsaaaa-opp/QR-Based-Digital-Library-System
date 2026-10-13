using DigitalLibrarySystem.Data;
using DigitalLibrarySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace DigitalLibrarySystem.Controllers
{
    public class LibraryController : Controller
    {
        private readonly LibraryContext _db;
        public LibraryController(LibraryContext db) { _db = db; }

        // MY BOOKS
        public async Task<IActionResult> MyBooks()
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null) return RedirectToAction("Login", "Account");
            var books = await _db.Transactions
                .Include(t => t.Book)
                .Where(t => t.StudentID == sid)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
            return View(books);
        }

        // QR SCANNER PAGE (Issue)
        public IActionResult Scan()
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null) return RedirectToAction("Login", "Account");
            return View();
        }

        // PROCESS QR CODE (for Issuing)
        [HttpPost]
        public async Task<IActionResult> ProcessQR([FromBody] QRRequest req)
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null)
                return Json(new { success = false, message = "Not logged in." });

            var book = await _db.Books.FirstOrDefaultAsync(b => b.QRCode == req.QRCode);
            if (book == null)
                return Json(new { success = false, message = "Book not found for this QR code." });

            if (!book.IsAvailable)
                return Json(new { success = false, message = "This book is already issued to another student." });

            var student = await _db.Students.FindAsync(sid);
            if (student == null || !student.IsActive)
                return Json(new { success = false, message = "Your account is inactive." });

            int activeCount = await _db.Transactions
                .CountAsync(t => t.StudentID == sid && t.Status == "Issued");
            if (activeCount >= 3)
                return Json(new { success = false, message = "Borrowing limit reached (max 3 books)." });

            return Json(new
            {
                success = true,
                bookID = book.BookID,
                title = book.Title,
                author = book.Author,
                publisher = book.Publisher,
                isbn = book.ISBN,
                qrCode = book.QRCode
            });
        }

        // ISSUE BOOK
        [HttpPost]
        public async Task<IActionResult> IssueBook(int bookID)
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null) return RedirectToAction("Login", "Account");

            var book = await _db.Books.FindAsync(bookID);
            if (book == null || !book.IsAvailable)
            { TempData["Error"] = "Book not available."; return RedirectToAction("Scan"); }

            int activeCount = await _db.Transactions
                .CountAsync(t => t.StudentID == sid && t.Status == "Issued");
            if (activeCount >= 3)
            { TempData["Error"] = "Borrowing limit reached."; return RedirectToAction("Scan"); }

            string txID = "TRX" + DateTime.Now.Ticks.ToString().Substring(0, 12);

            var transaction = new Transaction
            {
                TransactionID = txID,
                StudentID = sid.Value,
                BookID = bookID,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Status = "Issued"
            };

            book.IsAvailable = false;
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Book issued! Transaction ID: {txID}. Due: {transaction.DueDate:dd-MM-yyyy}";
            return RedirectToAction("MyBooks");
        }

        // RETURN PAGE
        public IActionResult Return() => View();

        // ✅ RETURN BOOK — Manual Entry (with Fine Calculation)
        [HttpPost]
        public async Task<IActionResult> ReturnBook(string transactionID)
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null) return RedirectToAction("Login", "Account");

            var tx = await _db.Transactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.TransactionID == transactionID && t.StudentID == sid);

            if (tx == null)
            { TempData["Error"] = "Transaction not found."; return View(); }

            if (tx.Status == "Returned")
            { TempData["Error"] = "Book already returned."; return View(); }

            tx.ReturnDate = DateTime.Now;

            // FINE CALCULATION — 14 din ke baad har din Rs 10 fine
            decimal fine = 0;
            if (DateTime.Now > tx.DueDate)
            {
                int lateDays = (DateTime.Now.Date - tx.DueDate.Date).Days;
                fine = lateDays * 10;
            }
            tx.FineAmount = fine;
            tx.Status = "Returned";
            tx.Book.IsAvailable = true;

            await _db.SaveChangesAsync();

            if (fine > 0)
                TempData["Success"] = $"Book returned! Late by {(DateTime.Now.Date - tx.DueDate.Date).Days} days. Fine: Rs {fine}";
            else
                TempData["Success"] = "Book returned successfully! No fine.";

            return RedirectToAction("MyBooks");
        }

        // ✅ RETURN VIA QR CODE (with Fine Calculation)
        [HttpPost]
        public async Task<IActionResult> ProcessReturnQR([FromBody] QRRequest req)
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null) return Json(new { success = false, message = "Not logged in." });

            var book = await _db.Books.FirstOrDefaultAsync(b => b.QRCode == req.QRCode);
            if (book == null)
                return Json(new { success = false, message = "Book not found." });

            var tx = await _db.Transactions
                .FirstOrDefaultAsync(t => t.BookID == book.BookID && t.StudentID == sid && t.Status == "Issued");

            if (tx == null)
                return Json(new { success = false, message = "No active loan found for this book under your account." });

            decimal fine = 0;
            if (DateTime.Now > tx.DueDate)
            {
                int lateDays = (DateTime.Now.Date - tx.DueDate.Date).Days;
                fine = lateDays * 10;
            }

            tx.ReturnDate = DateTime.Now;
            tx.FineAmount = fine;
            tx.Status = "Returned";
            book.IsAvailable = true;

            await _db.SaveChangesAsync();

            return Json(new { success = true, title = book.Title, fine = fine, transactionID = tx.TransactionID });
        }

        // QR CODES PAGE — all books ke labels
        public async Task<IActionResult> QRCodes()
        {
            int? sid = HttpContext.Session.GetInt32("StudentID");
            if (sid == null) return RedirectToAction("Login", "Account");
            var books = await _db.Books.ToListAsync();
            return View(books);
        }

        // GENERATE QR IMAGE
        public IActionResult GenerateQR(string code)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20);
            return File(qrBytes, "image/png");
        }
    }

    public class QRRequest { public string QRCode { get; set; } = ""; }
}