
namespace DigitalLibrarySystem.Services;
 
using Microsoft.Data.SqlClient;

public class LoggingService
{
    private readonly string _connectionString = string.Empty;

    public LoggingService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    public void Log(int? userId, string action, string description, string ipAddress = "")
    {
        using SqlConnection con = new SqlConnection(_connectionString);
        string query = @"INSERT INTO ActivityLogs 
                       (UserID, Action, Description, IPAddress, LoggedAt) 
                       VALUES (@UserID, @Action, @Description, @IPAddress, GETDATE())";

        SqlCommand cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@UserID", userId.HasValue ? (object)userId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@Action", action);
        cmd.Parameters.AddWithValue("@Description", description);
        cmd.Parameters.AddWithValue("@IPAddress", ipAddress ?? "");

        con.Open();
        cmd.ExecuteNonQuery();
    }
}