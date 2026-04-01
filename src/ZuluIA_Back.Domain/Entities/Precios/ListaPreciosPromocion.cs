using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Precios;

/// <summary>
/// Promoción aplicable a una lista de precios.
/// Permite definir descuentos especiales con vigencia temporal.
/// </summary>
public class ListaPreciosPromocion : AuditableEntity
{
    public long ListaId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal DescuentoPct { get; private set; }
    public DateOnly VigenciaDesde { get; private set; }
    public DateOnly VigenciaHasta { get; private set; }
    public bool Activa { get; private set; } = true;
    public long? ItemId { get; private set; }
    public long? CategoriaId { get; private set; }
    public decimal? MontoMinimoCompra { get; private set; }
    public int? CantidadMinima { get; private set; }
    public string? Observaciones { get; private set; }

    public ListaPrecios? Lista { get; private set; }

    private ListaPreciosPromocion() { }

    public static ListaPreciosPromocion Crear(
        long listaId,
        string descripcion,
        decimal descuentoPct,
        DateOnly vigenciaDesde,
        DateOnly vigenciaHasta,
        long? itemId,
        long? categoriaId,
        decimal? montoMinimoCompra,
        int? cantidadMinima,
        string? observaciones,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (descuentoPct < 0 || descuentoPct > 100)
            throw new ArgumentException("El descuento debe estar entre 0 y 100%.", nameof(descuentoPct));

        if (vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException(
                "La fecha de vigencia hasta no puede ser anterior a la fecha desde.");

        if (montoMinimoCompra.HasValue && montoMinimoCompra.Value < 0)
            throw new ArgumentException("El monto mínimo no puede ser negativo.", nameof(montoMinimoCompra));

        if (cantidadMinima.HasValue && cantidadMinima.Value < 0)
            throw new ArgumentException("La cantidad mínima no puede ser negativa.", nameof(cantidadMinima));

        var promocion = new ListaPreciosPromocion
        {
            ListaId           = listaId,
            Descripcion       = descripcion.Trim(),
            DescuentoPct      = descuentoPct,
            VigenciaDesde     = vigenciaDesde,
            VigenciaHasta     = vigenciaHasta,
            ItemId            = itemId,
            CategoriaId       = categoriaId,
            MontoMinimoCompra = montoMinimoCompra,
            CantidadMinima    = cantidadMinima,
            Observaciones     = observaciones?.Trim(),
            Activa            = true
        };

        promocion.SetCreated(userId);
        return promocion;
    }

    public void Actualizar(
        string descripcion,
        decimal descuentoPct,
        DateOnly vigenciaDesde,
        DateOnly vigenciaHasta,
        long? itemId,
        long? categoriaId,
        decimal? montoMinimoCompra,
        int? cantidadMinima,
        string? observaciones,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (descuentoPct < 0 || descuentoPct > 100)
            throw new ArgumentException("El descuento debe estar entre 0 y 100%.", nameof(descuentoPct));

        if (vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException(
                "La fecha de vigencia hasta no puede ser anterior a la fecha desde.");

        if (montoMinimoCompra.HasValue && montoMinimoCompra.Value < 0)
            throw new ArgumentException("El monto mínimo no puede ser negativo.", nameof(montoMinimoCompra));

        if (cantidadMinima.HasValue && cantidadMinima.Value < 0)
            throw new ArgumentException("La cantidad mínima no puede ser negativa.", nameof(cantidadMinima));

        Descripcion       = descripcion.Trim();
        DescuentoPct      = descuentoPct;
        VigenciaDesde     = vigenciaDesde;
        VigenciaHasta     = vigenciaHasta;
        ItemId            = itemId;
        CategoriaId       = categoriaId;
        MontoMinimoCompra = montoMinimoCompra;
        CantidadMinima    = cantidadMinima;
        Observaciones     = observaciones?.Trim();
        SetUpdated(userId);
    }

    public bool EstaVigente(DateOnly fecha) =>
        Activa && fecha >= VigenciaDesde && fecha <= VigenciaHasta;

    public bool AplicaA(long itemId, long? categoriaId = null, decimal? montoCompra = null, int? cantidad = null)
    {
        if (!Activa) return false;

        if (ItemId.HasValue && ItemId.Value != itemId)
            return false;

        if (CategoriaId.HasValue && categoriaId.HasValue && CategoriaId.Value != categoriaId.Value)
            return false;

        if (MontoMinimoCompra.HasValue && (!montoCompra.HasValue || montoCompra.Value < MontoMinimoCompra.Value))
            return false;

        if (CantidadMinima.HasValue && (!cantidad.HasValue || cantidad.Value < CantidadMinima.Value))
            return false;

        return true;
    }

    public void Desactivar(long? userId)
    {
        Activa = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activa = true;
        SetUpdated(userId);
    }
}
