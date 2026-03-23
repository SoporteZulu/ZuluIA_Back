using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Régimen de retención: parámetros que definen cómo se calcula la retención
/// para una retención específica (base imponible, mínimos, máximos, alícuotas, etc.).
/// Migrado desde VB6: clsRetencionesRegimenes / RETENCIONES_REGIMENES_PARAMETROS.
/// </summary>
public class RetencionRegimen : BaseEntity
{
    public string  Codigo                         { get; private set; } = string.Empty;
    public string  Descripcion                    { get; private set; } = string.Empty;
    public string? Observacion                    { get; private set; }
    public long    RetencionId                    { get; private set; }
    public bool    ControlTipoComprobante         { get; private set; }
    public bool    ControlTipoComprobanteAplica   { get; private set; }
    public string? BaseImponibleComposicion       { get; private set; }
    public decimal? NoImponible                   { get; private set; }
    public bool    NoImponibleAplica              { get; private set; }
    public decimal? BaseImponiblePorcentaje       { get; private set; }
    public bool    BaseImponiblePorcentajeAplica  { get; private set; }
    public decimal? BaseImponibleMinimo           { get; private set; }
    public bool    BaseImponibleMinimoAplica      { get; private set; }
    public decimal? BaseImponibleMaximo           { get; private set; }
    public bool    BaseImponibleMaximoAplica      { get; private set; }
    public string? RetencionComposicion           { get; private set; }
    public decimal? RetencionMinimo               { get; private set; }
    public bool    RetencionMinimoAplica          { get; private set; }
    public decimal? RetencionMaximo               { get; private set; }
    public bool    RetencionMaximoAplica          { get; private set; }
    public decimal? Alicuota                      { get; private set; }
    public bool    AlicuotaAplica                 { get; private set; }
    public bool    AlicuotaEscalaAplica           { get; private set; }
    public decimal? AlicuotaConvenio              { get; private set; }
    public bool    AlicuotaConvenioAplica         { get; private set; }

    private RetencionRegimen() { }

    public static RetencionRegimen Crear(string codigo, string descripcion,
        long retencionId, string? observacion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (retencionId <= 0) throw new ArgumentException("La retención padre es requerida.");

        return new RetencionRegimen
        {
            Codigo        = codigo.Trim().ToUpperInvariant(),
            Descripcion   = descripcion.Trim(),
            RetencionId   = retencionId,
            Observacion   = observacion?.Trim()
        };
    }

    public void ActualizarParametros(
        bool controlTipoComprobante, bool controlTipoComprobanteAplica,
        string? baseImponibleComposicion, decimal? noImponible, bool noImponibleAplica,
        decimal? baseImponiblePorcentaje, bool baseImponiblePorcentajeAplica,
        decimal? baseImponibleMinimo, bool baseImponibleMinimoAplica,
        decimal? baseImponibleMaximo, bool baseImponibleMaximoAplica,
        string? retencionComposicion, decimal? retencionMinimo, bool retencionMinimoAplica,
        decimal? retencionMaximo, bool retencionMaximoAplica,
        decimal? alicuota, bool alicuotaAplica, bool alicuotaEscalaAplica,
        decimal? alicuotaConvenio, bool alicuotaConvenioAplica,
        string? observacion)
    {
        ControlTipoComprobante        = controlTipoComprobante;
        ControlTipoComprobanteAplica  = controlTipoComprobanteAplica;
        BaseImponibleComposicion      = baseImponibleComposicion?.Trim();
        NoImponible                   = noImponible;
        NoImponibleAplica             = noImponibleAplica;
        BaseImponiblePorcentaje       = baseImponiblePorcentaje;
        BaseImponiblePorcentajeAplica = baseImponiblePorcentajeAplica;
        BaseImponibleMinimo           = baseImponibleMinimo;
        BaseImponibleMinimoAplica     = baseImponibleMinimoAplica;
        BaseImponibleMaximo           = baseImponibleMaximo;
        BaseImponibleMaximoAplica     = baseImponibleMaximoAplica;
        RetencionComposicion          = retencionComposicion?.Trim();
        RetencionMinimo               = retencionMinimo;
        RetencionMinimoAplica         = retencionMinimoAplica;
        RetencionMaximo               = retencionMaximo;
        RetencionMaximoAplica         = retencionMaximoAplica;
        Alicuota                      = alicuota;
        AlicuotaAplica                = alicuotaAplica;
        AlicuotaEscalaAplica          = alicuotaEscalaAplica;
        AlicuotaConvenio              = alicuotaConvenio;
        AlicuotaConvenioAplica        = alicuotaConvenioAplica;
        Observacion                   = observacion?.Trim();
    }
}
