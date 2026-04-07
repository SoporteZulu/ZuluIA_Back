namespace ZuluIA_Back.Application.Features.Reportes.DTOs;

public class ExportacionReporteDto
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? LayoutProfile { get; set; }
    public byte[] Contenido { get; set; } = [];
}
