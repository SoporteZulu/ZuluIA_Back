using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve el catálogo de categorías de proveedores para mantenimiento y combos del módulo legacy.
/// </summary>
public record GetCategoriasProveedoresQuery(bool SoloActivas = false)
    : IRequest<IReadOnlyList<CategoriaTerceroCatalogoDto>>;

public class GetCategoriasProveedoresQueryHandler(
    IRepository<CategoriaProveedor> repo)
    : IRequestHandler<GetCategoriasProveedoresQuery, IReadOnlyList<CategoriaTerceroCatalogoDto>>
{
    public async Task<IReadOnlyList<CategoriaTerceroCatalogoDto>> Handle(
        GetCategoriasProveedoresQuery request,
        CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);

        return items
            .Where(x => !x.IsDeleted)
            .Where(x => !request.SoloActivas || x.Activa)
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new CategoriaTerceroCatalogoDto(x.Id, x.Codigo, x.Descripcion, x.Activa))
            .ToList();
    }
}
