namespace CellToSQL.Models;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSql,
    MySql,
    Sqlite
}

public enum ColumnDataType
{
    Integer,
    Decimal,
    DateTime,
    Text
}