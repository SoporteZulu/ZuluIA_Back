using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Comprobantes;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class Comprobante : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long? PuntoFacturacionId { get; private set; }
    public long TipoComprobanteId { get; private set; }
    public NroComprobante Numero { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }
    public long TerceroId { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;
    
    // Datos comerciales
    public long? VendedorId { get; private set; }
    public long? CobradorId { get; private set; }
    public long? ZonaComercialId { get; private set; }
    public long? ListaPreciosId { get; private set; }
    public long? CondicionPagoId { get; private set; }
    public int? PlazoDias { get; private set; }
    public long? CanalVentaId { get; private set; }
    public decimal? PorcentajeComisionVendedor { get; private set; }
    public decimal? PorcentajeComisionCobrador { get; private set; }
    public decimal? ImporteComisionVendedor { get; private set; } // AGREGADO - se persiste el cálculo
    
    // Datos logísticos (para remitos principalmente)
    public long? TransporteId { get; private set; }
    public string? ChoferNombre { get; private set; }
    public string? ChoferDni { get; private set; }
    public string? PatVehiculo { get; private set; }
    public string? PatAcoplado { get; private set; }
    public string? RutaLogistica { get; private set; } // AGREGADO
    public string? DomicilioEntrega { get; private set; }
    public string? ObservacionesLogisticas { get; private set; }
    public DateOnly? FechaEstimadaEntrega { get; private set; }
    public DateOnly? FechaRealEntrega { get; private set; }
    public string? FirmaConformidad { get; private set; }
    public string? NombreQuienRecibe { get; private set; }
    public string? DniQuienRecibe { get; private set; } // AGREGADO
    public EstadoLogisticoRemito? EstadoLogistico { get; private set; }
    public bool EsValorizado { get; private set; } = true;
    public long? DepositoOrigenId { get; private set; }
    
    // Datos específicos de carga (remitos)
    public decimal? PesoTotal { get; private set; } // AGREGADO (kg)
    public decimal? VolumenTotal { get; private set; } // AGREGADO (m³)
    public int? Bultos { get; private set; } // AGREGADO
    public string? TipoEmbalaje { get; private set; } // AGREGADO
    public bool? SeguroTransporte { get; private set; } // AGREGADO
    public decimal? ValorDeclarado { get; private set; } // AGREGADO

    // Datos de pedido
    public DateOnly? FechaEntregaCompromiso { get; private set; }
    public EstadoPedido? EstadoPedido { get; private set; }
    public string? MotivoCierrePedido { get; private set; }
    public DateTimeOffset? FechaCierrePedido { get; private set; }
    public PrioridadPedido? Prioridad { get; private set; } // AGREGADO
    public bool StockReservado { get; private set; } // AGREGADO
    public long? UsuarioAprobadorId { get; private set; } // AGREGADO
    public DateTimeOffset? FechaAprobacion { get; private set; } // AGREGADO
    public string? MotivoRechazo { get; private set; } // AGREGADO
    
    // Datos adicionales comerciales/fiscales
    public string? ObservacionInterna { get; private set; }
    public string? ObservacionFiscal { get; private set; }
    public decimal? RecargoPorcentaje { get; private set; }
    public decimal? RecargoImporte { get; private set; }
    public decimal? DescuentoPorcentaje { get; private set; }
    
    // Snapshot de datos del tercero al momento de emisión
    public string? TerceroDomicilioSnapshot { get; private set; }
    public string? TerceroRazonSocialSnapshot { get; private set; } // AGREGADO
    public string? TerceroCondicionIvaSnapshot { get; private set; } // AGREGADO

    // Datos específicos de Notas de Débito/Crédito
    public long? MotivoDebitoId { get; private set; }
    public string? MotivoDebitoObservacion { get; private set; }
    public DateTimeOffset? FechaAnulacion { get; private set; }
    public long? UsuarioAnulacionId { get; private set; }
    public string? MotivoAnulacion { get; private set; }
    
    // Metadatos de devolución
    public MotivoDevolucion? MotivoDevolucion { get; private set; }
    public TipoDevolucion? TipoDevolucion { get; private set; }
    public long? AutorizadorDevolucionId { get; private set; }
    public DateTimeOffset? FechaAutorizacionDevolucion { get; private set; }
    public string? ObservacionDevolucion { get; private set; }
    public bool ReingresaStock { get; private set; }
    public bool AcreditaCuentaCorriente { get; private set; }
    
    public decimal Subtotal { get; private set; }
    public decimal DescuentoImporte { get; private set; }
    public decimal NetoGravado { get; private set; }
    public decimal NetoNoGravado { get; private set; }
    public decimal IvaRi { get; private set; }
    public decimal IvaRni { get; private set; }
    public decimal Percepciones { get; private set; }
    public decimal Retenciones { get; private set; }
    public decimal Total { get; private set; }
    public decimal Saldo { get; private set; }
    
    public long? TimbradoId { get; private set; }
    public string? NroTimbrado { get; private set; }
    public EstadoSifenParaguay? EstadoSifen { get; private set; }
    public string? SifenCodigoRespuesta { get; private set; }
    public string? SifenMensajeRespuesta { get; private set; }
    public string? SifenTrackingId { get; private set; }
    public string? SifenCdc { get; private set; }
    public string? SifenNumeroLote { get; private set; }
    public DateTimeOffset? SifenFechaRespuesta { get; private set; }
    public string? Cae { get; private set; }
    public string? Caea { get; private set; }
    public DateOnly? FechaVtoCae { get; private set; }
    public string? QrData { get; private set; }
    public EstadoAfipWsfe EstadoAfip { get; private set; } = EstadoAfipWsfe.Pendiente;
    public string? UltimoErrorAfip { get; private set; }
    public DateTimeOffset? FechaUltimaConsultaAfip { get; private set; }
    public EstadoComprobante Estado { get; private set; }
    public string? Observacion { get; private set; }

    public long? ComprobanteOrigenId { get; private set; }

    public ComprobanteCot? Cot { get; private set; }

    private readonly List<ComprobanteItem> _items = [];
    public IReadOnlyCollection<ComprobanteItem> Items => _items.AsReadOnly();
    private readonly List<ComprobanteAtributo> _atributos = [];
    public IReadOnlyCollection<ComprobanteAtributo> Atributos => _atributos.AsReadOnly();

    private Comprobante() { }

    public static Comprobante Crear(
        long sucursalId,
        long? puntoFacturacionId,
        long tipoComprobanteId,
        short prefijo,
        long numero,
        DateOnly fecha,
        DateOnly? fechaVencimiento,
        long terceroId,
        long monedaId,
        decimal cotizacion,
        string? observacion,
        long? userId)
    {
        var comp = new Comprobante
        {
            SucursalId          = sucursalId,
            PuntoFacturacionId  = puntoFacturacionId,
            TipoComprobanteId   = tipoComprobanteId,
            Numero              = new NroComprobante(prefijo, numero),
            Fecha               = fecha,
            FechaVencimiento    = fechaVencimiento,
            TerceroId           = terceroId,
            MonedaId            = monedaId,
            Cotizacion          = cotizacion <= 0 ? 1 : cotizacion,
            Estado              = EstadoComprobante.Borrador,
            Observacion         = observacion?.Trim()
        };

        comp.SetCreated(userId);
        return comp;
    }

    public void AgregarItem(ComprobanteItem item)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se pueden agregar ítems a comprobantes en borrador.");

        _items.Add(item);
        RecalcularTotales();
    }

    public void RemoverItem(long itemId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se pueden remover ítems de comprobantes en borrador.");

        var item = _items.FirstOrDefault(x => x.Id == itemId);
        if (item is not null)
        {
            _items.Remove(item);
            RecalcularTotales();
        }
    }

    public void AgregarAtributo(ComprobanteAtributo atributo, long? userId)
    {
        ArgumentNullException.ThrowIfNull(atributo);

        if (_atributos.Any(x => string.Equals(x.Clave, atributo.Clave, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe un atributo con la clave '{atributo.Clave}'.");

        _atributos.Add(atributo);
        SetUpdated(userId);
    }

    public void AsignarCot(ComprobanteCot cot, long? userId)
    {
        ArgumentNullException.ThrowIfNull(cot);

        if (cot.ComprobanteId != Id && Id != 0)
            throw new InvalidOperationException("El COT no pertenece a este comprobante.");

        Cot = cot;
        SetUpdated(userId);
    }

    public void RecalcularTotales()
    {
        Subtotal        = _items.Sum(x => x.TotalLinea + x.IvaImporte);
        NetoGravado     = _items.Where(x => x.EsGravado).Sum(x => x.SubtotalNeto);
        NetoNoGravado   = _items.Where(x => !x.EsGravado).Sum(x => x.SubtotalNeto);
        IvaRi           = _items.Sum(x => x.IvaImporte);
        IvaRni          = 0;
        DescuentoImporte = _items.Sum(x =>
            x.PrecioUnitario * x.Cantidad * (x.DescuentoPct / 100));

        Total  = NetoGravado + NetoNoGravado + IvaRi + IvaRni +
                 Percepciones - Retenciones;
        Saldo  = Total;
    }

    public void SetPercepciones(decimal percepciones, long? userId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se pueden modificar percepciones en comprobantes borrador.");

        Percepciones = percepciones;
        RecalcularTotales();
        SetUpdated(userId);
    }

    public void SetRetenciones(decimal retenciones, long? userId)
    {
        Retenciones = retenciones;
        RecalcularTotales();
        SetUpdated(userId);
    }

    public void AplicarPago(decimal importe)
    {
        if (Estado != EstadoComprobante.Emitido)
        {
            throw new InvalidOperationException("Solo se puede aplicar un pago a un comprobante emitido.");
        }

        if (importe <= 0)
        {
            throw new ArgumentException("El importe debe ser mayor a cero.", nameof(importe));
        }

        Saldo -= importe;

        if (Saldo <= 0)
        {
            Saldo = 0;
            Estado = EstadoComprobante.Pagado;
        }
        else
        {
            Estado = EstadoComprobante.PagadoParcial;
        }
    }

    public void Emitir(long? userId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException($"No se puede emitir un comprobante en estado {Estado}.");

        if (!_items.Any())
            throw new InvalidOperationException("No se puede emitir un comprobante sin ítems.");

        Estado = EstadoComprobante.Emitido;
        Saldo  = Total;
        AddDomainEvent(new ComprobanteEmitidoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
        SetUpdated(userId);
    }

    /// <summary>
    /// Asigna datos comerciales del comprobante para mantener trazabilidad operativa.
    /// </summary>
    public void AsignarDatosComerciales(
        long? vendedorId,
        long? cobradorId,
        long? zonaComercialId,
        long? listaPreciosId,
        long? condicionPagoId,
        int? plazoDias,
        long? canalVentaId,
        decimal? porcentajeComisionVendedor,
        decimal? porcentajeComisionCobrador,
        long? userId)
    {
        if (plazoDias.HasValue && plazoDias.Value < 0)
        {
            throw new ArgumentException("El plazo de días no puede ser negativo.", nameof(plazoDias));
        }

        VendedorId = vendedorId;
        CobradorId = cobradorId;
        ZonaComercialId = zonaComercialId;
        ListaPreciosId = listaPreciosId;
        CondicionPagoId = condicionPagoId;
        PlazoDias = plazoDias;
        CanalVentaId = canalVentaId;
        PorcentajeComisionVendedor = porcentajeComisionVendedor;
        PorcentajeComisionCobrador = porcentajeComisionCobrador;
        SetUpdated(userId);
    }

    public void AsignarCae(string cae, DateOnly fechaVto, string? qrData, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cae);
        Cae         = cae.Trim();
        Caea        = null;
        FechaVtoCae = fechaVto;
        QrData      = qrData;
        EstadoAfip  = EstadoAfipWsfe.AutorizadoCae;
        UltimoErrorAfip = null;
        SetUpdated(userId);
    }

    public void AsignarCaea(string caea, DateOnly fechaVto, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caea);
        Caea = caea.Trim();
        FechaVtoCae = fechaVto;
        EstadoAfip = EstadoAfipWsfe.AutorizadoCaea;
        UltimoErrorAfip = null;
        SetUpdated(userId);
    }

    public void RegistrarEstadoAfip(EstadoAfipWsfe estadoAfip, string? ultimoError, DateTimeOffset fechaConsulta, long? userId)
    {
        EstadoAfip = estadoAfip;
        UltimoErrorAfip = ultimoError?.Trim();
        FechaUltimaConsultaAfip = fechaConsulta;
        SetUpdated(userId);
    }

    public void AsignarTimbrado(long timbradoId, string nroTimbrado, long? userId)
    {
        if (timbradoId <= 0)
            throw new ArgumentException("El timbrado es requerido.", nameof(timbradoId));

        ArgumentException.ThrowIfNullOrWhiteSpace(nroTimbrado);

        TimbradoId = timbradoId;
        NroTimbrado = nroTimbrado.Trim();
        SetUpdated(userId);
    }

    public void RegistrarResultadoSifen(
        EstadoSifenParaguay estado,
        string? codigoRespuesta,
        string? mensajeRespuesta,
        string? trackingId,
        string? cdc,
        string? numeroLote,
        DateTimeOffset? fechaRespuesta,
        long? userId)
    {
        EstadoSifen = estado;
        SifenCodigoRespuesta = string.IsNullOrWhiteSpace(codigoRespuesta) ? null : codigoRespuesta.Trim();
        SifenMensajeRespuesta = string.IsNullOrWhiteSpace(mensajeRespuesta) ? null : mensajeRespuesta.Trim();
        SifenTrackingId = string.IsNullOrWhiteSpace(trackingId) ? null : trackingId.Trim();
        SifenCdc = string.IsNullOrWhiteSpace(cdc) ? null : cdc.Trim();
        SifenNumeroLote = string.IsNullOrWhiteSpace(numeroLote) ? null : numeroLote.Trim();
        SifenFechaRespuesta = fechaRespuesta ?? DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("El comprobante ya está anulado.");

        Estado = EstadoComprobante.Anulado;
        Saldo  = 0;
        FechaAnulacion = DateTimeOffset.UtcNow;
        UsuarioAnulacionId = userId;
        AddDomainEvent(new ComprobanteAnuladoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
        SetDeleted();
        SetUpdated(userId);
    }

    public void AnularConMotivo(string motivoAnulacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(motivoAnulacion, nameof(motivoAnulacion));
        
        if (Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("El comprobante ya está anulado.");

        Estado = EstadoComprobante.Anulado;
        Saldo  = 0;
        FechaAnulacion = DateTimeOffset.UtcNow;
        UsuarioAnulacionId = userId;
        MotivoAnulacion = motivoAnulacion.Trim();
        AddDomainEvent(new ComprobanteAnuladoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
        SetDeleted();
        SetUpdated(userId);
    }

    public void SetMotivoDebito(long? motivoDebitoId, string? observacion, long? userId)
    {
        MotivoDebitoId = motivoDebitoId;
        MotivoDebitoObservacion = string.IsNullOrWhiteSpace(observacion) 
            ? null 
            : observacion.Trim();
        SetUpdated(userId);
    }

    /// <summary>
    /// Marca un presupuesto/comprobante borrador como convertido a un comprobante definitivo.
    /// Equivale a la acción "Convertir Presupuesto a Factura" del VB6.
    /// </summary>
    public void MarcarComoConvertido(long? userId)
    {
        if (Estado is not (EstadoComprobante.Borrador or EstadoComprobante.Emitido))
            throw new InvalidOperationException("Solo se puede marcar como Convertido un presupuesto en estado Borrador o Emitido.");

        Estado = EstadoComprobante.Convertido;
        SetUpdated(userId);
    }

    /// <summary>Establece el comprobante de origen cuando se convierte un presupuesto.</summary>
    public void SetComprobanteOrigen(long origenId) => ComprobanteOrigenId = origenId;

    public void VincularAComprobanteOrigen(long origenId, long? userId)
    {
        if (origenId <= 0)
            throw new InvalidOperationException("El comprobante origen es obligatorio.");

        if (Id != 0 && origenId == Id)
            throw new InvalidOperationException("No se puede vincular un comprobante consigo mismo.");

        if (ComprobanteOrigenId.HasValue && ComprobanteOrigenId.Value != origenId)
            throw new InvalidOperationException("El comprobante ya tiene un origen comercial distinto.");

        ComprobanteOrigenId = origenId;
        SetUpdated(userId);
    }

    public void ActualizarSaldo(decimal importeImputado, long? userId)
    {
        Saldo -= importeImputado;
        if (Saldo < 0) Saldo = 0;

        if (Saldo == 0)
            Estado = EstadoComprobante.Pagado;
        else if (Saldo < Total)
            Estado = EstadoComprobante.PagadoParcial;

        SetUpdated(userId);
    }

    public void RevertirSaldo(decimal importeDesimputado, long? userId)
    {
        if (importeDesimputado <= 0)
            throw new InvalidOperationException("El importe a revertir debe ser mayor a 0.");

        if (Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede revertir saldo sobre un comprobante anulado.");

        Saldo += importeDesimputado;
        if (Saldo > Total)
            Saldo = Total;

        if (Saldo == 0)
            Estado = EstadoComprobante.Pagado;
        else if (Saldo < Total)
            Estado = EstadoComprobante.PagadoParcial;
        else
            Estado = EstadoComprobante.Emitido;

        SetUpdated(userId);
    }

    public void SetFechaVencimiento(DateOnly fecha, long? userId)
    {
        FechaVencimiento = fecha;
        SetUpdated(userId);
    }

    public void SetDatosComerciales(
        long? vendedorId,
        long? cobradorId,
        long? zonaComercialId,
        long? listaPreciosId,
        long? condicionPagoId,
        int? plazoDias,
        long? canalVentaId,
        decimal? porcentajeComisionVendedor,
        decimal? porcentajeComisionCobrador,
        long? userId)
    {
        VendedorId = vendedorId;
        CobradorId = cobradorId;
        ZonaComercialId = zonaComercialId;
        ListaPreciosId = listaPreciosId;
        CondicionPagoId = condicionPagoId;
        PlazoDias = plazoDias;
        CanalVentaId = canalVentaId;
        PorcentajeComisionVendedor = porcentajeComisionVendedor;
        PorcentajeComisionCobrador = porcentajeComisionCobrador;
        
        // Calcular y persistir importe comisión vendedor
        if (porcentajeComisionVendedor.HasValue && porcentajeComisionVendedor.Value > 0)
        {
            ImporteComisionVendedor = Total * (porcentajeComisionVendedor.Value / 100m);
        }
        else
        {
            ImporteComisionVendedor = null;
        }
        
        SetUpdated(userId);
    }

    public void SetDatosLogisticos(
        long? transporteId,
        string? choferNombre,
        string? choferDni,
        string? patVehiculo,
        string? patAcoplado,
        string? rutaLogistica,
        string? domicilioEntrega,
        string? observacionesLogisticas,
        DateOnly? fechaEstimadaEntrega,
        string? dniQuienRecibe,
        decimal? pesoTotal,
        decimal? volumenTotal,
        int? bultos,
        string? tipoEmbalaje,
        bool? seguroTransporte,
        decimal? valorDeclarado,
        long? userId)
    {
        TransporteId = transporteId;
        ChoferNombre = choferNombre?.Trim();
        ChoferDni = choferDni?.Trim();
        PatVehiculo = patVehiculo?.Trim();
        PatAcoplado = patAcoplado?.Trim();
        RutaLogistica = rutaLogistica?.Trim();
        DomicilioEntrega = domicilioEntrega?.Trim();
        ObservacionesLogisticas = observacionesLogisticas?.Trim();
        FechaEstimadaEntrega = fechaEstimadaEntrega;
        DniQuienRecibe = dniQuienRecibe?.Trim();
        PesoTotal = pesoTotal;
        VolumenTotal = volumenTotal;
        Bultos = bultos;
        TipoEmbalaje = tipoEmbalaje?.Trim();
        SeguroTransporte = seguroTransporte;
        ValorDeclarado = valorDeclarado;
        SetUpdated(userId);
    }

    public void ConfigurarRemito(
        long? depositoOrigenId,
        bool esValorizado,
        EstadoLogisticoRemito? estadoLogistico,
        long? userId)
    {
        DepositoOrigenId = depositoOrigenId;
        EsValorizado = esValorizado;
        EstadoLogistico = estadoLogistico;
        SetUpdated(userId);
    }
    
    public void ConfigurarPedido(
        DateOnly? fechaEntregaCompromiso,
        PrioridadPedido? prioridad,
        bool stockReservado,
        long? userId)
    {
        FechaEntregaCompromiso = fechaEntregaCompromiso;
        Prioridad = prioridad;
        StockReservado = stockReservado;
        SetUpdated(userId);
    }
    
    public void AprobarPedido(long usuarioAprobadorId, long? userId)
    {
        if (EstadoPedido != Enums.EstadoPedido.Pendiente)
            throw new InvalidOperationException($"Solo se puede aprobar un pedido en estado Pendiente. Estado actual: {EstadoPedido}");
            
        EstadoPedido = Enums.EstadoPedido.Aprobado;
        UsuarioAprobadorId = usuarioAprobadorId;
        FechaAprobacion = DateTimeOffset.UtcNow;
        MotivoRechazo = null;
        SetUpdated(userId);
    }
    
    public void RechazarPedido(string motivoRechazo, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(motivoRechazo);
        
        if (EstadoPedido != Enums.EstadoPedido.Pendiente)
            throw new InvalidOperationException($"Solo se puede rechazar un pedido en estado Pendiente. Estado actual: {EstadoPedido}");
            
        EstadoPedido = Enums.EstadoPedido.Rechazado;
        MotivoRechazo = motivoRechazo.Trim();
        UsuarioAprobadorId = userId;
        FechaAprobacion = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }
    
    public void CerrarPedido(string? motivoCierre, long? userId)
    {
        if (EstadoPedido == Enums.EstadoPedido.Cerrado)
            throw new InvalidOperationException("El pedido ya está cerrado.");
            
        if (EstadoPedido == Enums.EstadoPedido.Anulado)
            throw new InvalidOperationException("No se puede cerrar un pedido anulado.");
            
        EstadoPedido = Enums.EstadoPedido.Cerrado;
        MotivoCierrePedido = motivoCierre?.Trim();
        FechaCierrePedido = DateTimeOffset.UtcNow;
        
        // Liberar stock reservado
        if (StockReservado)
        {
            StockReservado = false;
            // TODO: Aquí se debería agregar evento para liberar stock
        }
        
        SetUpdated(userId);
    }
    
    public void SetSnapshotsTercero(
        string? razonSocial,
        string? condicionIva,
        string? domicilio,
        long? userId)
    {
        TerceroRazonSocialSnapshot = razonSocial?.Trim();
        TerceroCondicionIvaSnapshot = condicionIva?.Trim();
        TerceroDomicilioSnapshot = domicilio?.Trim();
        SetUpdated(userId);
    }

    public void SetRecargo(decimal? recargoPorcentaje, decimal? recargoImporte, long? userId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se puede modificar el recargo en comprobantes borrador.");
        
        RecargoPorcentaje = recargoPorcentaje;
        RecargoImporte = recargoImporte;
        RecalcularTotales();
        SetUpdated(userId);
    }

    public void SetDescuentoGlobal(decimal? descuentoPorcentaje, long? userId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se puede modificar el descuento en comprobantes borrador.");
        
        DescuentoPorcentaje = descuentoPorcentaje;
        RecalcularTotales();
        SetUpdated(userId);
    }

    public void SetTerceroDomicilioSnapshot(string domicilio, long? userId)
    {
        TerceroDomicilioSnapshot = domicilio?.Trim();
        SetUpdated(userId);
    }

    // Métodos de gestión de pedidos

    public void InicializarComoPedido(DateOnly? fechaEntregaCompromiso, long? userId)
    {
        FechaEntregaCompromiso = fechaEntregaCompromiso;
        EstadoPedido = Domain.Enums.EstadoPedido.Pendiente;
        
        foreach (var item in _items)
        {
            item.ActualizarEstadoAtraso(fechaEntregaCompromiso);
        }
        
        SetUpdated(userId);
    }

    public void ActualizarCumplimientoPedido(long? userId)
    {
        if (!EstadoPedido.HasValue)
            throw new InvalidOperationException("Este comprobante no es un pedido.");

        if (EstadoPedido == Domain.Enums.EstadoPedido.Anulado)
            return;

        if (EstadoPedido == Domain.Enums.EstadoPedido.Cerrado)
            return;

        var todosCompletados = _items.All(i => i.EstadoEntrega == Domain.Enums.EstadoEntregaItem.EntregaCompleta || 
                                               i.EstadoEntrega == Domain.Enums.EstadoEntregaItem.EntregaSobrepasada);
        var algunoParcial = _items.Any(i => i.EstadoEntrega == Domain.Enums.EstadoEntregaItem.EntregaParcial);
        var todosNoEntregados = _items.All(i => i.EstadoEntrega == Domain.Enums.EstadoEntregaItem.NoEntregado || 
                                                !i.EstadoEntrega.HasValue);

        if (todosCompletados)
        {
            EstadoPedido = Domain.Enums.EstadoPedido.Completado;
        }
        else if (algunoParcial || !todosNoEntregados)
        {
            EstadoPedido = Domain.Enums.EstadoPedido.EnProceso;
        }
        else
        {
            EstadoPedido = Domain.Enums.EstadoPedido.Pendiente;
        }

        foreach (var item in _items)
        {
            item.ActualizarEstadoAtraso(FechaEntregaCompromiso);
        }

        SetUpdated(userId);
    }

    public void RegistrarEntregaParcialPedido(long itemId, decimal cantidadEntregada, long? userId)
    {
        if (!EstadoPedido.HasValue)
            throw new InvalidOperationException("Este comprobante no es un pedido.");

        var item = _items.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"No se encontró el item con ID {itemId}.");

        item.RegistrarEntrega(cantidadEntregada, FechaEntregaCompromiso);
        ActualizarCumplimientoPedido(userId);
    }

    // Métodos de gestión de devoluciones

    public void ConfigurarComoDevolucion(
        MotivoDevolucion motivo,
        TipoDevolucion tipo,
        string? observacionDevolucion,
        long? autorizadorId,
        bool reingresaStock,
        bool acreditaCuentaCorriente,
        long? userId)
    {
        MotivoDevolucion = motivo;
        TipoDevolucion = tipo;
        ObservacionDevolucion = observacionDevolucion?.Trim();
        AutorizadorDevolucionId = autorizadorId;
        FechaAutorizacionDevolucion = autorizadorId.HasValue ? DateTimeOffset.UtcNow : null;
        ReingresaStock = reingresaStock;
        AcreditaCuentaCorriente = acreditaCuentaCorriente;
        SetUpdated(userId);
    }

    public void AutorizarDevolucion(long autorizadorId, long? userId)
    {
        if (!MotivoDevolucion.HasValue)
            throw new InvalidOperationException("Este comprobante no está configurado como devolución.");

        if (AutorizadorDevolucionId.HasValue)
            throw new InvalidOperationException("La devolución ya ha sido autorizada.");

        AutorizadorDevolucionId = autorizadorId;
        FechaAutorizacionDevolucion = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }
}
