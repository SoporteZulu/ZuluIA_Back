namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ExternalIntegrationCertificationDto
{
    public string Proveedor { get; init; } = string.Empty;
    public string Ambiente { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public bool Habilitada { get; init; }
    public bool UsaTransporteReal { get; init; }
    public bool ConfiguracionValida { get; init; }
    public bool CredencialesCompletas { get; init; }
    public bool ListoParaProduccion { get; init; }
    public int RegistrosConfigurados { get; init; }
    public int ErroresRecientes { get; init; }
    public int ErroresFuncionalesRecientes { get; init; }
    public IReadOnlyList<string> Issues { get; init; } = [];
}

public class ExternalIntegrationConnectivityDto
{
    public string Proveedor { get; init; } = string.Empty;
    public string Ambiente { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public bool Habilitada { get; init; }
    public bool UsaTransporteReal { get; init; }
    public bool ConfiguracionValida { get; init; }
    public bool CredencialesCompletas { get; init; }
    public bool ConectividadOk { get; init; }
    public int? HttpStatusCode { get; init; }
    public string? Mensaje { get; init; }
    public long DuracionMs { get; init; }
    public IReadOnlyList<string> Issues { get; init; } = [];
}
