using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Events.Items;

namespace ZuluIA_Back.Domain.Entities.Items;

public class Item : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string? CodigoAlternativo { get; private set; }
    public string? CodigoBarras { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public string? DescripcionAdicional { get; private set; }
    public long? CategoriaId { get; private set; }
    public long? MarcaId { get; private set; }
    public long UnidadMedidaId { get; private set; }
    public long AlicuotaIvaId { get; private set; }
    public long? AlicuotaIvaCompraId { get; private set; }
    public long? ImpuestoInternoId { get; private set; }
    public long MonedaId { get; private set; }
    public bool EsProducto { get; private set; } = true;
    public bool EsServicio { get; private set; }
    public bool EsFinanciero { get; private set; }
    public bool ManejaStock { get; private set; } = true;
    public decimal PrecioCosto { get; private set; }
    public decimal PrecioVenta { get; private set; }
    public decimal StockMinimo { get; private set; }
    public decimal? StockMaximo { get; private set; }
    public decimal? PuntoReposicion { get; private set; }
    public decimal? StockSeguridad { get; private set; }
    public decimal? Peso { get; private set; }
    public decimal? Volumen { get; private set; }
    public bool EsTrazable { get; private set; }
    public bool PermiteFraccionamiento { get; private set; }
    public int? DiasVencimientoLimite { get; private set; }
    public long? DepositoDefaultId { get; private set; }
    public string? CodigoAfip { get; private set; }
    public long? SucursalId { get; private set; }
    public bool Activo { get; private set; } = true;

    // ── Fase 1: Campos Esenciales de Ventas ──────────────────────────────────
    public bool AplicaVentas { get; private set; } = true;
    public bool AplicaCompras { get; private set; } = true;
    public decimal? PorcentajeGanancia { get; private set; }
    public decimal? PorcentajeMaximoDescuento { get; private set; }
    public bool EsRpt { get; private set; }
    public bool EsSistema { get; private set; }

    private Item() { }

    public static Item Crear(
        string codigo,
        string descripcion,
        long unidadMedidaId,
        long alicuotaIvaId,
        long monedaId,
        bool esProducto,
        bool esServicio,
        bool esFinanciero,
        bool manejaStock,
        decimal precioCosto,
        decimal precioVenta,
        long? categoriaId,
        decimal stockMinimo,
        decimal? stockMaximo,
        string? codigoBarras,
        string? descripcionAdicional,
        string? codigoAfip,
        long? sucursalId,
        long? userId,
        decimal? puntoReposicion = null,
        decimal? stockSeguridad = null,
        decimal? peso = null,
        decimal? volumen = null,
        string? codigoAlternativo = null,
        bool esTrazable = false,
        bool permiteFraccionamiento = false,
        int? diasVencimientoLimite = null,
        long? depositoDefaultId = null,
        long? alicuotaIvaCompraId = null,
        long? impuestoInternoId = null)
        => Crear(
            codigo,
            descripcion,
            unidadMedidaId,
            alicuotaIvaId,
            monedaId,
            esProducto,
            esServicio,
            esFinanciero,
            manejaStock,
            precioCosto,
            precioVenta,
            categoriaId,
            null,
            stockMinimo,
            stockMaximo,
            codigoBarras,
            descripcionAdicional,
            codigoAfip,
            sucursalId,
            null, null, null, null, false, false,
            userId,
            puntoReposicion,
            stockSeguridad,
            peso,
            volumen,
            codigoAlternativo,
            esTrazable,
            permiteFraccionamiento,
            diasVencimientoLimite,
            depositoDefaultId,
            alicuotaIvaCompraId,
            impuestoInternoId);

    public static Item Crear(
        string codigo,
        string descripcion,
        long unidadMedidaId,
        long alicuotaIvaId,
        long monedaId,
        bool esProducto,
        bool esServicio,
        bool esFinanciero,
        bool manejaStock,
        decimal precioCosto,
        decimal precioVenta,
        long? categoriaId,
        long? marcaId,
        decimal stockMinimo,
        decimal? stockMaximo,
        string? codigoBarras,
        string? descripcionAdicional,
        string? codigoAfip,
        long? sucursalId,
        bool? aplicaVentas,
        bool? aplicaCompras,
        decimal? porcentajeGanancia,
        decimal? porcentajeMaximoDescuento,
        bool esRpt,
        bool esSistema,
        long? userId,
        decimal? puntoReposicion = null,
        decimal? stockSeguridad = null,
        decimal? peso = null,
        decimal? volumen = null,
        string? codigoAlternativo = null,
        bool esTrazable = false,
        bool permiteFraccionamiento = false,
        int? diasVencimientoLimite = null,
        long? depositoDefaultId = null,
        long? alicuotaIvaCompraId = null,
        long? impuestoInternoId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (esProducto && esServicio)
            throw new InvalidOperationException("Un ítem no puede ser producto y servicio a la vez.");

        if (precioCosto < 0)
            throw new InvalidOperationException("El precio de costo no puede ser negativo.");
        if (precioVenta < 0)
            throw new InvalidOperationException("El precio de venta no puede ser negativo.");
        if (stockMinimo < 0)
            throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
        if (stockMaximo.HasValue && stockMaximo < stockMinimo)
            throw new InvalidOperationException("El stock máximo no puede ser menor al stock mínimo.");
        if (puntoReposicion.HasValue && puntoReposicion < 0)
            throw new InvalidOperationException("El punto de reposición no puede ser negativo.");
        if (stockSeguridad.HasValue && stockSeguridad < 0)
            throw new InvalidOperationException("El stock de seguridad no puede ser negativo.");
        if (peso.HasValue && peso < 0)
            throw new InvalidOperationException("El peso no puede ser negativo.");
        if (volumen.HasValue && volumen < 0)
            throw new InvalidOperationException("El volumen no puede ser negativo.");
        if (diasVencimientoLimite.HasValue && diasVencimientoLimite < 0)
            throw new InvalidOperationException("Los días límite de vencimiento no pueden ser negativos.");
        if (alicuotaIvaCompraId.HasValue && alicuotaIvaCompraId <= 0)
            throw new InvalidOperationException("La alícuota IVA compra es inválida.");
        if (impuestoInternoId.HasValue && impuestoInternoId <= 0)
            throw new InvalidOperationException("El impuesto interno es inválido.");
        if (esTrazable && !(manejaStock && esProducto) && !esFinanciero)
            throw new InvalidOperationException("Solo un ítem de stock puede ser trazable.");
        if (permiteFraccionamiento && !esProducto)
            throw new InvalidOperationException("Solo un producto puede permitir fraccionamiento.");
        if (depositoDefaultId.HasValue && !(manejaStock && esProducto) && !esFinanciero)
            throw new InvalidOperationException("Solo un ítem que maneja stock puede tener depósito por defecto.");
        if (porcentajeGanancia.HasValue && porcentajeGanancia < 0)
            throw new InvalidOperationException("El porcentaje de ganancia no puede ser negativo.");
        if (porcentajeMaximoDescuento.HasValue && (porcentajeMaximoDescuento < 0 || porcentajeMaximoDescuento > 100))
            throw new InvalidOperationException("El porcentaje máximo de descuento debe estar entre 0 y 100.");

        var item = new Item
        {
            Codigo              = codigo.Trim().ToUpperInvariant(),
            Descripcion         = descripcion.Trim(),
            UnidadMedidaId      = unidadMedidaId,
            AlicuotaIvaId       = alicuotaIvaId,
            AlicuotaIvaCompraId = alicuotaIvaCompraId,
            ImpuestoInternoId   = impuestoInternoId,
            MonedaId            = monedaId,
            EsProducto          = esProducto,
            EsServicio          = esServicio,
            EsFinanciero        = esFinanciero,
            ManejaStock         = manejaStock && esProducto,
            PrecioCosto         = precioCosto,
            PrecioVenta         = precioVenta,
            CategoriaId         = categoriaId,
            MarcaId             = marcaId,
            StockMinimo         = stockMinimo,
            StockMaximo         = stockMaximo,
            PuntoReposicion     = puntoReposicion,
            StockSeguridad      = stockSeguridad,
            Peso                = peso,
            Volumen             = volumen,
            EsTrazable          = esFinanciero ? false : esTrazable,
            PermiteFraccionamiento = esFinanciero ? false : permiteFraccionamiento,
            DiasVencimientoLimite = esFinanciero ? null : diasVencimientoLimite,
            DepositoDefaultId    = esFinanciero ? null : depositoDefaultId,
            CodigoAlternativo    = string.IsNullOrWhiteSpace(codigoAlternativo) ? null : codigoAlternativo.Trim().ToUpperInvariant(),
            CodigoBarras        = codigoBarras?.Trim(),
            DescripcionAdicional= descripcionAdicional?.Trim(),
            CodigoAfip          = codigoAfip?.Trim(),
            SucursalId          = sucursalId,
            AplicaVentas        = aplicaVentas ?? !esFinanciero,
            AplicaCompras       = aplicaCompras ?? !esFinanciero,
            PorcentajeGanancia  = porcentajeGanancia,
            PorcentajeMaximoDescuento = porcentajeMaximoDescuento,
            EsRpt               = esRpt,
            EsSistema           = esSistema,
            Activo              = true
        };

        if (item.PorcentajeGanancia.HasValue && item.PrecioCosto > 0)
            item.PrecioVenta = item.CalcularPrecioVentaPorGanancia();

        item.SetCreated(userId);
        item.AddDomainEvent(new ItemCreadoEvent(item.Codigo, item.Descripcion));

        return item;
    }

    public void Actualizar(
        string descripcion,
        string? descripcionAdicional,
        string? codigoBarras,
        long unidadMedidaId,
        long alicuotaIvaId,
        long monedaId,
        bool esProducto,
        bool esServicio,
        bool esFinanciero,
        bool manejaStock,
        long? categoriaId,
        string? codigoAfip,
        decimal stockMinimo,
        decimal? stockMaximo,
        long? sucursalId,
        long? userId,
        decimal? puntoReposicion = null,
        decimal? stockSeguridad = null,
        decimal? peso = null,
        decimal? volumen = null,
        string? codigoAlternativo = null,
        bool esTrazable = false,
        bool permiteFraccionamiento = false,
        int? diasVencimientoLimite = null,
        long? depositoDefaultId = null,
        long? alicuotaIvaCompraId = null,
        long? impuestoInternoId = null)
        => Actualizar(
            descripcion,
            descripcionAdicional,
            codigoBarras,
            unidadMedidaId,
            alicuotaIvaId,
            monedaId,
            esProducto,
            esServicio,
            esFinanciero,
            manejaStock,
            categoriaId,
            null,
            codigoAfip,
            stockMinimo,
            stockMaximo,
            sucursalId,
            userId,
            puntoReposicion,
            stockSeguridad,
            peso,
            volumen,
            codigoAlternativo,
            esTrazable,
            permiteFraccionamiento,
            diasVencimientoLimite,
            depositoDefaultId,
            alicuotaIvaCompraId,
            impuestoInternoId);

    public void Actualizar(
        string descripcion,
        string? descripcionAdicional,
        string? codigoBarras,
        long unidadMedidaId,
        long alicuotaIvaId,
        long monedaId,
        bool esProducto,
        bool esServicio,
        bool esFinanciero,
        bool manejaStock,
        long? categoriaId,
        long? marcaId,
        string? codigoAfip,
        decimal stockMinimo,
        decimal? stockMaximo,
        long? sucursalId,
        long? userId,
        decimal? puntoReposicion = null,
        decimal? stockSeguridad = null,
        decimal? peso = null,
        decimal? volumen = null,
        string? codigoAlternativo = null,
        bool esTrazable = false,
        bool permiteFraccionamiento = false,
        int? diasVencimientoLimite = null,
        long? depositoDefaultId = null,
        long? alicuotaIvaCompraId = null,
        long? impuestoInternoId = null)
    {
        ValidarEdicionPermitida();
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (esProducto && esServicio)
            throw new InvalidOperationException("Un ítem no puede ser producto y servicio a la vez.");
        if (stockMinimo < 0)
            throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
        if (stockMaximo.HasValue && stockMaximo < stockMinimo)
            throw new InvalidOperationException("El stock máximo no puede ser menor al stock mínimo.");
        if (puntoReposicion.HasValue && puntoReposicion < 0)
            throw new InvalidOperationException("El punto de reposición no puede ser negativo.");
        if (stockSeguridad.HasValue && stockSeguridad < 0)
            throw new InvalidOperationException("El stock de seguridad no puede ser negativo.");
        if (peso.HasValue && peso < 0)
            throw new InvalidOperationException("El peso no puede ser negativo.");
        if (volumen.HasValue && volumen < 0)
            throw new InvalidOperationException("El volumen no puede ser negativo.");
        if (diasVencimientoLimite.HasValue && diasVencimientoLimite < 0)
            throw new InvalidOperationException("Los días límite de vencimiento no pueden ser negativos.");
        if (alicuotaIvaCompraId.HasValue && alicuotaIvaCompraId <= 0)
            throw new InvalidOperationException("La alícuota IVA compra es inválida.");
        if (impuestoInternoId.HasValue && impuestoInternoId <= 0)
            throw new InvalidOperationException("El impuesto interno es inválido.");
        if (esTrazable && !(manejaStock && esProducto) && !esFinanciero)
            throw new InvalidOperationException("Solo un ítem de stock puede ser trazable.");
        if (permiteFraccionamiento && !esProducto)
            throw new InvalidOperationException("Solo un producto puede permitir fraccionamiento.");
        if (depositoDefaultId.HasValue && !(manejaStock && esProducto) && !esFinanciero)
            throw new InvalidOperationException("Solo un ítem que maneja stock puede tener depósito por defecto.");

        Descripcion          = descripcion.Trim();
        DescripcionAdicional = descripcionAdicional?.Trim();
        CodigoAlternativo    = string.IsNullOrWhiteSpace(codigoAlternativo) ? null : codigoAlternativo.Trim().ToUpperInvariant();
        CodigoBarras         = codigoBarras?.Trim();
        UnidadMedidaId       = unidadMedidaId;
        AlicuotaIvaId        = alicuotaIvaId;
        AlicuotaIvaCompraId  = alicuotaIvaCompraId;
        ImpuestoInternoId    = impuestoInternoId;
        MonedaId             = monedaId;
        EsProducto           = esProducto;
        EsServicio           = esServicio;
        EsFinanciero         = esFinanciero;
        ManejaStock          = manejaStock && esProducto;
        CategoriaId          = categoriaId;
        MarcaId              = marcaId;
        CodigoAfip           = codigoAfip?.Trim();
        StockMinimo          = stockMinimo;
        StockMaximo          = stockMaximo;
        PuntoReposicion      = puntoReposicion;
        StockSeguridad       = stockSeguridad;
        Peso                 = peso;
        Volumen              = volumen;
        EsTrazable           = esFinanciero ? false : esTrazable;
        PermiteFraccionamiento = esFinanciero ? false : permiteFraccionamiento;
        DiasVencimientoLimite = esFinanciero ? null : diasVencimientoLimite;
        DepositoDefaultId    = esFinanciero ? null : depositoDefaultId;
        SucursalId           = sucursalId;
        
        // Si cambia a financiero, desactivar ventas/compras y stock
        if (esFinanciero)
        {
            AplicaVentas = false;
            AplicaCompras = false;
            ManejaStock = false;
            EsTrazable = false;
            PermiteFraccionamiento = false;
            DiasVencimientoLimite = null;
            DepositoDefaultId = null;
        }
        
        SetUpdated(userId);
    }

    public void ActualizarPrecios(decimal precioCosto, decimal precioVenta, long? userId)
    {
        if (precioCosto < 0)
            throw new InvalidOperationException("El precio de costo no puede ser negativo.");
        if (precioVenta < 0)
            throw new InvalidOperationException("El precio de venta no puede ser negativo.");

        var precioAnterior = PrecioVenta;
        PrecioCosto = precioCosto;
        PrecioVenta = precioVenta;
        SetUpdated(userId);
        AddDomainEvent(new PrecioItemActualizadoEvent(Id, Codigo, precioAnterior, precioVenta));
    }

    public void ActualizarStock(decimal stockMinimo, decimal? stockMaximo, long? userId)
    {
        if (stockMinimo < 0)
            throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
        if (stockMaximo.HasValue && stockMaximo < stockMinimo)
            throw new InvalidOperationException("El stock máximo no puede ser menor al stock mínimo.");

        StockMinimo = stockMinimo;
        StockMaximo = stockMaximo;
        SetUpdated(userId);
    }

    // ── Métodos Fase 1: Gestión de Ventas ────────────────────────────────────

    public void ActualizarConfiguracionVentas(
        bool aplicaVentas,
        bool aplicaCompras,
        decimal? porcentajeMaximoDescuento,
        bool esRpt,
        long? userId)
    {
        if (porcentajeMaximoDescuento.HasValue && 
            (porcentajeMaximoDescuento < 0 || porcentajeMaximoDescuento > 100))
            throw new InvalidOperationException(
                "El porcentaje máximo de descuento debe estar entre 0 y 100.");

        AplicaVentas = aplicaVentas;
        AplicaCompras = aplicaCompras;
        PorcentajeMaximoDescuento = porcentajeMaximoDescuento;
        EsRpt = esRpt;
        SetUpdated(userId);
    }

    public void ActualizarPorcentajeGanancia(decimal? porcentajeGanancia, long? userId)
    {
        if (porcentajeGanancia.HasValue && porcentajeGanancia < 0)
            throw new InvalidOperationException(
                "El porcentaje de ganancia no puede ser negativo.");

        PorcentajeGanancia = porcentajeGanancia;
        
        // Recalcular precio de venta si hay porcentaje de ganancia
        if (porcentajeGanancia.HasValue && PrecioCosto > 0)
        {
            var nuevoPrecioVenta = PrecioCosto * (1 + (porcentajeGanancia.Value / 100));
            var precioAnterior = PrecioVenta;
            PrecioVenta = Math.Round(nuevoPrecioVenta, 2);
            SetUpdated(userId);
            AddDomainEvent(new PrecioItemActualizadoEvent(Id, Codigo, precioAnterior, PrecioVenta));
        }
        else
        {
            SetUpdated(userId);
        }
    }

    public decimal CalcularPrecioVentaPorGanancia()
    {
        if (!PorcentajeGanancia.HasValue || PrecioCosto <= 0)
            return PrecioVenta;
        
        return Math.Round(PrecioCosto * (1 + (PorcentajeGanancia.Value / 100)), 2);
    }

    public bool ValidarDescuento(decimal porcentajeDescuento)
    {
        if (!PorcentajeMaximoDescuento.HasValue)
            return true; // Sin límite configurado

        return porcentajeDescuento <= PorcentajeMaximoDescuento.Value;
    }

    public void MarcarComoItemSistema()
    {
        if (!EsSistema)
        {
            EsSistema = true;
            SetUpdated(null); // Sistema, sin usuario
        }
    }

    public void ValidarEdicionPermitida()
    {
        if (EsSistema)
            throw new InvalidOperationException(
                "No se puede modificar un item del sistema.");
    }

    public void Desactivar(long? userId)
    {
        ValidarEdicionPermitida();
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    /// <summary>
    /// Reactiva un ítem desactivado y limpia la marca de borrado lógico.
    /// </summary>
    public void Activar(long? userId)
    {
        ValidarEdicionPermitida();
        Activo = true;
        SetDeletedAt(null);
        SetUpdated(userId);
    }
}
