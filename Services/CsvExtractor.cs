using CellToSQL.Models;
using System.Text;

namespace CellToSQL.Services;

public class CsvExtractor : IFileExtractor
{
    public List<ExtractedTable> Extract(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        var nonEmpty = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (nonEmpty.Count == 0) return new List<ExtractedTable>();

        char delimiter = DetectDelimiter(nonEmpty[0]);

        var table = new ExtractedTable { Name = "" };
        bool header = true;
        foreach (string line in nonEmpty)
        {
            var values = ParseLine(line, delimiter);
            if (header)
            {
                table.Columns = values;
                header = false;
            }
            else
            {
                table.Rows.Add(values.Select(v => (object)v).ToList());
            }
        }
        return new List<ExtractedTable> { table };
    }

    private static char DetectDelimiter(string sample)
    {
        char[] candidates = { ',', ';', '\t', '|' };
        char best = ',';
        int bestCount = -1;
        foreach (char c in candidates)
        {
            int count = sample.Count(ch => ch == c);
            if (count > bestCount) { bestCount = count; best = c; }
        }
        return best;
    }

    private static List<string> ParseLine(string line, char delimiter)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == delimiter && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(ch);
            }
        }
        result.Add(sb.ToString());
        return result;
    }
}