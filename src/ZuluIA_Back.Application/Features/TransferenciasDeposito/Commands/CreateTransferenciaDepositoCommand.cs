using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public record CreateTransferenciaDepositoDetalleInput(long ItemId, decimal Cantidad, string? Observacion);

public record CreateTransferenciaDepositoCommand(long SucursalId, long DepositoOrigenId, long DepositoDestinoId, DateOnly Fecha, string? Observacion, IReadOnlyList<CreateTransferenciaDepositoDetalleInput> Detalles, long? OrdenPreparacionId = null) : IRequest<Result<long>>;
