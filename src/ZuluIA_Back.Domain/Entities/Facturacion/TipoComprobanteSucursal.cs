using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

/// <summary>
/// Configuración de un tipo de comprobante para una sucursal (numeración, copias, moneda, etc.).
/// Define el próximo número de comprobante y otras configuraciones de impresión por sucursal.
/// Migrado desde VB6: clsTiposComprobantesSucursal / TiposComprobantesSucursal.
/// </summary>
public class TipoComprobanteSucursal : BaseEntity
{
    public long  TipoComprobanteId          { get; private set; }
    public long? SucursalId                 { get; private set; }
    public long  NumeroComprobanteProximo   { get; private set; }
    public int   FilasCantidad              { get; private set; }
    public int   FilasAnchoMaximo           { get; private set; }
    public int   CantidadCopias            { get; private set; }
    public bool  ImprimirControladorFiscal  { get; private set; }
    public bool  VarianteNroUnico           { get; private set; }
    public bool  PermitirSeleccionMoneda    { get; private set; }
    public long? MonedaId                   { get; private set; }
    public bool  Editable                   { get; private set; } = true;
    public bool  VistaPrevia                { get; private set; }
    public bool  ControlIntervalo           { get; private set; }
    public long? NumeroDesde                { get; private set; }
    public long? NumeroHasta                { get; private set; }

    private TipoComprobanteSucursal() { }

    public static TipoComprobanteSucursal Crear(long tipoComprobanteId, long? sucursalId = null,
        long numeroProximo = 1, int filasCantidad = 0, int filasAnchoMaximo = 0,
        int cantidadCopias = 1, bool imprimirControladorFiscal = false,
        bool varianteNroUnico = false, bool permitirSeleccionMoneda = false,
        long? monedaId = null, bool editable = true, bool vistaPrevia = false,
        bool controlIntervalo = false, long? numeroDesde = null, long? numeroHasta = null)
    {
        if (tipoComprobanteId <= 0) throw new ArgumentException("El tipo de comprobante es requerido.");
        if (numeroProximo     <= 0) throw new ArgumentException("El número próximo debe ser mayor a cero.");

        return new TipoComprobanteSucursal
        {
            TipoComprobanteId         = tipoComprobanteId,
            SucursalId                = sucursalId,
            NumeroComprobanteProximo  = numeroProximo,
            FilasCantidad             = filasCantidad,
            FilasAnchoMaximo          = filasAnchoMaximo,
            CantidadCopias            = cantidadCopias,
            ImprimirControladorFiscal = imprimirControladorFiscal,
            VarianteNroUnico          = varianteNroUnico,
            PermitirSeleccionMoneda   = permitirSeleccionMoneda,
            MonedaId                  = monedaId,
            Editable                  = editable,
            VistaPrevia               = vistaPrevia,
            ControlIntervalo          = controlIntervalo,
            NumeroDesde               = numeroDesde,
            NumeroHasta               = numeroHasta
        };
    }

    public void IncrementarNumero() => NumeroComprobanteProximo++;

    public void ActualizarConfiguracion(int filasCantidad, int filasAnchoMaximo,
        int cantidadCopias, bool imprimirControladorFiscal, bool varianteNroUnico,
        bool permitirSeleccionMoneda, long? monedaId, bool editable, bool vistaPrevia,
        bool controlIntervalo, long? numeroDesde, long? numeroHasta)
    {
        FilasCantidad             = filasCantidad;
        FilasAnchoMaximo          = filasAnchoMaximo;
        CantidadCopias            = cantidadCopias;
        ImprimirControladorFiscal = imprimirControladorFiscal;
        VarianteNroUnico          = varianteNroUnico;
        PermitirSeleccionMoneda   = permitirSeleccionMoneda;
        MonedaId                  = monedaId;
        Editable                  = editable;
        VistaPrevia               = vistaPrevia;
        ControlIntervalo          = controlIntervalo;
        NumeroDesde               = numeroDesde;
        NumeroHasta               = numeroHasta;
    }
}
