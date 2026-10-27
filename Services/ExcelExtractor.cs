using CellToSQL.Models;
using ClosedXML.Excel;

namespace CellToSQL.Services;

public class ExcelExtractor : IFileExtractor
{
    public List<ExtractedTable> Extract(string filePath)
    {
        var tables = new List<ExtractedTable>();
        using var wb = new XLWorkbook(filePath);
        foreach (var ws in wb.Worksheets)
        {
            var range = ws.RangeUsed();
            if (range == null) continue;

            int colCount = range.ColumnCount();
            var table = new ExtractedTable { Name = ws.Name };
            bool header = true;
            foreach (var row in range.RowsUsed())
            {
                var values = new List<object>();
                for (int c = 1; c <= colCount; c++)
                    values.Add(ReadCell(row.Cell(c)));
                if (header)
                {
                    table.Columns = values.Select(v => v?.ToString()).ToList();
                    header = false;
                }
                else
                {
                    table.Rows.Add(values);
                }
            }
            tables.Add(table);
        }
        return tables;
    }

    private static object ReadCell(IXLCell cell)
    {
        var v = cell.Value;
        if (v.IsBlank) return null;
        if (v.IsNumber) return v.GetNumber();
        if (v.IsDateTime) return v.GetDateTime();
        if (v.IsBoolean) return v.GetBoolean();
        if (v.IsTimeSpan) return v.GetTimeSpan();
        return v.GetText();
    }
}