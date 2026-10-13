namespace DigitalLibrarySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[Route("Analytics")]
public class AnalyticsController : Controller
{
    private readonly string _connectionString = string.Empty;

    public AnalyticsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("Role") == "Admin";
    }

    [Route("MostActiveStudents")]
    public IActionResult MostActiveStudents()
    {
        var data = new List<object>();
        using SqlConnection con = new SqlConnection(_connectionString);
        string query = @"SELECT TOP 10 
            s.StudentID, s.FullName as Name,
            ISNULL(s.Department, 'N/A') as Department,
            COUNT(t.TransactionID) as TotalBorrows
            FROM Students s
            LEFT JOIN Transactions t ON s.StudentID = t.StudentID
            WHERE s.Role = 'Student'
            GROUP BY s.StudentID, s.FullName, s.Department
            ORDER BY TotalBorrows DESC";

        SqlCommand cmd = new SqlCommand(query, con);
        con.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new
            {
                StudentID = reader["StudentID"],
                Name = reader["Name"],
                Department = reader["Department"],
                TotalBorrows = reader["TotalBorrows"]
            });
        }
        return Json(data);
    }

    [Route("PopularBooks")]
    public IActionResult PopularBooks()
    {
        var data = new List<object>();
        using SqlConnection con = new SqlConnection(_connectionString);
        string query = @"SELECT TOP 10 
            b.BookID, b.Title, b.Author,
            COUNT(t.TransactionID) as BorrowCount
            FROM Books b
            LEFT JOIN Transactions t ON b.BookID = t.BookID
            GROUP BY b.BookID, b.Title, b.Author
            ORDER BY BorrowCount DESC";

        SqlCommand cmd = new SqlCommand(query, con);
        con.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new
            {
                BookID = reader["BookID"],
                Title = reader["Title"],
                Author = reader["Author"],
                BorrowCount = reader["BorrowCount"]
            });
        }
        return Json(data);
    }

    [Route("FineTrends")]
    public IActionResult FineTrends()
    {
        var data = new List<object>();
        using SqlConnection con = new SqlConnection(_connectionString);
        string query = @"SELECT 
            MONTH(IssueDate) as Month,
            YEAR(IssueDate) as Year,
            SUM(FineAmount) as TotalFines,
            COUNT(*) as FineCount
            FROM Transactions
            WHERE FineAmount > 0
            GROUP BY YEAR(IssueDate), MONTH(IssueDate)
            ORDER BY Year DESC, Month DESC";

        SqlCommand cmd = new SqlCommand(query, con);
        con.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new
            {
                Month = reader["Month"],
                Year = reader["Year"],
                TotalFines = reader["TotalFines"],
                FineCount = reader["FineCount"]
            });
        }
        return Json(data);
    }

    [Route("DepartmentStats")]
    public IActionResult DepartmentStats()
    {
        var data = new List<object>();
        using SqlConnection con = new SqlConnection(_connectionString);
        string query = @"SELECT 
            ISNULL(s.Department, 'Unknown') as Department,
            COUNT(DISTINCT s.StudentID) as TotalStudents,
            COUNT(t.TransactionID) as TotalBorrows,
            SUM(ISNULL(t.FineAmount, 0)) as TotalFines
            FROM Students s
            LEFT JOIN Transactions t ON s.StudentID = t.StudentID
            WHERE s.Role = 'Student'
            GROUP BY s.Department
            ORDER BY TotalBorrows DESC";

        SqlCommand cmd = new SqlCommand(query, con);
        con.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new
            {
                Department = reader["Department"],
                TotalStudents = reader["TotalStudents"],
                TotalBorrows = reader["TotalBorrows"],
                TotalFines = reader["TotalFines"]
            });
        }
        return Json(data);
    }

    [Route("ActivityLogs")]
    public IActionResult ActivityLogs()
    {
        var data = new List<object>();
        using SqlConnection con = new SqlConnection(_connectionString);
        string query = @"SELECT TOP 50 
            LogID, Action, Description, IPAddress, LoggedAt, IsSuccess
            FROM ActivityLogs
            ORDER BY LoggedAt DESC";

        SqlCommand cmd = new SqlCommand(query, con);
        con.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new
            {
                LogID = reader["LogID"],
                Action = reader["Action"],
                Description = reader["Description"],
                IPAddress = reader["IPAddress"],
                LoggedAt = reader["LoggedAt"],
                IsSuccess = reader["IsSuccess"]
            });
        }
        return Json(data);
    }

    [Route("Index")]
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
            return RedirectToAction("Index", "Admin");
        return View();
    }
}