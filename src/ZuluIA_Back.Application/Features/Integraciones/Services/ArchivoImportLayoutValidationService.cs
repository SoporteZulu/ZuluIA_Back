using ZuluIA_Back.Application.Features.Integraciones.DTOs;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ArchivoImportLayoutValidationService(ArchivoTabularParserService parserService, ArchivoImportLayoutProfileService layoutProfileService)
{
    public ArchivoLayoutValidationDto Validate(string circuito, string fileName, byte[] content, int maxRows = 20)
    {
        var analysis = parserService.Analyze(fileName, content, maxRows);
        var key = circuito.Trim().ToUpperInvariant();
        var normalizedRows = layoutProfileService.NormalizeRows(key, analysis.Rows);
        var normalizedColumns = normalizedRows.SelectMany(x => x.Keys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList().AsReadOnly();
        var requiredColumns = layoutProfileService.GetRequiredColumns(key);
        if (requiredColumns.Count == 0)
        {
            return new ArchivoLayoutValidationDto
            {
                Circuito = key,
                FileName = fileName,
                LayoutProfile = analysis.LayoutProfile,
                IsValid = false,
                RequiredColumns = [],
                MissingColumns = [],
                ExtraColumns = normalizedColumns,
                Issues = ["Circuito de importación no soportado para validación de layout."]
            };
        }

        var missing = requiredColumns.Where(x => !normalizedColumns.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList().AsReadOnly();
        var extras = normalizedColumns.Where(x => !requiredColumns.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList().AsReadOnly();
        var issues = new List<string>();
        if (missing.Count > 0)
            issues.Add($"Faltan columnas obligatorias: {string.Join(", ", missing)}.");
        if (analysis.TotalRows == 0)
            issues.Add("El archivo no contiene filas de datos.");

        return new ArchivoLayoutValidationDto
        {
            Circuito = key,
            FileName = fileName,
            LayoutProfile = analysis.LayoutProfile,
            IsValid = missing.Count == 0 && analysis.TotalRows > 0,
            RequiredColumns = requiredColumns,
            MissingColumns = missing,
            ExtraColumns = extras,
            Issues = issues.AsReadOnly()
        };
    }
}
