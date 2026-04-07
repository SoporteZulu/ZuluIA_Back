namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ExternalProviderErrorCatalogDto
{
    public string Proveedor { get; init; } = string.Empty;
    public IReadOnlyList<ExternalProviderErrorCatalogItemDto> Items { get; init; } = [];
}

public class ExternalProviderErrorCatalogItemDto
{
    public string Codigo { get; init; } = string.Empty;
    public bool ErrorFuncional { get; init; }
    public string Mensaje { get; init; } = string.Empty;
}
