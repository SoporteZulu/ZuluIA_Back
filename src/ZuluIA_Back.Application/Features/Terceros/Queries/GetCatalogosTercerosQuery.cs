using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve en una sola respuesta los catálogos legacy usados por la pantalla de terceros.
/// </summary>
public record GetCatalogosTercerosQuery(bool SoloActivos = false)
    : IRequest<CatalogosTercerosDto>;

public class GetCatalogosTercerosQueryHandler(
    IRepository<CategoriaCliente> categoriasClientesRepo,
    IRepository<CategoriaProveedor> categoriasProveedoresRepo,
    IRepository<EstadoCliente> estadosClientesRepo,
    IRepository<EstadoProveedor> estadosProveedoresRepo,
    IRepository<EstadoPersonaCatalogo> estadosPersonasRepo,
    IRepository<EstadoCivilCatalogo> estadosCivilesRepo,
    IRepository<TipoDomicilioCatalogo> tiposDomicilioRepo)
    : IRequestHandler<GetCatalogosTercerosQuery, CatalogosTercerosDto>
{
    public async Task<CatalogosTercerosDto> Handle(GetCatalogosTercerosQuery request, CancellationToken ct)
    {
        var categoriasClientes = await categoriasClientesRepo.GetAllAsync(ct);
        var categoriasProveedores = await categoriasProveedoresRepo.GetAllAsync(ct);
        var estadosClientes = await estadosClientesRepo.GetAllAsync(ct);
        var estadosProveedores = await estadosProveedoresRepo.GetAllAsync(ct);
        var estadosPersonas = await estadosPersonasRepo.GetAllAsync(ct);
        var estadosCiviles = await estadosCivilesRepo.GetAllAsync(ct);
        var tiposDomicilio = await tiposDomicilioRepo.GetAllAsync(ct);

        return new CatalogosTercerosDto(
            MapCategorias(categoriasClientes, request.SoloActivos),
            MapCategorias(categoriasProveedores, request.SoloActivos),
            MapEstados(estadosClientes, request.SoloActivos),
            MapEstados(estadosProveedores, request.SoloActivos),
            MapEstadosPersonas(estadosPersonas, request.SoloActivos),
            MapEstadosCiviles(estadosCiviles, request.SoloActivos),
            MapTiposDomicilio(tiposDomicilio));
    }

    private static IReadOnlyList<CategoriaTerceroCatalogoDto> MapCategorias(
        IEnumerable<CategoriaCliente> items,
        bool soloActivos)
        => items
            .Where(x => !x.IsDeleted)
            .Where(x => !soloActivos || x.Activa)
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new CategoriaTerceroCatalogoDto(x.Id, x.Codigo, x.Descripcion, x.Activa))
            .ToList();

    private static IReadOnlyList<CategoriaTerceroCatalogoDto> MapCategorias(
        IEnumerable<CategoriaProveedor> items,
        bool soloActivos)
        => items
            .Where(x => !x.IsDeleted)
            .Where(x => !soloActivos || x.Activa)
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new CategoriaTerceroCatalogoDto(x.Id, x.Codigo, x.Descripcion, x.Activa))
            .ToList();

    private static IReadOnlyList<EstadoTerceroCatalogoDto> MapEstados(
        IEnumerable<EstadoCliente> items,
        bool soloActivos)
        => items
            .Where(x => !x.IsDeleted)
            .Where(x => !soloActivos || x.Activo)
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new EstadoTerceroCatalogoDto(x.Id, x.Codigo, x.Descripcion, x.Bloquea, x.Activo))
            .ToList();

    private static IReadOnlyList<EstadoTerceroCatalogoDto> MapEstados(
        IEnumerable<EstadoProveedor> items,
        bool soloActivos)
        => items
            .Where(x => !x.IsDeleted)
            .Where(x => !soloActivos || x.Activo)
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new EstadoTerceroCatalogoDto(x.Id, x.Codigo, x.Descripcion, x.Bloquea, x.Activo))
            .ToList();

    private static IReadOnlyList<EstadoCivilCatalogoDto> MapEstadosCiviles(
        IEnumerable<EstadoCivilCatalogo> items,
        bool soloActivos)
        => items
            .Where(x => !x.IsDeleted)
            .Where(x => !soloActivos || x.Activo)
            .OrderBy(x => x.Descripcion)
            .Select(x => new EstadoCivilCatalogoDto(x.Id, x.Descripcion, x.Activo))
            .ToList();

    private static IReadOnlyList<EstadoPersonaCatalogoDto> MapEstadosPersonas(
        IEnumerable<EstadoPersonaCatalogo> items,
        bool soloActivos)
        => items
            .Where(x => !x.IsDeleted)
            .Where(x => !soloActivos || x.Activo)
            .OrderBy(x => x.Descripcion)
            .Select(x => new EstadoPersonaCatalogoDto(x.Id, x.Descripcion, x.Activo))
            .ToList();

    private static IReadOnlyList<TipoDomicilioCatalogoDto> MapTiposDomicilio(
        IEnumerable<TipoDomicilioCatalogo> items)
        => items
            .OrderBy(x => x.Descripcion)
            .Select(x => new TipoDomicilioCatalogoDto(x.Id, x.Descripcion))
            .ToList();
}
