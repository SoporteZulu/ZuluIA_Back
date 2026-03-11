using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class PeriodoIva : BaseEntity
{
    public long EjercicioId { get; private set; }
    public long SucursalId { get; private set; }

    /// <summary>
    /// Primer día del mes del período (ej: 2025-01-01 para enero 2025).
    /// </summary>
    public DateOnly Periodo { get; private set; }
    public bool Cerrado { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private PeriodoIva() { }

    public static PeriodoIva Crear(
        long ejercicioId,
        long sucursalId,
        DateOnly periodo)
    {
        // Normalizar al primer día del mes
        var primerDia = new DateOnly(periodo.Year, periodo.Month, 1);

        return new PeriodoIva
        {
            EjercicioId = ejercicioId,
            SucursalId  = sucursalId,
            Periodo     = primerDia,
            Cerrado     = false,
            CreatedAt   = DateTimeOffset.UtcNow
        };
    }

    public void Cerrar()
    {
        if (Cerrado)
            throw new InvalidOperationException(
                $"El período {Periodo:yyyy-MM} ya está cerrado.");

        Cerrado = true;
    }

    public void Reabrir()
    {
        if (!Cerrado)
            throw new InvalidOperationException(
                $"El período {Periodo:yyyy-MM} no está cerrado.");

        Cerrado = false;
    }

    public string PeriodoDescripcion =>
        $"{Periodo:MMMM yyyy}";
}