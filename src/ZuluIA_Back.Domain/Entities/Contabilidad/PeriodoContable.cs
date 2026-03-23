using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

/// <summary>
/// Período contable (mensual o anual) con control de apertura y cierre.
/// Migrado desde VB6: clsPeriodoContable / PERIODO_CONTABLE.
/// </summary>
public class PeriodoContable : BaseEntity
{
    /// <summary>Ej: "2025-01", "2025-12"</summary>
    public string Periodo { get; private set; } = string.Empty;
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public bool Abierto { get; private set; } = true;

    private PeriodoContable() { }

    public static PeriodoContable Crear(string periodo, DateOnly fechaInicio, DateOnly fechaFin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(periodo);
        if (fechaFin < fechaInicio)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");

        return new PeriodoContable
        {
            Periodo     = periodo.Trim(),
            FechaInicio = fechaInicio,
            FechaFin    = fechaFin,
            Abierto     = true
        };
    }

    public void Cerrar()
    {
        if (!Abierto) throw new InvalidOperationException("El período contable ya está cerrado.");
        Abierto = false;
    }

    public void Abrir()
    {
        if (Abierto) throw new InvalidOperationException("El período contable ya está abierto.");
        Abierto = true;
    }
}
