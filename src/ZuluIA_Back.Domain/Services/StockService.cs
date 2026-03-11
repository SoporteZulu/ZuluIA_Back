using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que centraliza toda la lógica de movimientos de stock.
/// Garantiza que por cada movimiento de stock se registre su trazabilidad.
/// </summary>
public class StockService(
    IStockRepository stockRepo,
    IMovimientoStockRepository movimientoRepo)
{
    /// <summary>
    /// Registra un ingreso de stock y su movimiento asociado.
    /// </summary>
    public async Task<MovimientoStock> IngresarAsync(
        long itemId,
        long depositoId,
        decimal cantidad,
        TipoMovimientoStock tipo,
        string? origenTabla,
        long? origenId,
        string? observacion,
        long? userId,
        CancellationToken ct = default)
    {
        var stock = await stockRepo.GetOrCreateAsync(itemId, depositoId, ct);
        stock.Ingresar(cantidad);
        stockRepo.Update(stock);

        var movimiento = MovimientoStock.Crear(
            itemId, depositoId, tipo,
            cantidad, stock.Cantidad,
            origenTabla, origenId, observacion, userId);

        await movimientoRepo.AddAsync(movimiento, ct);
        return movimiento;
    }

    /// <summary>
    /// Registra un egreso de stock y su movimiento asociado.
    /// </summary>
    public async Task<MovimientoStock> EgresarAsync(
        long itemId,
        long depositoId,
        decimal cantidad,
        TipoMovimientoStock tipo,
        string? origenTabla,
        long? origenId,
        string? observacion,
        long? userId,
        bool permitirNegativo = false,
        CancellationToken ct = default)
    {
        var stock = await stockRepo.GetOrCreateAsync(itemId, depositoId, ct);
        stock.Egresar(cantidad, permitirNegativo);
        stockRepo.Update(stock);

        var movimiento = MovimientoStock.Crear(
            itemId, depositoId, tipo,
            -cantidad, stock.Cantidad,
            origenTabla, origenId, observacion, userId);

        await movimientoRepo.AddAsync(movimiento, ct);
        return movimiento;
    }

    /// <summary>
    /// Ajusta el stock a una cantidad específica.
    /// Genera el movimiento correspondiente (positivo o negativo).
    /// </summary>
    public async Task<MovimientoStock> AjustarAsync(
        long itemId,
        long depositoId,
        decimal nuevaCantidad,
        string? observacion,
        long? userId,
        CancellationToken ct = default)
    {
        var stock = await stockRepo.GetOrCreateAsync(itemId, depositoId, ct);
        var diferencia = stock.AjustarA(nuevaCantidad);
        stockRepo.Update(stock);

        var tipo = diferencia >= 0
            ? TipoMovimientoStock.AjustePositivo
            : TipoMovimientoStock.AjusteNegativo;

        var movimiento = MovimientoStock.Crear(
            itemId, depositoId, tipo,
            diferencia, stock.Cantidad,
            null, null, observacion, userId);

        await movimientoRepo.AddAsync(movimiento, ct);
        return movimiento;
    }

    /// <summary>
    /// Transfiere stock entre dos depósitos.
    /// Genera dos movimientos: salida del origen y entrada al destino.
    /// </summary>
    public async Task TransferirAsync(
        long itemId,
        long depositoOrigenId,
        long depositoDestinoId,
        decimal cantidad,
        string? observacion,
        long? userId,
        CancellationToken ct = default)
    {
        if (depositoOrigenId == depositoDestinoId)
            throw new InvalidOperationException(
                "El depósito de origen y destino no pueden ser el mismo.");

        await EgresarAsync(
            itemId, depositoOrigenId, cantidad,
            TipoMovimientoStock.TransferenciaSalida,
            "transferencia", null, observacion, userId,
            false, ct);

        await IngresarAsync(
            itemId, depositoDestinoId, cantidad,
            TipoMovimientoStock.TransferenciaEntrada,
            "transferencia", null, observacion, userId,
            ct);
    }
}