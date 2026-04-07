using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Finanzas.Services;

public class PagoWorkflowService(
    IComprobanteRepository comprobanteRepo,
    IImputacionRepository imputacionRepo,
    CuentaCorrienteService ctaCteService,
    TesoreriaService tesoreriaService,
    IApplicationDbContext db)
{
    public async Task<Result<long>?> ApplyAsync(Pago pago, RegistrarPagoCommand request, long? userId, CancellationToken ct)
    {
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

        foreach (var comp in request.ComprobantesAImputar)
        {
            var comprobante = await comprobanteRepo.GetByIdAsync(comp.ComprobanteId, ct);
            if (comprobante is null)
                continue;

            if (comp.Importe > comprobante.Saldo)
            {
                return Result.Failure<long>(
                    $"El importe supera el saldo del comprobante {comprobante.Numero.Formateado}.");
            }

            comprobante.ActualizarSaldo(comp.Importe, userId);
            comprobanteRepo.Update(comprobante);

            var imputacion = Domain.Entities.Comprobantes.Imputacion.Crear(
                pago.Id,
                comp.ComprobanteId,
                comp.Importe,
                request.Fecha,
                userId);

            await imputacionRepo.AddAsync(imputacion, ct);
        }

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

        foreach (var medio in request.Medios)
        {
            await tesoreriaService.RegistrarMovimientoAsync(
                request.SucursalId,
                medio.CajaId,
                request.Fecha,
                TipoOperacionTesoreria.PagoVentanilla,
                SentidoMovimientoTesoreria.Egreso,
                medio.Importe,
                medio.MonedaId,
                medio.Cotizacion,
                request.TerceroId,
                "PAGO",
                pago.Id,
                request.Observacion,
                ct);
        }

        return null;
    }
}
