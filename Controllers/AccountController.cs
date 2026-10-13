using DigitalLibrarySystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DigitalLibrarySystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryContext _db;
        public AccountController(LibraryContext db) { _db = db; }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string universityID, string password)
        {
            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.UniversityID == universityID);

            if (student == null)
            { TempData["Error"] = "University ID not found."; return View(); }

            if (!student.IsActive)
            { TempData["Error"] = "Your account is inactive."; return View(); }

            bool valid = GetMD5(password) == student.PasswordHash;

            if (!valid)
            { TempData["Error"] = "Invalid password."; return View(); }

            HttpContext.Session.SetInt32("StudentID", student.StudentID);
            HttpContext.Session.SetString("StudentName", student.FullName);
            HttpContext.Session.SetString("UniversityID", student.UniversityID);
            HttpContext.Session.SetString("Role", student.Role);

            // ✅ Role check — Admin alag dashboard pe jayega
            if (student.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private static string GetMD5(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }
}