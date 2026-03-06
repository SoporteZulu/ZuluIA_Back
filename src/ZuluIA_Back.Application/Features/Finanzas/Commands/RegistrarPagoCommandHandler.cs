using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class RegistrarPagoCommandHandler(
    IPagoRepository pagoRepo,
    IComprobanteRepository comprobanteRepo,
    IImputacionRepository imputacionRepo,
    CuentaCorrienteService ctaCteService,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db)
    : IRequestHandler<RegistrarPagoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        RegistrarPagoCommand request,
        CancellationToken ct)
    {
        if (!request.Medios.Any())
            return Result.Failure<long>(
                "El pago debe tener al menos un medio de pago.");

        // 1. Crear pago
        var pago = Pago.Crear(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            currentUser.UserId);

        // 2. Agregar medios
        foreach (var medio in request.Medios)
        {
            pago.AgregarMedio(PagoMedio.Crear(
                0,
                medio.CajaId,
                medio.FormaPagoId,
                medio.ChequeId,
                medio.Importe,
                medio.MonedaId,
                medio.Cotizacion));
        }

        await pagoRepo.AddAsync(pago, ct);

        // 3. Agregar retenciones
        foreach (var ret in request.Retenciones)
        {
            var retencion = Retencion.CrearEnPago(
                pago.Id,
                ret.Tipo,
                ret.Importe,
                ret.NroCertificado,
                request.Fecha);

            await db.Retenciones.AddAsync(retencion, ct);
        }

        // 4. Imputar comprobantes
        foreach (var comp in request.ComprobantesAImputar)
        {
            var comprobante = await comprobanteRepo.GetByIdAsync(comp.ComprobanteId, ct);
            if (comprobante is null) continue;

            if (comp.Importe > comprobante.Saldo)
                return Result.Failure<long>(
                    $"El importe supera el saldo del comprobante {comprobante.Numero.Formateado}.");

            comprobante.ActualizarSaldo(comp.Importe, currentUser.UserId);
            comprobanteRepo.Update(comprobante);

            var imputacion = Domain.Entities.Comprobantes.Imputacion.Crear(
                pago.Id, comp.ComprobanteId,
                comp.Importe, request.Fecha,
                currentUser.UserId);

            await imputacionRepo.AddAsync(imputacion, ct);
        }

        // 5. Acreditar cuenta corriente del proveedor
        if (request.ComprobantesAImputar.Any())
        {
            var totalImputado = request.ComprobantesAImputar.Sum(x => x.Importe);
            await ctaCteService.AcreditarAsync(
                request.TerceroId,
                request.SucursalId,
                request.MonedaId,
                totalImputado,
                null,
                request.Fecha,
                $"Pago #{pago.Id}",
                ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success(pago.Id);
    }
}