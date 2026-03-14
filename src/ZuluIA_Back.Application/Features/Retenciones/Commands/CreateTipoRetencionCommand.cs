using MediatR;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Retenciones.Commands;

public record CreateTipoRetencionCommand(
    string Descripcion,
    string Regimen,
    decimal MinimoNoImponible,
    bool AcumulaPago,
    long? TipoComprobanteId,
    long? ItemId,
    IReadOnlyList<EscalaRetencionInputDto> Escalas
) : IRequest<Result<long>>;
