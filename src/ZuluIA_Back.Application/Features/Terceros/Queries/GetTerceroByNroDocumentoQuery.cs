using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Obtiene el detalle de un tercero por su número de documento (CUIT/DNI).
/// Equivalente a la búsqueda rápida por documento en el formulario del VB6.
/// </summary>
public record GetTerceroByNroDocumentoQuery(string NroDocumento)
    : IRequest<Result<TerceroDto>>;

public class GetTerceroByNroDocumentoQueryHandler(
    ITerceroRepository repo,
    ISender sender)
    : IRequestHandler<GetTerceroByNroDocumentoQuery, Result<TerceroDto>>
{
    public async Task<Result<TerceroDto>> Handle(
        GetTerceroByNroDocumentoQuery request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NroDocumento))
            return Result.Failure<TerceroDto>("El número de documento no puede estar vacío.");

        var tercero = await repo.GetByNroDocumentoAsync(request.NroDocumento, ct);

        if (tercero is null)
            return Result.Failure<TerceroDto>(
                $"No se encontró ningún tercero con el documento '{request.NroDocumento.Trim()}'.");

        // Reutiliza la misma lógica de enriquecimiento que GetTerceroByIdQuery.
        return await sender.Send(new GetTerceroByIdQuery(tercero.Id), ct);
    }
}
