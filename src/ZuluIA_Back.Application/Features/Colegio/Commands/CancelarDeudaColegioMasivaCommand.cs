using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record CancelacionCedulonColegioMasivaInput(long CedulonId, decimal Importe);

public record CancelarDeudaColegioMasivaCommand(
    long SucursalId,
    DateOnly Fecha,
    long CajaId,
    long FormaPagoId,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    IReadOnlyList<CancelacionCedulonColegioMasivaInput> Cedulones) : IRequest<Result<IReadOnlyList<long>>>;
