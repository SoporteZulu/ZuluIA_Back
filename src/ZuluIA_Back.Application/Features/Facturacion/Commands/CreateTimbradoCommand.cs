using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record CreateTimbradoCommand(
    long SucursalId,
    long PuntoFacturacionId,
    long TipoComprobanteId,
    string NroTimbrado,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int NroComprobanteDesde,
    int NroComprobanteHasta) : IRequest<Result<long>>;