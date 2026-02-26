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
        bool manejaStock,
        decimal precioCosto,
        decimal precioVenta,
        long? sucursalId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (esProducto && esServicio)
            throw new InvalidOperationException("Un ítem no puede ser producto y servicio a la vez.");

        var item = new Item
        {
            Codigo         = codigo.Trim().ToUpperInvariant(),
            Descripcion    = descripcion.Trim(),
            UnidadMedidaId = unidadMedidaId,
            AlicuotaIvaId  = alicuotaIvaId,
            MonedaId       = monedaId,
            EsProducto     = esProducto,
            EsServicio     = esServicio,
            ManejaStock    = manejaStock && esProducto,
            PrecioCosto    = precioCosto,
            PrecioVenta    = precioVenta,
            SucursalId     = sucursalId,
            Activo         = true
        };

        item.SetCreated(userId);
        item.AddDomainEvent(new ItemCreadoEvent(item.Codigo, item.Descripcion));

        return item;
    }

    public void ActualizarPrecios(decimal precioCosto, decimal precioVenta, long? userId)
    {
        var precioAnterior = PrecioVenta;
        PrecioCosto = precioCosto;
        PrecioVenta = precioVenta;
        SetUpdated(userId);
        AddDomainEvent(new PrecioItemActualizadoEvent(Id, Codigo, precioAnterior, precioVenta));
    }

    public void Actualizar(
        string descripcion,
        string? descripcionAdicional,
        string? codigoBarras,
        long? categoriaId,
        decimal stockMinimo,
        decimal? stockMaximo,
        string? codigoAfip,
        long? userId)
    {
        Descripcion          = descripcion.Trim();
        DescripcionAdicional = descripcionAdicional?.Trim();
        CodigoBarras         = codigoBarras?.Trim();
        CategoriaId          = categoriaId;
        StockMinimo          = stockMinimo;
        StockMaximo          = stockMaximo;
        CodigoAfip           = codigoAfip?.Trim();
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