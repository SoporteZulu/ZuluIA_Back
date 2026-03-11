using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Ventas;

public class PlanPago : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public short CantidadCuotas { get; private set; } = 1;
    public decimal InteresPct { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private PlanPago() { }

    public static PlanPago Crear(
        string descripcion,
        short cantidadCuotas,
        decimal interesPct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (cantidadCuotas < 1)
            throw new InvalidOperationException("La cantidad de cuotas debe ser al menos 1.");

        if (interesPct < 0)
            throw new InvalidOperationException("El interés no puede ser negativo.");

        return new PlanPago
        {
            Descripcion     = descripcion.Trim(),
            CantidadCuotas  = cantidadCuotas,
            InteresPct      = interesPct,
            Activo          = true,
            CreatedAt       = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(
        string descripcion,
        short cantidadCuotas,
        decimal interesPct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (cantidadCuotas < 1)
            throw new InvalidOperationException("La cantidad de cuotas debe ser al menos 1.");

        if (interesPct < 0)
            throw new InvalidOperationException("El interés no puede ser negativo.");

        Descripcion    = descripcion.Trim();
        CantidadCuotas = cantidadCuotas;
        InteresPct     = interesPct;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;

    /// <summary>
    /// Calcula el importe de cada cuota dado un total.
    /// </summary>
    public decimal CalcularCuota(decimal total)
    {
        if (CantidadCuotas <= 0) return total;
        var totalConInteres = total * (1 + InteresPct / 100);
        return Math.Round(totalConInteres / CantidadCuotas, 2);
    }

    /// <summary>
    /// Calcula el total con interés aplicado.
    /// </summary>
    public decimal CalcularTotalConInteres(decimal total) =>
        Math.Round(total * (1 + InteresPct / 100), 2);
}