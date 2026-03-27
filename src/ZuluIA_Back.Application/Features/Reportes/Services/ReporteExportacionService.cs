using System.Text;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;

namespace ZuluIA_Back.Application.Features.Reportes.Services;

public class ReporteExportacionService
{
    public ExportacionReporteDto Exportar(ReporteTabularDto reporte, FormatoExportacionReporte formato, string nombreBase, string? layoutProfile = null)
    {
        var resolvedLayoutProfile = ResolveLayoutProfile(formato, layoutProfile);
        return formato switch
        {
            FormatoExportacionReporte.Csv => new ExportacionReporteDto
            {
                NombreArchivo = $"{nombreBase}.csv",
                ContentType = "text/csv",
                LayoutProfile = resolvedLayoutProfile,
                Contenido = Encoding.UTF8.GetBytes(GenerarCsv(reporte))
            },
            FormatoExportacionReporte.Xls => new ExportacionReporteDto
            {
                NombreArchivo = $"{nombreBase}.xls",
                ContentType = "application/vnd.ms-excel",
                LayoutProfile = resolvedLayoutProfile,
                Contenido = Encoding.UTF8.GetBytes(GenerarHtmlTable(reporte))
            },
            _ => new ExportacionReporteDto
            {
                NombreArchivo = $"{nombreBase}.pdf",
                ContentType = "application/pdf",
                LayoutProfile = resolvedLayoutProfile,
                Contenido = GenerarPdfSimple(reporte, resolvedLayoutProfile)
            }
        };
    }

    private static string GenerarCsv(ReporteTabularDto reporte)
    {
        var sb = new StringBuilder();
        foreach (var parametro in reporte.Parametros)
            sb.AppendLine($"{EscaparCsv(parametro.Key)},{EscaparCsv(parametro.Value)}");

        if (reporte.Parametros.Count > 0)
            sb.AppendLine();

        sb.AppendLine(string.Join(',', reporte.Columnas.Select(EscaparCsv)));
        foreach (var fila in reporte.Filas)
            sb.AppendLine(string.Join(',', fila.Select(EscaparCsv)));

        return sb.ToString();
    }

    private static string GenerarHtmlTable(ReporteTabularDto reporte)
    {
        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset=\"utf-8\" /></head><body>");
        sb.Append($"<h2>{System.Net.WebUtility.HtmlEncode(reporte.Titulo)}</h2>");
        if (reporte.Parametros.Count > 0)
        {
            sb.Append("<ul>");
            foreach (var parametro in reporte.Parametros)
                sb.Append($"<li><b>{System.Net.WebUtility.HtmlEncode(parametro.Key)}:</b> {System.Net.WebUtility.HtmlEncode(parametro.Value)}</li>");
            sb.Append("</ul>");
        }

        sb.Append("<table border=\"1\"><thead><tr>");
        foreach (var columna in reporte.Columnas)
            sb.Append($"<th>{System.Net.WebUtility.HtmlEncode(columna)}</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var fila in reporte.Filas)
        {
            sb.Append("<tr>");
            foreach (var valor in fila)
                sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(valor)}</td>");
            sb.Append("</tr>");
        }

        sb.Append("</tbody></table></body></html>");
        return sb.ToString();
    }

    private static byte[] GenerarPdfSimple(ReporteTabularDto reporte, string layoutProfile)
    {
        var (fontSize, lineHeight, pageWidth, titlePrefix) = layoutProfile switch
        {
            "COMPACT" => (8, 10, 595, "[COMPACT] "),
            "WIDE" => (9, 11, 842, "[WIDE] "),
            "LEGACY_TEXT" => (10, 12, 842, "[LEGACY] "),
            _ => (10, 12, 595, string.Empty)
        };

        var lineas = new List<string> { $"{titlePrefix}{reporte.Titulo}" };
        lineas.Add($"LayoutProfile: {layoutProfile}");
        lineas.AddRange(reporte.Parametros.Select(x => $"{x.Key}: {x.Value}"));
        if (reporte.Parametros.Count > 0)
            lineas.Add(string.Empty);
        var separator = layoutProfile == "LEGACY_TEXT" ? " ; " : " | ";
        lineas.Add(string.Join(separator, reporte.Columnas));
        lineas.AddRange(reporte.Filas.Select(f => string.Join(separator, f)));

        var contenidoTexto = string.Join("\n", lineas)
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");

        var stream = $"BT /F1 {fontSize} Tf 40 780 Td {lineHeight} TL {string.Join(" ", contenidoTexto.Split('\n').Select((linea, i) => i == 0 ? $"({linea}) Tj" : $"T* ({linea}) Tj"))} ET";
        var objects = new[]
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj",
            $"3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj",
            "4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj",
            $"5 0 obj << /Length {Encoding.ASCII.GetByteCount(stream)} >> stream\n{stream}\nendstream endobj"
        };

        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");
        var offsets = new List<int>();
        foreach (var obj in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
            sb.AppendLine(obj);
        }

        var xrefPos = Encoding.ASCII.GetByteCount(sb.ToString());
        sb.AppendLine($"xref\n0 {objects.Length + 1}\n0000000000 65535 f ");
        foreach (var offset in offsets)
            sb.AppendLine($"{offset:D10} 00000 n ");
        sb.AppendLine($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF");
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static string ResolveLayoutProfile(FormatoExportacionReporte formato, string? layoutProfile)
        => string.IsNullOrWhiteSpace(layoutProfile)
            ? formato == FormatoExportacionReporte.Pdf ? "DEFAULT" : "TABULAR_DEFAULT"
            : layoutProfile.Trim().ToUpperInvariant();

    private static string EscaparCsv(string? value)
    {
        var texto = value ?? string.Empty;
        if (texto.IndexOfAny([',', '"', '\n', '\r']) >= 0)
            return $"\"{texto.Replace("\"", "\"\"")}\"";
        return texto;
    }
}
