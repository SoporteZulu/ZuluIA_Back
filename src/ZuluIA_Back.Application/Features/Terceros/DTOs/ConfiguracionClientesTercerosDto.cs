namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

/// <summary>
/// Resuelve en una sola respuesta los catálogos base que consume la ficha de clientes.
/// Reduce llamadas dispersas del frontend sin reemplazar los endpoints auxiliares existentes.
/// </summary>
public record ConfiguracionClientesTercerosDto(
    CatalogosTercerosDto Catalogos,
    IReadOnlyList<ConfiguracionClienteReferenciaDto> CategoriasTerceros,
    IReadOnlyList<ConfiguracionClienteCodigoDescripcionDto> CondicionesIva,
    IReadOnlyList<ConfiguracionClienteCodigoDescripcionDto> TiposDocumento,
    IReadOnlyList<ConfiguracionClienteMonedaDto> Monedas,
    IReadOnlyList<ConfiguracionClienteReferenciaDto> Paises,
    IReadOnlyList<ConfiguracionClienteCodigoDescripcionDto> ZonasComerciales,
    IReadOnlyList<ConfiguracionClienteSucursalDto> Sucursales,
    IReadOnlyList<ConfiguracionClienteUsuarioComercialDto> Vendedores,
    IReadOnlyList<ConfiguracionClienteUsuarioComercialDto> Cobradores);

public record ConfiguracionClienteReferenciaDto(
    long Id,
    string Descripcion);

public record ConfiguracionClienteCodigoDescripcionDto(
    long Id,
    string Codigo,
    string Descripcion);

public record ConfiguracionClienteMonedaDto(
    long Id,
    string Codigo,
    string Descripcion,
    string Simbolo,
    bool SinDecimales);

public record ConfiguracionClienteSucursalDto(
    long Id,
    string RazonSocial,
    string? NombreFantasia,
    bool CasaMatriz);

public record ConfiguracionClienteUsuarioComercialDto(
    long Id,
    string UserName,
    string? NombreCompleto,
    string? Email);
