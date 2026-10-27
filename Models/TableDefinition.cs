namespace CellToSQL.Models;

public class TableDefinition
{
    public string Name { get; set; } = "";
    public List<ColumnDefinition> Columns { get; set; } = new();
}