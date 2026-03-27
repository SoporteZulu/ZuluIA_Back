namespace ZuluIA_Back.Application.Features.Reportes.DTOs;

public class ReporteTabularDto
{
    public string Titulo { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Parametros { get; set; } = new Dictionary<string, string>();
    public IReadOnlyList<string> Columnas { get; set; } = [];
    public IReadOnlyList<IReadOnlyList<string>> Filas { get; set; } = [];
}
