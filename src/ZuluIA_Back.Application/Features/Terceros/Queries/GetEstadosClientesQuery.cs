using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve el catálogo de estados de clientes, incluyendo su marca de bloqueo operativo.
/// </summary>
public record GetEstadosClientesQuery(bool SoloActivos = false)
    : IRequest<IReadOnlyList<EstadoTerceroCatalogoDto>>;

public class GetEstadosClientesQueryHandler(
    IRepository<EstadoCliente> repo)
    : IRequestHandler<GetEstadosClientesQuery, IReadOnlyList<EstadoTerceroCatalogoDto>>
{
    public async Task<IReadOnlyList<EstadoTerceroCatalogoDto>> Handle(
        GetEstadosClientesQuery request,
        CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);

        return items
            .Where(x => !x.IsDeleted)
            .Where(x => !request.SoloActivos || x.Activo)
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new EstadoTerceroCatalogoDto(x.Id, x.Codigo, x.Descripcion, x.Bloquea, x.Activo))
            .ToList();
    }
}
