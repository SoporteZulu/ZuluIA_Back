using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class RegistrarTransferenciaCommandHandler(
    IApplicationDbContext db,
    TesoreriaService tesoreriaService,
    IUnitOfWork uow)
    : IRequestHandler<RegistrarTransferenciaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        RegistrarTransferenciaCommand request,
        CancellationToken ct)
    {
        // Validar que ambas cajas existan y estén activas
        var cajaOrigen = await db.CajasCuentasBancarias
            .FirstOrDefaultAsync(x => x.Id == request.CajaOrigenId && !x.IsDeleted, ct);

        if (cajaOrigen is null)
            return Result.Failure<long>(
                $"No se encontró la caja origen con ID {request.CajaOrigenId}.");

        var cajaDestino = await db.CajasCuentasBancarias
            .FirstOrDefaultAsync(x => x.Id == request.CajaDestinoId && !x.IsDeleted, ct);

        if (cajaDestino is null)
            return Result.Failure<long>(
                $"No se encontró la caja destino con ID {request.CajaDestinoId}.");

        var transferencia = TransferenciaCaja.Registrar(
            request.SucursalId,
            request.CajaOrigenId,
            request.CajaDestinoId,
            request.Fecha,
            request.Importe,
            request.MonedaId,
            request.Cotizacion,
            request.Concepto,
            userId: null);

        db.TransferenciasCaja.Add(transferencia);
        await uow.SaveChangesAsync(ct);

        var movimientos = await tesoreriaService.RegistrarMovimientoEntreCajasAsync(
            request.SucursalId,
            request.CajaOrigenId,
            request.CajaDestinoId,
            request.Fecha,
            TipoOperacionTesoreria.TransferenciaEgreso,
            TipoOperacionTesoreria.TransferenciaIngreso,
            request.Importe,
            request.MonedaId,
            request.Cotizacion,
            "TRANSFERENCIA",
            transferencia.Id,
            request.Concepto,
            ct);

        transferencia.AsignarMovimientos(movimientos.origen.Id, movimientos.destino.Id);
        await uow.SaveChangesAsync(ct);

        return Result.Success(transferencia.Id);
    }
}
