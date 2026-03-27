using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

/// <summary>
/// Categoría de tercero con parámetros financieros de crédito e interés.
/// Migrado desde VB6: clsCategoriaPersonas / CATEGORIAPERSONAS.
/// </summary>
public class CategoriaTercero : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool EsImportador { get; private set; }
    /// <summary>Vínculo opcional a tipo de persona (cliente/proveedor/ambos).</summary>
    public int? TipoPersonaId { get; private set; }
    /// <summary>True si es una categoría creada por el sistema (no editable).</summary>
    public bool EsSistema { get; private set; }
    /// <summary>Cuenta contable asignada por defecto a los terceros de esta categoría.</summary>
    public long? CuentaContableDefectoId { get; private set; }
    /// <summary>Fecha de referencia para cálculo de intereses.</summary>
    public DateOnly? FechaReferencia { get; private set; }
    /// <summary>Cada cuántos días se actualiza la categoría.</summary>
    public int DiasFrecuenciaActualizacion { get; private set; }
    /// <summary>Días de vencimiento estándar de facturas.</summary>
    public int DiasVencimiento { get; private set; }
    /// <summary>Días de financiación estándar.</summary>
    public int DiasFinanciacion { get; private set; }
    /// <summary>Tasa de interés diaria (ej: 0.001 = 0.1%).</summary>
    public decimal TasaInteresDiaria { get; private set; }
    /// <summary>Si se debe cobrar interés por mora.</summary>
    public bool CobrarInteres { get; private set; }

    private CategoriaTercero() { }

    public static CategoriaTercero Crear(string descripcion, bool esImportador = false,
        int? tipoPersonaId = null, long? cuentaContableDefectoId = null,
        int diasVencimiento = 0, int diasFinanciacion = 0,
        decimal tasaInteresDiaria = 0m, bool cobrarInteres = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new CategoriaTercero
        {
            Descripcion              = descripcion.Trim(),
            EsImportador             = esImportador,
            TipoPersonaId            = tipoPersonaId,
            CuentaContableDefectoId  = cuentaContableDefectoId,
            DiasVencimiento          = diasVencimiento,
            DiasFinanciacion         = diasFinanciacion,
            TasaInteresDiaria        = tasaInteresDiaria,
            CobrarInteres            = cobrarInteres
        };
    }

    public void Actualizar(string descripcion, bool esImportador, int? tipoPersonaId,
        long? cuentaContableDefectoId, int diasVencimiento, int diasFinanciacion,
        decimal tasaInteresDiaria, bool cobrarInteres)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion             = descripcion.Trim();
        EsImportador            = esImportador;
        TipoPersonaId           = tipoPersonaId;
        CuentaContableDefectoId = cuentaContableDefectoId;
        DiasVencimiento         = diasVencimiento;
        DiasFinanciacion        = diasFinanciacion;
        TasaInteresDiaria       = tasaInteresDiaria;
        CobrarInteres           = cobrarInteres;
    }
}