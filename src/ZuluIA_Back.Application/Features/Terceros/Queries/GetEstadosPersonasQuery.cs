using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve el catálogo legacy de estados generales de persona usado por terceros.
/// </summary>
public record GetEstadosPersonasQuery(bool SoloActivos = false)
    : IRequest<IReadOnlyList<EstadoPersonaCatalogoDto>>;

public class GetEstadosPersonasQueryHandler(
    IRepository<EstadoPersonaCatalogo> repo)
    : IRequestHandler<GetEstadosPersonasQuery, IReadOnlyList<EstadoPersonaCatalogoDto>>
{
    public async Task<IReadOnlyList<EstadoPersonaCatalogoDto>> Handle(GetEstadosPersonasQuery request, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);

        return items
            .Where(x => !x.IsDeleted)
            .Where(x => !request.SoloActivos || x.Activo)
            .OrderBy(x => x.Descripcion)
            .Select(x => new EstadoPersonaCatalogoDto(x.Id, x.Descripcion, x.Activo))
            .ToList();
    }
}
