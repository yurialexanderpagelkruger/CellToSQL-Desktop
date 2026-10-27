using CellToSQL.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CellToSQL.Services;

public class WordExtractor : IFileExtractor
{
    public List<ExtractedTable> Extract(string filePath)
    {
        var tables = new List<ExtractedTable>();
        var paragraphs = new List<string>();

        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart.Document.Body;
        int tableIndex = 0;
        foreach (var element in body.Elements())
        {
            if (element is Table table)
            {
                tableIndex++;
                tables.Add(ReadTable(table, tableIndex));
            }
            else if (element is Paragraph para)
            {
                string text = para.InnerText;
                if (!string.IsNullOrWhiteSpace(text))
                    paragraphs.Add(text.Trim());
            }
        }

        var content = new ExtractedTable
        {
            Name = "",
            Columns = new List<string> { "paragraph", "text" }
        };
        for (int i = 0; i < paragraphs.Count; i++)
            content.Rows.Add(new List<object> { i + 1, paragraphs[i] });

        var result = new List<ExtractedTable>();
        foreach (var t in tables) result.Add(t);
        if (paragraphs.Count > 0) result.Add(content);
        return result;
    }

    private static ExtractedTable ReadTable(Table table, int index)
    {
        var extracted = new ExtractedTable { Name = "table_" + index };
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0) return extracted;

        var headerCells = rows[0].Elements<TableCell>().ToList();
        extracted.Columns = headerCells.Select(c => GetCellText(c)).ToList();

        for (int i = 1; i < rows.Count; i++)
        {
            var cells = rows[i].Elements<TableCell>().ToList();
            extracted.Rows.Add(cells.Select(c => (object)GetCellText(c)).ToList());
        }
        return extracted;
    }

    private static string GetCellText(TableCell cell)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var p in cell.Descendants<Paragraph>())
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(p.InnerText);
        }
        return sb.ToString().Trim();
    }
}