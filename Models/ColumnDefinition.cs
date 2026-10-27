namespace CellToSQL.Models;

public class ColumnDefinition
{
    public string Name { get; set; } = "";
    public ColumnDataType Type { get; set; } = ColumnDataType.Text;
}