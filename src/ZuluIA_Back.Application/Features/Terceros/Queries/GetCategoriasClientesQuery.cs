using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve el catálogo de categorías de clientes para mantenimiento y combos del módulo legacy.
/// </summary>
public record GetCategoriasClientesQuery(bool SoloActivas = false)
    : IRequest<IReadOnlyList<CategoriaTerceroCatalogoDto>>;

public class GetCategoriasClientesQueryHandler(
    IRepository<CategoriaCliente> repo)
    : IRequestHandler<GetCategoriasClientesQuery, IReadOnlyList<CategoriaTerceroCatalogoDto>>
{
    public async Task<IReadOnlyList<CategoriaTerceroCatalogoDto>> Handle(
        GetCategoriasClientesQuery request,
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
