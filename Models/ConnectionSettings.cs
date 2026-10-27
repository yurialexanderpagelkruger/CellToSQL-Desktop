namespace CellToSQL.Models;

public class ConnectionSettings
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 1433;
    public string Database { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string SqliteFile { get; set; } = "";
    public bool IntegratedSecurity { get; set; } = false;
}