namespace ZuluIA_Back.Application.Features.Reportes.DTOs;

public class DashboardReporteDto
{
    public string Titulo { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, decimal> Indicadores { get; set; } = new Dictionary<string, decimal>();
    public IReadOnlyList<DashboardSerieDto> Series { get; set; } = [];
}

public class DashboardSerieDto
{
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
