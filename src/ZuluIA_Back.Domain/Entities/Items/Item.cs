using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Events.Items;

namespace ZuluIA_Back.Domain.Entities.Items;

public class Item : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string? CodigoBarras { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public string? DescripcionAdicional { get; private set; }
    public long? CategoriaId { get; private set; }
    public long UnidadMedidaId { get; private set; }
    public long AlicuotaIvaId { get; private set; }
    public long MonedaId { get; private set; }
    public bool EsProducto { get; private set; } = true;
    public bool EsServicio { get; private set; }
    public bool EsFinanciero { get; private set; }
    public bool ManejaStock { get; private set; } = true;
    public decimal PrecioCosto { get; private set; }
    public decimal PrecioVenta { get; private set; }
    public decimal StockMinimo { get; private set; }
    public decimal? StockMaximo { get; private set; }
    public string? CodigoAfip { get; private set; }
    public long? SucursalId { get; private set; }
    public bool Activo { get; private set; } = true;

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
        long? userId)
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

        var item = new Item
        {
            Codigo              = codigo.Trim().ToUpperInvariant(),
            Descripcion         = descripcion.Trim(),
            UnidadMedidaId      = unidadMedidaId,
            AlicuotaIvaId       = alicuotaIvaId,
            MonedaId            = monedaId,
            EsProducto          = esProducto,
            EsServicio          = esServicio,
            EsFinanciero        = esFinanciero,
            ManejaStock         = manejaStock && esProducto,
            PrecioCosto         = precioCosto,
            PrecioVenta         = precioVenta,
            CategoriaId         = categoriaId,
            StockMinimo         = stockMinimo,
            StockMaximo         = stockMaximo,
            CodigoBarras        = codigoBarras?.Trim(),
            DescripcionAdicional= descripcionAdicional?.Trim(),
            CodigoAfip          = codigoAfip?.Trim(),
            SucursalId          = sucursalId,
            Activo              = true
        };

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
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (esProducto && esServicio)
            throw new InvalidOperationException("Un ítem no puede ser producto y servicio a la vez.");
        if (stockMinimo < 0)
            throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
        if (stockMaximo.HasValue && stockMaximo < stockMinimo)
            throw new InvalidOperationException("El stock máximo no puede ser menor al stock mínimo.");

        Descripcion          = descripcion.Trim();
        DescripcionAdicional = descripcionAdicional?.Trim();
        CodigoBarras         = codigoBarras?.Trim();
        UnidadMedidaId       = unidadMedidaId;
        AlicuotaIvaId        = alicuotaIvaId;
        MonedaId             = monedaId;
        EsProducto           = esProducto;
        EsServicio           = esServicio;
        EsFinanciero         = esFinanciero;
        ManejaStock          = manejaStock && esProducto;
        CategoriaId          = categoriaId;
        CodigoAfip           = codigoAfip?.Trim();
        StockMinimo          = stockMinimo;
        StockMaximo          = stockMaximo;
        SucursalId           = sucursalId;
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

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activo = true;
        SetUpdated(userId);
    }
}
