using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Ventas;

/// <summary>
/// Descuento comercial configurado para un tercero (cliente) + ítem específico,
/// aplicable dentro de un rango de fechas.
/// Equivale a la tabla DESCUENTOS_COMERCIALES del sistema VB6.
/// </summary>
public class DescuentoComercial : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long ItemId { get; private set; }
    public DateOnly FechaDesde { get; private set; }
    public DateOnly? FechaHasta { get; private set; }

    /// <summary>Porcentaje de descuento, ej. 10.5 = 10.5 %</summary>
    public decimal Porcentaje { get; private set; }

    private DescuentoComercial() { }

    public static DescuentoComercial Crear(
        long terceroId,
        long itemId,
        DateOnly fechaDesde,
        DateOnly? fechaHasta,
        decimal porcentaje,
        long? userId)
    {
        if (porcentaje <= 0 || porcentaje > 100)
            throw new ArgumentOutOfRangeException(nameof(porcentaje), "El porcentaje debe estar entre 0 y 100.");

        if (fechaHasta.HasValue && fechaHasta.Value < fechaDesde)
            throw new ArgumentException("FechaHasta no puede ser anterior a FechaDesde.");

        var d = new DescuentoComercial
        {
            TerceroId  = terceroId,
            ItemId     = itemId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Porcentaje = porcentaje
        };

        d.SetCreated(userId);
        return d;
    }

    public void Actualizar(DateOnly fechaDesde, DateOnly? fechaHasta, decimal porcentaje, long? userId)
    {
        if (porcentaje <= 0 || porcentaje > 100)
            throw new ArgumentOutOfRangeException(nameof(porcentaje), "El porcentaje debe estar entre 0 y 100.");

        if (fechaHasta.HasValue && fechaHasta.Value < fechaDesde)
            throw new ArgumentException("FechaHasta no puede ser anterior a FechaDesde.");

        FechaDesde = fechaDesde;
        FechaHasta = fechaHasta;
        Porcentaje = porcentaje;
        SetUpdated(userId);
    }

    public void Eliminar(long? userId)
    {
        SetDeleted();
        SetUpdated(userId);
    }

    /// <summary>
    /// Retorna el porcentaje si la fecha es válida dentro del rango, o 0 si no aplica.
    /// </summary>
    public decimal ObtenerPorcentajeVigente(DateOnly fecha) =>
        fecha >= FechaDesde && (!FechaHasta.HasValue || fecha <= FechaHasta.Value)
            ? Porcentaje
            : 0m;
}
