using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

internal class RegistrarDevolucionVentaInternaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    CircuitoVentasService circuitoVentas,
    TerceroOperacionValidationService terceroOperacionValidationService)
    : IRequestHandler<RegistrarDevolucionVentaInternaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarDevolucionVentaInternaCommand request, CancellationToken ct)
    {
        var createHandler = new CrearBorradorVentaCommandHandler(comprobanteRepo, db, uow, currentUser, terceroOperacionValidationService);
        var createResult = await createHandler.Handle(
            new CrearBorradorVentaCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                null,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.ComprobanteOrigenId,
                request.Items),
            ct);

        if (!createResult.IsSuccess)
            return createResult;

        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(createResult.Value, ct);
        if (comprobante is null)
            return Result.Failure<long>("No se pudo recuperar la devolución creada.");

        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, ct);

        if (tipo is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {comprobante.TipoComprobanteId}.");

        // Configurar como devolución
        var tipoDevolucion = (request.OperacionStock, request.OperacionCuentaCorriente) switch
        {
            (OperacionStockVenta.Ninguna, OperacionCuentaCorrienteVenta.Ninguna) => Domain.Enums.TipoDevolucion.SinReintegroStock,
            (OperacionStockVenta.Ingreso, OperacionCuentaCorrienteVenta.Ninguna) => Domain.Enums.TipoDevolucion.ConReintegroStock,
            (OperacionStockVenta.Ingreso, OperacionCuentaCorrienteVenta.Credito) => Domain.Enums.TipoDevolucion.ConReintegroStockYAcreditacion,
            (OperacionStockVenta.Ninguna, OperacionCuentaCorrienteVenta.Credito) => Domain.Enums.TipoDevolucion.SoloAcreditacion,
            _ => Domain.Enums.TipoDevolucion.SinReintegroStock
        };

        comprobante.ConfigurarComoDevolucion(
            request.MotivoDevolucion,
            tipoDevolucion,
            request.ObservacionDevolucion,
            request.AutorizadorDevolucionId,
            request.OperacionStock == OperacionStockVenta.Ingreso,
            request.OperacionCuentaCorriente == OperacionCuentaCorrienteVenta.Credito,
            currentUser.UserId);

        comprobante.Emitir(currentUser.UserId);
        comprobanteRepo.Update(comprobante);

        await circuitoVentas.AplicarEfectosAsync(
            comprobante,
            tipo,
            request.OperacionStock,
            request.OperacionCuentaCorriente,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(comprobante.Id);
    }
}
