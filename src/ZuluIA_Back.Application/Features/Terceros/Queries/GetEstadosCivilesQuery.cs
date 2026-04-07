using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve el catálogo legacy de estados civiles usado por personas físicas en terceros.
/// </summary>
public record GetEstadosCivilesQuery(bool SoloActivos = false)
    : IRequest<IReadOnlyList<EstadoCivilCatalogoDto>>;

public class GetEstadosCivilesQueryHandler(
    IRepository<EstadoCivilCatalogo> repo)
    : IRequestHandler<GetEstadosCivilesQuery, IReadOnlyList<EstadoCivilCatalogoDto>>
{
    public async Task<IReadOnlyList<EstadoCivilCatalogoDto>> Handle(GetEstadosCivilesQuery request, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);

        return items
            .Where(x => !x.IsDeleted)
            .Where(x => !request.SoloActivos || x.Activo)
            .OrderBy(x => x.Descripcion)
            .Select(x => new EstadoCivilCatalogoDto(x.Id, x.Descripcion, x.Activo))
            .ToList();
    }
}
