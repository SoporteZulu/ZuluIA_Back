using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class RegistrarCobroCommandHandler(
    ICobroRepository cobroRepo,
    IComprobanteRepository comprobanteRepo,
    IImputacionRepository imputacionRepo,
    CuentaCorrienteService ctaCteService,
    TesoreriaService tesoreriaService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<RegistrarCobroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        RegistrarCobroCommand request,
        CancellationToken ct)
    {
        // 1. Validar medios
        if (!request.Medios.Any())
            return Result.Failure<long>(
                "El cobro debe tener al menos un medio de cobro.");

        // 2. Crear cobro
        var cobro = Cobro.Crear(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            null,
            null,
            null,
            currentUser.UserId,
            null,
            TipoCobro.Administrativo,
            null,
            null,
            null,
            null,
            currentUser.UserId);

        // 3. Agregar medios
        foreach (var medio in request.Medios)
        {
            cobro.AgregarMedio(CobroMedio.Crear(
                0,
                medio.CajaId,
                medio.FormaPagoId,
                medio.ChequeId,
                medio.Importe,
                medio.MonedaId,
                medio.Cotizacion));
        }

        await cobroRepo.AddAsync(cobro, ct);
        await uow.SaveChangesAsync(ct);

        // 4. Imputar comprobantes
        foreach (var comp in request.ComprobantesAImputar)
        {
            var comprobante = await comprobanteRepo.GetByIdAsync(comp.ComprobanteId, ct);
            if (comprobante is null) continue;

            if (comp.Importe > comprobante.Saldo)
                return Result.Failure<long>(
                    $"El importe a imputar supera el saldo del comprobante {comprobante.Numero.Formateado}.");

            comprobante.ActualizarSaldo(comp.Importe, currentUser.UserId);
            comprobanteRepo.Update(comprobante);

            var imputacion = Domain.Entities.Comprobantes.Imputacion.Crear(
                cobro.Id, comp.ComprobanteId,
                comp.Importe, request.Fecha,
                currentUser.UserId);

            await imputacionRepo.AddAsync(imputacion, ct);
        }

        // 5. Acreditar cuenta corriente
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
                $"Cobro #{cobro.Id}",
                ct);
        }

        foreach (var medio in request.Medios)
        {
            await tesoreriaService.RegistrarMovimientoAsync(
                request.SucursalId,
                medio.CajaId,
                request.Fecha,
                TipoOperacionTesoreria.CobroVentanilla,
                SentidoMovimientoTesoreria.Ingreso,
                medio.Importe,
                medio.MonedaId,
                medio.Cotizacion,
                request.TerceroId,
                "COBRO",
                cobro.Id,
                request.Observacion,
                ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success(cobro.Id);
    }
}