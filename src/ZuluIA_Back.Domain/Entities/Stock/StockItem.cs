using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Events.Stock;

namespace ZuluIA_Back.Domain.Entities.Stock;

public class StockItem : BaseEntity
{
    public long ItemId { get; private set; }
    public long DepositoId { get; private set; }
    public decimal Cantidad { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private StockItem() { }

    public static StockItem Crear(long itemId, long depositoId, decimal cantidadInicial = 0)
    {
        return new StockItem
        {
            ItemId     = itemId,
            DepositoId = depositoId,
            Cantidad   = cantidadInicial,
            UpdatedAt  = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Incrementa el stock. Usado en ingresos, devoluciones, ajustes positivos.
    /// </summary>
    public void Ingresar(decimal cantidad)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad a ingresar debe ser mayor a 0.");

        Cantidad  += cantidad;
        UpdatedAt  = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Decrementa el stock. Usado en egresos, ventas, ajustes negativos.
    /// </summary>
    public void Egresar(decimal cantidad, bool permitirNegativo = false)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad a egresar debe ser mayor a 0.");

        if (!permitirNegativo && Cantidad < cantidad)
            throw new InvalidOperationException(
                $"Stock insuficiente. Stock actual: {Cantidad}, cantidad solicitada: {cantidad}.");

        Cantidad  -= cantidad;
        UpdatedAt  = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Ajuste directo a una cantidad específica.
    /// Retorna la diferencia (positiva = ingreso, negativa = egreso).
    /// Dispara evento de dominio.
    /// </summary>
    public decimal AjustarA(decimal nuevaCantidad, string motivo = "")
    {
        if (nuevaCantidad < 0)
            throw new InvalidOperationException("La cantidad de stock no puede ser negativa.");

        var anterior = Cantidad;
        var diferencia = nuevaCantidad - anterior;
        Cantidad  = nuevaCantidad;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new StockAjustadoEvent(ItemId, DepositoId, anterior, nuevaCantidad, motivo));
        return diferencia;
    }

    /// <summary>
    /// Ajuste directo sin evento (útil para migraciones o inicializaciones).
    /// </summary>
    public void AjustarStock(decimal nuevaCantidad, string motivo)
    {
        var anterior = Cantidad;
        Cantidad  = nuevaCantidad;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new StockAjustadoEvent(ItemId, DepositoId, anterior, nuevaCantidad, motivo));
    }

    /// <summary>
    /// Incrementa el stock (alias de Ingresar).
    /// </summary>
    public void Incrementar(decimal cantidad)
    {
        Ingresar(cantidad);
    }

    /// <summary>
    /// Decrementa el stock (alias de Egresar).
    /// </summary>
    public void Decrementar(decimal cantidad, bool permitirNegativo = false)
    {
        Egresar(cantidad, permitirNegativo);
    }
}
