using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Configuración de un tipo de retención (Ganancias, IIBB, IVA, SUSS, etc.).
/// Equivale a la tabla RETENCION del sistema VB6.
/// Cada TipoRetencion puede tener múltiples escalas/tramos de porcentaje.
/// </summary>
public class TipoRetencion : AuditableEntity
{
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Régimen de retención (ej: "IIBB CÓRDOBA", "GANANCIAS", "IVA", "SUSS").
    /// </summary>
    public string Regimen { get; private set; } = string.Empty;

    /// <summary>
    /// Mínimo no imponible: importe a partir del cual se calcula la retención.
    /// </summary>
    public decimal MinimoNoImponible { get; private set; }

    /// <summary>
    /// Indica si acumula la base de cálculo para pagos del mismo proveedor
    /// durante el período fiscal (comportamiento del VB6: RET_ACUMULA_PAGO).
    /// </summary>
    public bool AcumulaPago { get; private set; }

    /// <summary>
    /// Tipo de comprobante que genera la retención (FK referencial).
    /// </summary>
    public long? TipoComprobanteId { get; private set; }

    /// <summary>
    /// Ítem/concepto asociado a la retención en el comprobante.
    /// </summary>
    public long? ItemId { get; private set; }

    public bool Activo { get; private set; } = true;

    private readonly List<EscalaRetencion> _escalas = [];
    public IReadOnlyCollection<EscalaRetencion> Escalas => _escalas.AsReadOnly();

    private TipoRetencion() { }

    public static TipoRetencion Crear(
        string descripcion,
        string regimen,
        decimal minimoNoImponible,
        bool acumulaPago,
        long? tipoComprobanteId,
        long? itemId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        ArgumentException.ThrowIfNullOrWhiteSpace(regimen);

        var t = new TipoRetencion
        {
            Descripcion        = descripcion.Trim(),
            Regimen            = regimen.Trim().ToUpperInvariant(),
            MinimoNoImponible  = minimoNoImponible < 0 ? 0 : minimoNoImponible,
            AcumulaPago        = acumulaPago,
            TipoComprobanteId  = tipoComprobanteId,
            ItemId             = itemId,
            Activo             = true
        };

        t.SetCreated(userId);
        return t;
    }

    public void Actualizar(
        string descripcion,
        string regimen,
        decimal minimoNoImponible,
        bool acumulaPago,
        long? tipoComprobanteId,
        long? itemId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        ArgumentException.ThrowIfNullOrWhiteSpace(regimen);

        Descripcion       = descripcion.Trim();
        Regimen           = regimen.Trim().ToUpperInvariant();
        MinimoNoImponible = minimoNoImponible < 0 ? 0 : minimoNoImponible;
        AcumulaPago       = acumulaPago;
        TipoComprobanteId = tipoComprobanteId;
        ItemId            = itemId;
        SetUpdated(userId);
    }

    public void AgregarEscala(
        string descripcion,
        decimal importeDesde,
        decimal importeHasta,
        decimal porcentaje)
    {
        if (importeDesde < 0)
            throw new InvalidOperationException("El importe desde no puede ser negativo.");
        if (importeHasta > 0 && importeHasta < importeDesde)
            throw new InvalidOperationException("El importe hasta debe ser mayor al importe desde.");
        if (porcentaje < 0 || porcentaje > 100)
            throw new InvalidOperationException("El porcentaje debe estar entre 0 y 100.");

        _escalas.Add(EscalaRetencion.Crear(Id, descripcion, importeDesde, importeHasta, porcentaje));
    }

    public void RemoverEscalas()
    {
        _escalas.Clear();
    }

    public void Dar_De_Baja(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    /// <summary>
    /// Calcula el importe de retención para una base imponible dada,
    /// aplicando la escala correspondiente al tramo.
    /// </summary>
    public decimal CalcularImporte(decimal baseImponible)
    {
        if (baseImponible <= MinimoNoImponible)
            return 0;

        var baseEfectiva = baseImponible - MinimoNoImponible;

        // Si no hay escalas, no se puede calcular
        if (!_escalas.Any())
            return 0;

        // Buscar el tramo correspondiente a la base efectiva
        var escala = _escalas
            .Where(e => e.ImporteDesde <= baseImponible &&
                        (e.ImporteHasta == 0 || e.ImporteHasta >= baseImponible))
            .OrderByDescending(e => e.ImporteDesde)
            .FirstOrDefault();

        if (escala is null)
            return 0;

        return Math.Round(baseEfectiva * (escala.Porcentaje / 100m), 2);
    }
}
