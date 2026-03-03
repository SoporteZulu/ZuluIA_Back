using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record CreateCartaPorteCommand(
    long? ComprobanteId,
    string CuitRemitente,
    string CuitDestinatario,
    string? CuitTransportista,
    DateOnly FechaEmision,
    string? Observacion
) : IRequest<Result<long>>;