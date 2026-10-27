using System.Globalization;
using CellToSQL.Common;
using CellToSQL.Models;

namespace CellToSQL.Services;

public static class SchemaBuilder
{
    public static TableDefinition Build(ExtractedTable table, string fallbackTableName)
    {
        int maxWidth = table.Columns.Count;
        foreach (var row in table.Rows)
            if (row.Count > maxWidth) maxWidth = row.Count;

        var rawHeaders = new List<string>();
        for (int i = 0; i < maxWidth; i++)
            rawHeaders.Add(i < table.Columns.Count ? table.Columns[i] : "col" + (i + 1));

        var headers = Sanitizer.UniqueHeaders(rawHeaders);

        var def = new TableDefinition { Name = fallbackTableName, Columns = new List<ColumnDefinition>() };
        for (int c = 0; c < maxWidth; c++)
        {
            var values = new List<object>();
            foreach (var row in table.Rows)
                if (row.Count > c) values.Add(row[c]);
            def.Columns.Add(new ColumnDefinition { Name = headers[c], Type = InferType(values) });
        }
        return def;
    }

    private static ColumnDataType InferType(List<object> values)
    {
        var nonNull = new List<object>();
        foreach (var v in values)
        {
            if (v == null) continue;
            if (v is string s && string.IsNullOrWhiteSpace(s)) continue;
            nonNull.Add(v);
        }
        if (nonNull.Count == 0) return ColumnDataType.Text;

        bool allInt = true, allDec = true, allDate = true;
        foreach (var v in nonNull)
        {
            string sv = v is DateTime dt ? dt.ToString("O") : v.ToString();
            if (!long.TryParse(sv, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)) allInt = false;
            if (!decimal.TryParse(sv, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) allDec = false;
            if (!DateTime.TryParse(sv, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) allDate = false;
        }
        if (allInt) return ColumnDataType.Integer;
        if (allDec) return ColumnDataType.Decimal;
        if (allDate) return ColumnDataType.DateTime;
        return ColumnDataType.Text;
    }
}