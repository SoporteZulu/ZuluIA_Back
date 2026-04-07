using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Impuestos;

/// <summary>
/// Definición de un impuesto/percepción (IIBB, Ganancias, IVA especial, etc.)
/// con su alícuota y base de cálculo mínima.
/// Migrado desde VB6: clsImpuesto / IMP_IMPUESTO.
/// </summary>
public class Impuesto : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string? Observacion { get; private set; }
    /// <summary>Tasa/alícuota en porcentaje (ej: 3.5 = 3,5%).</summary>
    public decimal Alicuota { get; private set; }
    /// <summary>Monto mínimo de base imponible para aplicar el impuesto.</summary>
    public decimal MinimoBaseCalculo { get; private set; }
    /// <summary>Tipo de impuesto: "percepcion", "retencion", "iva_especial".</summary>
    public string Tipo { get; private set; } = "percepcion";
    public bool Activo { get; private set; } = true;

    private Impuesto() { }

    public static Impuesto Crear(string codigo, string descripcion, decimal alicuota,
        decimal minimoBaseCalculo = 0m, string tipo = "percepcion", string? observacion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (alicuota < 0) throw new ArgumentException("La alícuota no puede ser negativa.");

        return new Impuesto
        {
            Codigo             = codigo.Trim().ToUpperInvariant(),
            Descripcion        = descripcion.Trim(),
            Observacion        = observacion?.Trim(),
            Alicuota           = alicuota,
            MinimoBaseCalculo  = minimoBaseCalculo,
            Tipo               = tipo.Trim().ToLowerInvariant(),
            Activo             = true
        };
    }

    public void Actualizar(string descripcion, decimal alicuota, decimal minimoBaseCalculo,
        string tipo, string? observacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion       = descripcion.Trim();
        Alicuota          = alicuota;
        MinimoBaseCalculo = minimoBaseCalculo;
        Tipo              = tipo.Trim().ToLowerInvariant();
        Observacion       = observacion?.Trim();
    }

    public void Activar()    => Activo = true;
    public void Desactivar() => Activo = false;
}
