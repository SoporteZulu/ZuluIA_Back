using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record RegistrarTimbradoFiscalCommand(
    long SucursalId,
    long PuntoFacturacionId,
    string NumeroTimbrado,
    DateOnly VigenciaDesde,
    DateOnly VigenciaHasta,
    string? Observacion) : IRequest<Result<long>>;
