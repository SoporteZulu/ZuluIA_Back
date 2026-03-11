using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using AutoMapper;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Obtiene el detalle de un tercero por su legajo.
/// Equivalente a la búsqueda rápida por legajo en el formulario del VB6.
/// </summary>
public record GetTerceroByLegajoQuery(string Legajo)
    : IRequest<Result<TerceroDto>>;

public class GetTerceroByLegajoQueryHandler(
    ITerceroRepository repo,
    IApplicationDbContext db,
    IMapper mapper)
    : IRequestHandler<GetTerceroByLegajoQuery, Result<TerceroDto>>
{
    public async Task<Result<TerceroDto>> Handle(
        GetTerceroByLegajoQuery request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Legajo))
            return (Result<TerceroDto>)Result<TerceroDto>.Failure("El legajo no puede estar vacío.");

        var tercero = await repo.GetByLegajoAsync(request.Legajo, ct);

        if (tercero is null)
            return (Result<TerceroDto>)Result<TerceroDto>.Failure(
                $"No se encontró ningún tercero con el legajo '{request.Legajo.ToUpperInvariant()}'.");

        // Reutiliza la misma lógica de enriquecimiento que GetTerceroByIdQuery.
        // Al ser el mismo Handler de detalle, se delega directamente.
        return await new GetTerceroByIdQueryHandler(repo, db, mapper)
            .Handle(new GetTerceroByIdQuery(tercero.Id), ct);
    }
}