using System.Data;
using CellToSQL.Models;

namespace CellToSQL.Services;

public class DatabaseService
{
    private readonly ConnectionSettings _s;

    public DatabaseService(ConnectionSettings settings)
    {
        _s = settings;
    }

    public string BuildConnectionString()
    {
        switch (_s.Provider)
        {
            case DatabaseProvider.SqlServer:
                if (_s.IntegratedSecurity)
                    return $"Server={_s.Server},{_s.Port};Database={_s.Database};Trusted_Connection=True;TrustServerCertificate=True;";
                return $"Server={_s.Server},{_s.Port};Database={_s.Database};User Id={_s.User};Password={_s.Password};TrustServerCertificate=True;";
            case DatabaseProvider.PostgreSql:
                return $"Host={_s.Server};Port={_s.Port};Database={_s.Database};Username={_s.User};Password={_s.Password};";
            case DatabaseProvider.MySql:
                return $"Server={_s.Server};Port={_s.Port};Database={_s.Database};User ID={_s.User};Password={_s.Password};";
            case DatabaseProvider.Sqlite:
                return $"Data Source={_s.SqliteFile};";
            default:
                throw new NotSupportedException();
        }
    }

    public IDbConnection CreateConnection()
    {
        string cs = BuildConnectionString();
        switch (_s.Provider)
        {
            case DatabaseProvider.SqlServer: return new Microsoft.Data.SqlClient.SqlConnection(cs);
            case DatabaseProvider.PostgreSql: return new Npgsql.NpgsqlConnection(cs);
            case DatabaseProvider.MySql: return new MySqlConnector.MySqlConnection(cs);
            case DatabaseProvider.Sqlite: return new Microsoft.Data.Sqlite.SqliteConnection(cs);
            default: throw new NotSupportedException();
        }
    }

    public void TestConnection()
    {
        using var c = CreateConnection();
        c.Open();
        c.Close();
    }

    private string Quote(string name)
    {
        return _s.Provider switch
        {
            DatabaseProvider.SqlServer => $"[{name}]",
            DatabaseProvider.MySql => $"`{name}`",
            _ => $"\"{name}\""
        };
    }

    private string SqlType(ColumnDataType t)
    {
        switch (_s.Provider)
        {
            case DatabaseProvider.SqlServer:
                return t switch
                {
                    ColumnDataType.Integer => "BIGINT",
                    ColumnDataType.Decimal => "DECIMAL(28,6)",
                    ColumnDataType.DateTime => "DATETIME2",
                    _ => "NVARCHAR(MAX)"
                };
            case DatabaseProvider.MySql:
                return t switch
                {
                    ColumnDataType.Integer => "BIGINT",
                    ColumnDataType.Decimal => "DECIMAL(28,6)",
                    ColumnDataType.DateTime => "DATETIME",
                    _ => "LONGTEXT"
                };
            case DatabaseProvider.PostgreSql:
                return t switch
                {
                    ColumnDataType.Integer => "BIGINT",
                    ColumnDataType.Decimal => "NUMERIC(28,6)",
                    ColumnDataType.DateTime => "TIMESTAMP",
                    _ => "TEXT"
                };
            case DatabaseProvider.Sqlite:
                return t switch
                {
                    ColumnDataType.Integer => "INTEGER",
                    ColumnDataType.Decimal => "REAL",
                    ColumnDataType.DateTime => "TEXT",
                    _ => "TEXT"
                };
            default:
                throw new NotSupportedException();
        }
    }

    public void CreateTable(IDbConnection conn, IDbTransaction tx, TableDefinition table)
    {
        string drop = $"DROP TABLE IF EXISTS {Quote(table.Name)}";
        string cols = string.Join(", ", table.Columns.Select(c => $"{Quote(c.Name)} {SqlType(c.Type)}"));
        string create = $"CREATE TABLE {Quote(table.Name)} ({cols})";
        Exec(conn, tx, drop);
        Exec(conn, tx, create);
    }

    public void InsertRows(IDbConnection conn, IDbTransaction tx, TableDefinition table, List<List<object>> rows)
    {
        var names = table.Columns.Select(c => c.Name).ToList();
        string colList = string.Join(", ", names.Select(Quote));
        string valList = string.Join(", ", names.Select((_, i) => $"@p{i}"));
        string sql = $"INSERT INTO {Quote(table.Name)} ({colList}) VALUES ({valList})";

        foreach (var row in rows)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = $"@p{i}";
                object raw = i < row.Count ? row[i] : null;
                p.Value = Normalize(raw, table.Columns[i].Type);
                cmd.Parameters.Add(p);
            }
            cmd.ExecuteNonQuery();
        }
    }

    private static object Normalize(object v, ColumnDataType t)
    {
        if (v == null || v == DBNull.Value) return DBNull.Value;
        if (v is string s && string.IsNullOrWhiteSpace(s)) return DBNull.Value;
        try
        {
            switch (t)
            {
                case ColumnDataType.Integer:
                    return Convert.ToInt64(v, System.Globalization.CultureInfo.InvariantCulture);
                case ColumnDataType.Decimal:
                    return Convert.ToDecimal(v, System.Globalization.CultureInfo.InvariantCulture);
                case ColumnDataType.DateTime:
                    return v is DateTime d ? d : DateTime.Parse(v.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                default:
                    return v.ToString();
            }
        }
        catch
        {
            return v.ToString();
        }
    }

    private static void Exec(IDbConnection conn, IDbTransaction tx, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}