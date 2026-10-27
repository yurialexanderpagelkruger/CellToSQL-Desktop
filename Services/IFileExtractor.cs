using CellToSQL.Models;

namespace CellToSQL.Services;

public interface IFileExtractor
{
    List<ExtractedTable> Extract(string filePath);
}