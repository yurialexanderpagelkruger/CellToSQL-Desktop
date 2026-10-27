namespace CellToSQL.Models;

public class ExtractedTable
{
    public string Name { get; set; } = "";
    public List<string> Columns { get; set; } = new();
    public List<List<object>> Rows { get; set; } = new();
}