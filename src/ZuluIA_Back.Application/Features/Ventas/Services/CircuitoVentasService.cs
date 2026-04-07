using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Ventas.Services;

public class CircuitoVentasService(
    IApplicationDbContext db,
    StockService stockService,
    CuentaCorrienteService cuentaCorrienteService,
    ICurrentUserService currentUser)
{
    public async Task AplicarEfectosAsync(
        Comprobante comprobante,
        TipoComprobante tipo,
        OperacionStockVenta operacionStock,
        OperacionCuentaCorrienteVenta operacionCuentaCorriente,
        CancellationToken ct)
    {
        if (tipo.AfectaStock && operacionStock != OperacionStockVenta.Ninguna)
        {
            foreach (var item in comprobante.Items)
            {
                var depositoId = item.DepositoId ?? await ObtenerDepositoDefaultAsync(comprobante.SucursalId, ct);
                if (!depositoId.HasValue)
                    continue;

                var cantidad = item.Cantidad - item.CantidadBonificada;
                if (cantidad <= 0)
                    continue;

                if (operacionStock == OperacionStockVenta.Egreso)
                {
                    await stockService.EgresarAsync(
                        item.ItemId,
                        depositoId.Value,
                        cantidad,
                        TipoMovimientoStock.VentaDespacho,
                        "comprobantes",
                        comprobante.Id,
                        comprobante.Observacion,
                        currentUser.UserId,
                        false,
                        ct);
                }
                else
                {
                    await stockService.IngresarAsync(
                        item.ItemId,
                        depositoId.Value,
                        cantidad,
                        TipoMovimientoStock.DevolucionVenta,
                        "comprobantes",
                        comprobante.Id,
                        comprobante.Observacion,
                        currentUser.UserId,
                        ct);
                }
            }
        }

        if (tipo.AfectaCuentaCorriente && operacionCuentaCorriente != OperacionCuentaCorrienteVenta.Ninguna)
        {
            var descripcion = $"{tipo.Descripcion} #{comprobante.Numero.Formateado}";

            if (operacionCuentaCorriente == OperacionCuentaCorrienteVenta.Debito)
            {
                await cuentaCorrienteService.DebitarAsync(
                    comprobante.TerceroId,
                    comprobante.SucursalId,
                    comprobante.MonedaId,
                    comprobante.Total,
                    comprobante.Id,
                    comprobante.Fecha,
                    descripcion,
                    ct);
            }
            else
            {
                await cuentaCorrienteService.AcreditarAsync(
                    comprobante.TerceroId,
                    comprobante.SucursalId,
                    comprobante.MonedaId,
                    comprobante.Total,
                    comprobante.Id,
                    comprobante.Fecha,
                    descripcion,
                    ct);
            }
        }
    }

    private async Task<long?> ObtenerDepositoDefaultAsync(long sucursalId, CancellationToken ct)
    {
        return await db.Depositos
            .AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.EsDefault && x.Activo)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(ct);
    }
}
