namespace QR_Based_Digital_Library_System.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[Route("Recommendation")]
public class RecommendationController : Controller
{
    private readonly string _connectionString = string.Empty;

    public RecommendationController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    [Route("ForStudent/{studentId}")]
    public IActionResult ForStudent(int studentId)
    {
        var recommendations = new List<object>();

        using SqlConnection con = new SqlConnection(_connectionString);
        string query = @"
            SELECT TOP 5
                b.BookID, b.Title, b.Author,
                ISNULL(b.Category, 'General') as Category,
                COUNT(t2.TransactionID) as PopularityScore
            FROM Books b
            INNER JOIN Transactions t2 ON b.BookID = t2.BookID
            WHERE b.Category IN (
                SELECT TOP 3 b2.Category
                FROM Transactions t
                INNER JOIN Books b2 ON t.BookID = b2.BookID
                WHERE t.StudentID = @StudentID
                AND b2.Category IS NOT NULL
                GROUP BY b2.Category
                ORDER BY COUNT(*) DESC
            )
            AND b.BookID NOT IN (
                SELECT BookID FROM Transactions WHERE StudentID = @StudentID
            )
            AND b.IsAvailable = 1
            GROUP BY b.BookID, b.Title, b.Author, b.Category
            ORDER BY PopularityScore DESC";

        SqlCommand cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        con.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            recommendations.Add(new
            {
                BookID = reader["BookID"],
                Title = reader["Title"],
                Author = reader["Author"],
                Category = reader["Category"],
                Score = reader["PopularityScore"]
            });
        }

        // Agar history nahi to popular books show karo
        if (recommendations.Count == 0)
        {
            reader.Close();
            string fallbackQuery = @"
                SELECT TOP 5
                    b.BookID, b.Title, b.Author,
                    ISNULL(b.Category, 'General') as Category,
                    COUNT(t.TransactionID) as PopularityScore
                FROM Books b
                LEFT JOIN Transactions t ON b.BookID = t.BookID
                WHERE b.IsAvailable = 1
                GROUP BY b.BookID, b.Title, b.Author, b.Category
                ORDER BY PopularityScore DESC";

            SqlCommand cmd2 = new SqlCommand(fallbackQuery, con);
            SqlDataReader reader2 = cmd2.ExecuteReader();
            while (reader2.Read())
            {
                recommendations.Add(new
                {
                    BookID = reader2["BookID"],
                    Title = reader2["Title"],
                    Author = reader2["Author"],
                    Category = reader2["Category"],
                    Score = reader2["PopularityScore"]
                });
            }
        }

        return Json(recommendations);
    }

    [Route("Index")]
    public IActionResult Index()
    {
        var studentId = HttpContext.Session.GetInt32("StudentID") ?? 0;
        ViewBag.StudentID = studentId;
        return View();
    }
}