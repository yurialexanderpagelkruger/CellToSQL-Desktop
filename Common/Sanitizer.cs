using System.Text;

namespace CellToSQL.Common;

public static class Sanitizer
{
    public static string Identifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "col";
        var sb = new StringBuilder();
        foreach (char ch in name.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch == '_') sb.Append(ch);
            else sb.Append('_');
        }
        string s = sb.ToString().Trim('_');
        if (string.IsNullOrEmpty(s)) s = "col";
        if (char.IsDigit(s[0])) s = "c_" + s;
        if (s.Length > 64) s = s.Substring(0, 64);
        return s;
    }

    public static List<string> UniqueHeaders(List<string> raw)
    {
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        foreach (string r in raw)
        {
            string baseName = Identifier(string.IsNullOrWhiteSpace(r) ? "col" : r);
            if (seen.TryGetValue(baseName, out int count))
            {
                count++;
                seen[baseName] = count;
                result.Add(baseName + "_" + count);
            }
            else
            {
                seen[baseName] = 1;
                result.Add(baseName);
            }
        }
        return result;
    }
}