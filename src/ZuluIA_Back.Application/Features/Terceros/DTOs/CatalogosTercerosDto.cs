namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public record CatalogosTercerosDto(
    IReadOnlyList<CategoriaTerceroCatalogoDto> CategoriasClientes,
    IReadOnlyList<CategoriaTerceroCatalogoDto> CategoriasProveedores,
    IReadOnlyList<EstadoTerceroCatalogoDto> EstadosClientes,
    IReadOnlyList<EstadoTerceroCatalogoDto> EstadosProveedores,
    IReadOnlyList<EstadoPersonaCatalogoDto> EstadosPersonas,
    IReadOnlyList<EstadoCivilCatalogoDto> EstadosCiviles,
    IReadOnlyList<TipoDomicilioCatalogoDto> TiposDomicilio);

public record CategoriaTerceroCatalogoDto(
    long Id,
    string Codigo,
    string Descripcion,
    bool Activa);

public record EstadoTerceroCatalogoDto(
    long Id,
    string Codigo,
    string Descripcion,
    bool Bloquea,
    bool Activo);

public record EstadoPersonaCatalogoDto(
    long Id,
    string Descripcion,
    bool Activo);

public record EstadoCivilCatalogoDto(
    long Id,
    string Descripcion,
    bool Activo);

public record TipoDomicilioCatalogoDto(
    long Id,
    string Descripcion);
