using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record CancelacionCedulonColegioInput(long CedulonId, decimal Importe);

public record CancelarDeudaColegioCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    long CajaId,
    long FormaPagoId,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    IReadOnlyList<CancelacionCedulonColegioInput> Cedulones) : IRequest<Result<long>>;
