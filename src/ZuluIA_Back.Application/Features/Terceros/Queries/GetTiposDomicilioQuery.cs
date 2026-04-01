using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve el catálogo legacy de tipos de domicilio usado por `PER_DOMICILIO`.
/// </summary>
public record GetTiposDomicilioQuery : IRequest<IReadOnlyList<TipoDomicilioCatalogoDto>>;

public class GetTiposDomicilioQueryHandler(
    IRepository<TipoDomicilioCatalogo> repo)
    : IRequestHandler<GetTiposDomicilioQuery, IReadOnlyList<TipoDomicilioCatalogoDto>>
{
    public async Task<IReadOnlyList<TipoDomicilioCatalogoDto>> Handle(GetTiposDomicilioQuery request, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);

        return items
            .OrderBy(x => x.Descripcion)
            .Select(x => new TipoDomicilioCatalogoDto(x.Id, x.Descripcion))
            .ToList();
    }
}
