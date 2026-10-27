using System.Data;
using CellToSQL.Common;
using CellToSQL.Models;

namespace CellToSQL.Services;

public class ImportService
{
    private readonly DatabaseService _db;

    public ImportService(ConnectionSettings settings)
    {
        _db = new DatabaseService(settings);
    }

    public ImportSummary Import(List<string> filePaths, Action<string> log, CancellationToken ct)
    {
        var summary = new ImportSummary();
        using var conn = _db.CreateConnection();
        conn.Open();

        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string file in filePaths)
        {
            ct.ThrowIfCancellationRequested();
            string fileName = Path.GetFileNameWithoutExtension(file);
            log("Procesando: " + file);
            var extractedTables = FileExtractorService.Extract(file);

            foreach (var ext in extractedTables)
            {
                ct.ThrowIfCancellationRequested();
                string baseName = string.IsNullOrWhiteSpace(ext.Name) ? fileName : fileName + "_" + ext.Name;
                string tableName = EnsureUnique(usedNames, baseName);
                var def = SchemaBuilder.Build(ext, tableName);
                def.Name = tableName;
                usedNames.Add(tableName);

                if (def.Columns.Count == 0)
                {
                    log("  Saltando tabla vacia: " + tableName);
                    continue;
                }

                using var tx = conn.BeginTransaction();
                try
                {
                    _db.CreateTable(conn, tx, def);
                    _db.InsertRows(conn, tx, def, ext.Rows);
                    tx.Commit();
                    summary.TablesImported++;
                    summary.RowsImported += ext.Rows.Count;
                    log("  Tabla '" + tableName + "': " + def.Columns.Count + " columnas, " + ext.Rows.Count + " filas.");
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        return summary;
    }

    private static string EnsureUnique(HashSet<string> used, string baseName)
    {
        string name = Sanitizer.Identifier(baseName);
        if (!used.Contains(name)) return name;
        int i = 2;
        while (used.Contains(name + "_" + i)) i++;
        return name + "_" + i;
    }
}