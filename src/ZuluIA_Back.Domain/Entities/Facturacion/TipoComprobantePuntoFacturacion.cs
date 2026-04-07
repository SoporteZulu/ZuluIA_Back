using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

/// <summary>
/// Asignación de un tipo de comprobante a un punto de facturación,
/// con la configuración específica: número siguiente, copias, reportes, etc.
/// Migrado desde VB6: frmAjustesPV / TIPOSCOMPROBANTESPUNTOFACTURACION.
/// </summary>
public class TipoComprobantePuntoFacturacion : BaseEntity
{
    public long   PuntoFacturacionId          { get; private set; }   // pfac_id
    public long   TipoComprobanteId            { get; private set; }   // id_tipocomprobante
    public long   NumeroComprobanteProximo     { get; private set; }
    public bool   Editable                     { get; private set; }
    public int    FilasCantidad                { get; private set; }
    public int    FilasAnchoMaximo             { get; private set; }
    public long?  ReporteId                    { get; private set; }   // id_reporte
    public int    CantidadCopias               { get; private set; } = 1;
    public bool   VistaPrevia                  { get; private set; }
    public bool   ImprimirControladorFiscal    { get; private set; }
    public bool   PermitirSeleccionMoneda      { get; private set; }
    public int?   VarianteNroUnico             { get; private set; }
    public string? MascaraMoneda              { get; private set; }

    private TipoComprobantePuntoFacturacion() { }

    public static TipoComprobantePuntoFacturacion Crear(
        long   puntoFacturacionId,
        long   tipoComprobanteId,
        long   numeroComprobanteProximo   = 1,
        bool   editable                   = true,
        int    filasCantidad              = 0,
        int    filasAnchoMaximo           = 0,
        long?  reporteId                  = null,
        int    cantidadCopias             = 1,
        bool   vistaPrevia                = false,
        bool   imprimirControladorFiscal  = false,
        bool   permitirSeleccionMoneda    = false,
        int?   varianteNroUnico           = null,
        string? mascaraMoneda             = null)
    {
        if (puntoFacturacionId  <= 0) throw new ArgumentException("El punto de facturación es requerido.");
        if (tipoComprobanteId   <= 0) throw new ArgumentException("El tipo de comprobante es requerido.");

        return new TipoComprobantePuntoFacturacion
        {
            PuntoFacturacionId         = puntoFacturacionId,
            TipoComprobanteId           = tipoComprobanteId,
            NumeroComprobanteProximo    = numeroComprobanteProximo,
            Editable                    = editable,
            FilasCantidad               = filasCantidad,
            FilasAnchoMaximo            = filasAnchoMaximo,
            ReporteId                   = reporteId,
            CantidadCopias              = cantidadCopias,
            VistaPrevia                 = vistaPrevia,
            ImprimirControladorFiscal   = imprimirControladorFiscal,
            PermitirSeleccionMoneda     = permitirSeleccionMoneda,
            VarianteNroUnico            = varianteNroUnico,
            MascaraMoneda               = mascaraMoneda?.Trim()
        };
    }

    public void Actualizar(
        long   numeroComprobanteProximo,
        bool   editable,
        int    filasCantidad,
        int    filasAnchoMaximo,
        long?  reporteId,
        int    cantidadCopias,
        bool   vistaPrevia,
        bool   imprimirControladorFiscal,
        bool   permitirSeleccionMoneda,
        int?   varianteNroUnico,
        string? mascaraMoneda)
    {
        NumeroComprobanteProximo  = numeroComprobanteProximo;
        Editable                  = editable;
        FilasCantidad             = filasCantidad;
        FilasAnchoMaximo          = filasAnchoMaximo;
        ReporteId                 = reporteId;
        CantidadCopias            = cantidadCopias;
        VistaPrevia               = vistaPrevia;
        ImprimirControladorFiscal = imprimirControladorFiscal;
        PermitirSeleccionMoneda   = permitirSeleccionMoneda;
        VarianteNroUnico          = varianteNroUnico;
        MascaraMoneda             = mascaraMoneda?.Trim();
    }
}
