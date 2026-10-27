using CellToSQL.Models;

namespace CellToSQL.Services;

public static class FileExtractorService
{
    public static List<ExtractedTable> Extract(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        IFileExtractor extractor = ext switch
        {
            ".xlsx" or ".xlsm" => new ExcelExtractor(),
            ".csv" or ".txt" or ".tsv" or ".log" => new CsvExtractor(),
            ".pdf" => new PdfExtractor(),
            ".docx" => new WordExtractor(),
            _ => throw new NotSupportedException("Formato no soportado: " + ext)
        };
        return extractor.Extract(filePath);
    }

    public static bool IsSupported(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".xlsx" or ".xlsm" or ".csv" or ".txt" or ".tsv" or ".log" or ".pdf" or ".docx";
    }
}