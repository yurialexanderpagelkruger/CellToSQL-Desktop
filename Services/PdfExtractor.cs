using System.IO.Compression;
using System.Text;
using CellToSQL.Models;

namespace CellToSQL.Services;

public class PdfExtractor : IFileExtractor
{
    public List<ExtractedTable> Extract(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        string raw = Encoding.Latin1.GetString(data);

        var table = new ExtractedTable
        {
            Name = "",
            Columns = new List<string> { "page", "line", "text" }
        };

        int page = 0;
        foreach (string content in ReadContentStreams(raw, data))
        {
            string text = ExtractText(content);
            if (string.IsNullOrWhiteSpace(text)) continue;
            page++;
            var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int lineNo = 0;
            foreach (string rawLine in lines)
            {
                string line = CollapseSpaces(rawLine);
                if (line.Length == 0) continue;
                lineNo++;
                table.Rows.Add(new List<object> { page, lineNo, line });
            }
        }

        return new List<ExtractedTable> { table };
    }

    private static IEnumerable<string> ReadContentStreams(string raw, byte[] data)
    {
        var result = new List<string>();
        int search = 0;
        while (true)
        {
            int s = raw.IndexOf("stream", search, StringComparison.Ordinal);
            if (s < 0) break;
            int contentStart = s + 6;
            if (contentStart < raw.Length && raw[contentStart] == '\r') contentStart++;
            if (contentStart < raw.Length && raw[contentStart] == '\n') contentStart++;
            int e = raw.IndexOf("endstream", contentStart, StringComparison.Ordinal);
            if (e < 0) break;

            int dictStart = raw.LastIndexOf("<<", s, StringComparison.Ordinal);
            string dict = dictStart >= 0 && dictStart < s ? raw.Substring(dictStart, s - dictStart) : "";

            bool skip = dict.Contains("/Subtype") || dict.Contains("/Type /ObjStm")
                        || dict.Contains("/Type /XRef") || dict.Contains("/Type /Metadata")
                        || dict.Contains("/Type /Font");

            if (!skip && e > contentStart)
            {
                byte[] bytes = new byte[e - contentStart];
                Array.Copy(data, contentStart, bytes, 0, bytes.Length);
                bool flate = dict.Contains("FlateDecode");
                bool hasFilter = dict.Contains("/Filter");

                if (flate)
                {
                    byte[] dec = TryInflate(bytes);
                    if (dec != null) result.Add(Encoding.Latin1.GetString(dec));
                }
                else if (!hasFilter)
                {
                    result.Add(Encoding.Latin1.GetString(bytes));
                }
            }
            search = e + 9;
        }
        return result;
    }

    private static byte[] TryInflate(byte[] data)
    {
        try
        {
            using var input = new MemoryStream(data);
            if (data.Length > 2 && data[0] == 0x78) input.Position = 2;
            using var ds = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            ds.CopyTo(output);
            return output.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractText(string content)
    {
        var sb = new StringBuilder();
        int i = 0;
        int n = content.Length;
        while (i < n)
        {
            char c = content[i];
            if (c == '(')
            {
                i++;
                sb.Append(ReadLiteral(content, ref i));
            }
            else if (c == '<' && i + 1 < n && content[i + 1] != '<')
            {
                int end = content.IndexOf('>', i + 1);
                if (end < 0) { i++; continue; }
                string hex = content.Substring(i + 1, end - i - 1);
                sb.Append(HexToString(hex));
                i = end + 1;
            }
            else if (char.IsLetter(c) || c == '\'' || c == '"')
            {
                int start = i;
                while (i < n && (char.IsLetter(content[i]) || content[i] == '*' || content[i] == '\'' || content[i] == '"'))
                    i++;
                string op = content.Substring(start, i - start);
                if (op == "Tj" || op == "TJ" || op == "'" || op == "\"")
                    sb.Append(' ');
                else if (op == "Td" || op == "TD" || op == "T*" || op == "ET")
                    sb.Append('\n');
            }
            else
            {
                i++;
            }
        }
        return sb.ToString();
    }

    private static string ReadLiteral(string s, ref int i)
    {
        var sb = new StringBuilder();
        while (i < s.Length)
        {
            char c = s[i];
            if (c == '\\')
            {
                i++;
                if (i >= s.Length) break;
                char e = s[i];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case '(': sb.Append('('); break;
                    case ')': sb.Append(')'); break;
                    case '\\': sb.Append('\\'); break;
                    default:
                        if (e >= '0' && e <= '7')
                        {
                            int val = e - '0';
                            int count = 1;
                            while (count < 3 && i + 1 < s.Length && s[i + 1] >= '0' && s[i + 1] <= '7')
                            {
                                i++;
                                val = val * 8 + (s[i] - '0');
                                count++;
                            }
                            sb.Append((char)val);
                        }
                        else
                        {
                            sb.Append(e);
                        }
                        break;
                }
                i++;
            }
            else if (c == ')')
            {
                i++;
                break;
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }
        return sb.ToString();
    }

    private static string HexToString(string hex)
    {
        var bytes = new List<byte>();
        int hi = -1;
        foreach (char ch in hex)
        {
            int v = HexVal(ch);
            if (v < 0) continue;
            if (hi < 0) hi = v;
            else
            {
                bytes.Add((byte)((hi << 4) | v));
                hi = -1;
            }
        }
        if (hi >= 0) bytes.Add((byte)hi);
        return Encoding.Latin1.GetString(bytes.ToArray());
    }

    private static int HexVal(char ch)
    {
        if (ch >= '0' && ch <= '9') return ch - '0';
        if (ch >= 'a' && ch <= 'f') return ch - 'a' + 10;
        if (ch >= 'A' && ch <= 'F') return ch - 'A' + 10;
        return -1;
    }

    private static string CollapseSpaces(string line)
    {
        var sb = new StringBuilder();
        bool space = false;
        foreach (char ch in line)
        {
            if (ch == ' ' || ch == '\t')
            {
                if (!space) { sb.Append(' '); space = true; }
            }
            else if (!char.IsControl(ch))
            {
                sb.Append(ch);
                space = false;
            }
        }
        return sb.ToString().Trim();
    }
}