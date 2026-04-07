using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class RegistrarDepositoOperarCommandHandler(
    TesoreriaService tesoreriaService,
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow)
    : IRequestHandler<RegistrarDepositoOperarCommand, Result<IReadOnlyList<long>>>
{
    public async Task<Result<IReadOnlyList<long>>> Handle(RegistrarDepositoOperarCommand request, CancellationToken ct)
    {
        try
        {
            var transferencia = TransferenciaCaja.Registrar(
                request.SucursalId,
                request.CajaOrigenId,
                request.CajaDestinoId,
                request.Fecha,
                request.Importe,
                request.MonedaId,
                request.Cotizacion,
                request.Observacion,
                currentUser.UserId);

            await db.TransferenciasCaja.AddAsync(transferencia, ct);
            await uow.SaveChangesAsync(ct);

            var movimientos = await tesoreriaService.RegistrarMovimientoEntreCajasAsync(
                request.SucursalId,
                request.CajaOrigenId,
                request.CajaDestinoId,
                request.Fecha,
                TipoOperacionTesoreria.DepositoOperar,
                TipoOperacionTesoreria.DepositoOperar,
                request.Importe,
                request.MonedaId,
                request.Cotizacion,
                "DEPOSITO_OPERAR",
                transferencia.Id,
                request.Observacion,
                ct);

            transferencia.AsignarMovimientos(movimientos.origen.Id, movimientos.destino.Id);
            await uow.SaveChangesAsync(ct);
            return Result.Success<IReadOnlyList<long>>([movimientos.origen.Id, movimientos.destino.Id]);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<IReadOnlyList<long>>(ex.Message);
        }
    }
}
