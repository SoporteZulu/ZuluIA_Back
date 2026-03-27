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
    StockService stockService,
    IRepository<OrdenTrabajoConsumo> consumoRepo)
{
    /// <summary>
    /// Ejecuta el consumo de ingredientes y el ingreso del producto terminado
    /// al finalizar una orden de trabajo.
    /// </summary>
    public async Task EjecutarProduccionAsync(
        OrdenTrabajo ot,
        decimal? cantidadProducida,
        IReadOnlyDictionary<long, decimal>? consumosPersonalizados,
        long? userId,
        CancellationToken ct = default)
    {
        var formula = await formulaRepo.GetByIdConIngredientesAsync(ot.FormulaId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontró la fórmula ID {ot.FormulaId}.");

        var cantidadFinal = cantidadProducida ?? ot.Cantidad;

        // Factor de escala: si la fórmula produce 100 unidades y la OT pide 250 → factor 2.5
        var factor = cantidadFinal / formula.CantidadResultado;

        // 1. Egresar ingredientes del depósito origen
        foreach (var ing in formula.Ingredientes.Where(x => !x.EsOpcional))
        {
            var cantidadPlanificada = ing.Cantidad * factor;
            var cantidadConsumida = consumosPersonalizados?.GetValueOrDefault(ing.ItemId) ?? cantidadPlanificada;

            var movimiento = await stockService.EgresarAsync(
                ing.ItemId,
                ot.DepositoOrigenId,
                cantidadConsumida,
                TipoMovimientoStock.ProduccionConsumo,
                "ordenes_trabajo",
                ot.Id,
                $"OT #{ot.Id} — consumo fórmula {formula.Codigo}",
                userId,
                false,
                ct);

            await consumoRepo.AddAsync(
                OrdenTrabajoConsumo.Registrar(
                    ot.Id,
                    ing.ItemId,
                    ot.DepositoOrigenId,
                    cantidadPlanificada,
                    cantidadConsumida,
                    movimiento.Id,
                    $"Consumo fórmula {formula.Codigo}",
                    userId),
                ct);
        }

        // 2. Ingresar producto terminado al depósito destino
        await stockService.IngresarAsync(
            formula.ItemResultadoId,
            ot.DepositoDestinoId,
            cantidadFinal,
            TipoMovimientoStock.ProduccionIngreso,
            "ordenes_trabajo",
            ot.Id,
            $"OT #{ot.Id} — ingreso fórmula {formula.Codigo}",
            userId,
            ct);
    }

    public async Task AjustarSegunFormulaAsync(
        long formulaId,
        long depositoOrigenId,
        long depositoDestinoId,
        decimal cantidad,
        string? observacion,
        long? userId,
        CancellationToken ct = default)
    {
        var formula = await formulaRepo.GetByIdConIngredientesAsync(formulaId, ct)
            ?? throw new InvalidOperationException($"No se encontró la fórmula ID {formulaId}.");

        var factor = cantidad / formula.CantidadResultado;

        foreach (var ing in formula.Ingredientes.Where(x => !x.EsOpcional))
        {
            await stockService.EgresarAsync(
                ing.ItemId,
                depositoOrigenId,
                ing.Cantidad * factor,
                TipoMovimientoStock.AjusteNegativo,
                "ajuste_produccion",
                formulaId,
                observacion ?? $"Ajuste producción fórmula {formula.Codigo}",
                userId,
                false,
                ct);
        }

        await stockService.IngresarAsync(
            formula.ItemResultadoId,
            depositoDestinoId,
            cantidad,
            TipoMovimientoStock.AjustePositivo,
            "ajuste_produccion",
            formulaId,
            observacion ?? $"Ajuste producción fórmula {formula.Codigo}",
            userId,
            ct);
    }
}