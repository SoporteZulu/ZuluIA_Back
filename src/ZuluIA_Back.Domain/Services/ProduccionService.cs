using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que gestiona la ejecución de órdenes de trabajo:
/// consume ingredientes del depósito origen e ingresa el producto terminado
/// en el depósito destino.
/// </summary>
public class ProduccionService(
    IFormulaProduccionRepository formulaRepo,
    StockService stockService)
{
    /// <summary>
    /// Ejecuta el consumo de ingredientes y el ingreso del producto terminado
    /// al finalizar una orden de trabajo.
    /// </summary>
    public async Task EjecutarProduccionAsync(
        OrdenTrabajo ot,
        long? userId,
        CancellationToken ct = default)
    {
        var formula = await formulaRepo.GetByIdConIngredientesAsync(ot.FormulaId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontró la fórmula ID {ot.FormulaId}.");

        // Factor de escala: si la fórmula produce 100 unidades y la OT pide 250 → factor 2.5
        var factor = ot.Cantidad / formula.CantidadResultado;

        // 1. Egresar ingredientes del depósito origen
        foreach (var ing in formula.Ingredientes.Where(x => !x.EsOpcional))
        {
            await stockService.EgresarAsync(
                ing.ItemId,
                ot.DepositoOrigenId,
                ing.Cantidad * factor,
                TipoMovimientoStock.ProduccionConsumo,
                "ordenes_trabajo",
                ot.Id,
                $"OT #{ot.Id} — consumo fórmula {formula.Codigo}",
                userId,
                false,
                ct);
        }

        // 2. Ingresar producto terminado al depósito destino
        await stockService.IngresarAsync(
            formula.ItemResultadoId,
            ot.DepositoDestinoId,
            ot.Cantidad,
            TipoMovimientoStock.ProduccionIngreso,
            "ordenes_trabajo",
            ot.Id,
            $"OT #{ot.Id} — ingreso fórmula {formula.Codigo}",
            userId,
            ct);
    }
}