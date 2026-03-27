using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ArchivoTabularParserService
{
    private static readonly IReadOnlyList<string> SupportedFormats = [".csv", ".txt", ".tsv", ".xlsx"];

    public IReadOnlyList<IReadOnlyDictionary<string, string?>> Parse(string fileName, byte[] contenido)
    {
        var ext = Path.GetExtension(fileName)?.Trim().ToLowerInvariant();
        return ext switch
        {
            ".csv" or ".txt" or ".tsv" => ParseDelimited(contenido, ext),
            ".xlsx" => ParseXlsx(contenido),
            _ => throw new InvalidOperationException("Formato de archivo no soportado. Use CSV o XLSX.")
        };
    }

    public ArchivoTabularAnalisisDto Analyze(string fileName, byte[] contenido, int maxRows = 20)
    {
        var ext = Path.GetExtension(fileName)?.Trim().ToLowerInvariant() ?? string.Empty;
        var rows = Parse(fileName, contenido);
        var preview = rows.Take(Math.Max(1, maxRows)).ToList().AsReadOnly();
        var separator = ext == ".xlsx"
            ? null
            : DetectarSeparador(Encoding.UTF8.GetString(contenido), ext).ToString();

        return new ArchivoTabularAnalisisDto
        {
            FileName = fileName,
            Format = ext.TrimStart('.').ToUpperInvariant(),
            LayoutProfile = ResolveLayoutProfile(ext, separator),
            Separator = separator,
            TotalRows = rows.Count,
            PreviewRows = preview.Count,
            Columns = preview.SelectMany(x => x.Keys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList().AsReadOnly(),
            Rows = preview,
            SupportedFormats = SupportedFormats
        };
    }

    public IReadOnlyList<string> GetSupportedFormats() => SupportedFormats;

    private static IReadOnlyList<IReadOnlyDictionary<string, string?>> ParseDelimited(byte[] contenido, string? ext)
    {
        var texto = Encoding.UTF8.GetString(contenido);
        var separador = DetectarSeparador(texto, ext);
        var filas = ParseDelimitedLines(texto, separador);
        return ToRows(filas);
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string?>> ParseXlsx(byte[] contenido)
    {
        using var ms = new MemoryStream(contenido);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, false);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        var sharedStrings = new List<string>();
        var sharedEntry = zip.GetEntry("xl/sharedStrings.xml");
        if (sharedEntry is not null)
        {
            using var sharedStream = sharedEntry.Open();
            var sharedDoc = XDocument.Load(sharedStream);
            sharedStrings = sharedDoc.Descendants(ns + "si")
                .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value)))
                .ToList();
        }

        var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml")
            ?? zip.Entries.FirstOrDefault(x => x.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase) && x.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("No se encontró una hoja XLSX válida.");

        using var sheetStream = sheetEntry.Open();
        var sheetDoc = XDocument.Load(sheetStream);
        var rows = new List<List<string?>>();

        foreach (var row in sheetDoc.Descendants(ns + "row"))
        {
            var values = new SortedDictionary<int, string?>();
            foreach (var cell in row.Elements(ns + "c"))
            {
                var reference = cell.Attribute("r")?.Value ?? string.Empty;
                var columnIndex = GetColumnIndex(reference);
                var cellType = cell.Attribute("t")?.Value;
                var value = cell.Element(ns + "v")?.Value;
                var inline = cell.Element(ns + "is");

                string? resolved = null;
                if (cellType == "s" && int.TryParse(value, out var sharedIndex) && sharedIndex >= 0 && sharedIndex < sharedStrings.Count)
                    resolved = sharedStrings[sharedIndex];
                else if (cellType == "inlineStr")
                    resolved = inline?.Descendants(ns + "t").Select(t => t.Value).FirstOrDefault();
                else
                    resolved = value;

                values[columnIndex] = resolved;
            }

            if (values.Count == 0)
                continue;

            var maxIndex = values.Keys.Max();
            var fila = new List<string?>(Enumerable.Repeat<string?>(null, maxIndex + 1));
            foreach (var kv in values)
                fila[kv.Key] = kv.Value;
            rows.Add(fila);
        }

        return ToRows(rows);
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string?>> ToRows(IReadOnlyList<List<string?>> filas)
    {
        if (filas.Count == 0)
            return [];

        var headers = filas[0].Select(x => (x ?? string.Empty).Trim()).ToList();
        var result = new List<IReadOnlyDictionary<string, string?>>();

        foreach (var fila in filas.Skip(1))
        {
            if (fila.All(string.IsNullOrWhiteSpace))
                continue;

            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
            {
                var key = headers[i];
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                dict[key] = i < fila.Count ? fila[i]?.Trim() : null;
            }

            result.Add(dict);
        }

        return result;
    }

    private static List<List<string?>> ParseDelimitedLines(string text, char separator)
    {
        var rows = new List<List<string?>>();
        var row = new List<string?>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(ch);
                }
                continue;
            }

            if (ch == '"')
            {
                inQuotes = true;
                continue;
            }

            if (ch == separator)
            {
                row.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            if (ch == '\r')
                continue;

            if (ch == '\n')
            {
                row.Add(sb.ToString());
                sb.Clear();
                rows.Add(row);
                row = new List<string?>();
                continue;
            }

            sb.Append(ch);
        }

        row.Add(sb.ToString());
        rows.Add(row);
        return rows;
    }

    private static char DetectarSeparador(string text, string? ext)
    {
        if (ext == ".tsv")
            return '\t';

        var primeraLinea = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim('\r'))
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;

        var candidates = new[] { ',', ';', '\t', '|' };
        return candidates
            .Select(x => new { Separator = x, Count = CountOutsideQuotes(primeraLinea, x) })
            .OrderByDescending(x => x.Count)
            .First().Separator;
    }

    private static int CountOutsideQuotes(string text, char separator)
    {
        var inQuotes = false;
        var count = 0;
        foreach (var ch in text)
        {
            if (ch == '"')
                inQuotes = !inQuotes;
            else if (!inQuotes && ch == separator)
                count++;
        }

        return count;
    }

    private static int GetColumnIndex(string cellReference)
    {
        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray()).ToUpperInvariant();
        var sum = 0;
        foreach (var c in letters)
            sum = (sum * 26) + (c - 'A' + 1);
        return Math.Max(0, sum - 1);
    }

    private static string ResolveLayoutProfile(string ext, string? separator)
        => ext switch
        {
            ".xlsx" => "XLSX_STANDARD",
            ".tsv" => "TAB_DELIMITED",
            _ when separator == ";" => "CSV_SEMICOLON",
            _ when separator == "|" => "LEGACY_PIPE",
            _ when separator == "\t" => "TAB_DELIMITED",
            _ => "CSV_COMMA"
        };
}
