using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record ProcesarComprobanteSifenCommand(
    long ComprobanteId,
    long? TimbradoFiscalId,
    string? RequestPayload,
    string? ResponsePayload,
    string? CodigoSeguridad,
    string? Observacion,
    bool Confirmada = true) : IRequest<Result<long>>;
