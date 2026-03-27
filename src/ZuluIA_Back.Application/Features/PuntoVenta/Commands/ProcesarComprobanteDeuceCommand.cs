using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record ProcesarComprobanteDeuceCommand(
    long ComprobanteId,
    string ReferenciaExterna,
    string? RequestPayload,
    string? ResponsePayload,
    string? Observacion,
    bool Confirmada = true) : IRequest<Result<long>>;
