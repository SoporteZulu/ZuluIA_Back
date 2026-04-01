using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Precios;

/// <summary>
/// Precio especial para un canal de venta específico (ej: venta directa, distribuidores, e-commerce).
/// </summary>
public class PrecioEspecialCanal : AuditableEntity
{
    public long ItemId { get; private set; }
    public long CanalId { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Precio { get; private set; }
    public decimal DescuentoPct { get; private set; }
    public DateOnly? VigenciaDesde { get; private set; }
    public DateOnly? VigenciaHasta { get; private set; }
    public bool Activo { get; private set; } = true;
    public string? Observaciones { get; private set; }

    private PrecioEspecialCanal() { }

    public static PrecioEspecialCanal Crear(
        long itemId,
        long canalId,
        long monedaId,
        decimal precio,
        decimal descuentoPct,
        DateOnly? vigenciaDesde,
        DateOnly? vigenciaHasta,
        string? observaciones,
        long? userId)
    {
        if (precio < 0)
            throw new ArgumentException("El precio no puede ser negativo.", nameof(precio));

        if (descuentoPct < 0 || descuentoPct > 100)
            throw new ArgumentException("El descuento debe estar entre 0 y 100%.", nameof(descuentoPct));

        if (vigenciaDesde.HasValue && vigenciaHasta.HasValue && vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException(
                "La fecha de vigencia hasta no puede ser anterior a la fecha desde.");

        var precioEspecial = new PrecioEspecialCanal
        {
            ItemId         = itemId,
            CanalId        = canalId,
            MonedaId       = monedaId,
            Precio         = precio,
            DescuentoPct   = descuentoPct,
            VigenciaDesde  = vigenciaDesde,
            VigenciaHasta  = vigenciaHasta,
            Observaciones  = observaciones?.Trim(),
            Activo         = true
        };

        precioEspecial.SetCreated(userId);
        return precioEspecial;
    }

    public void Actualizar(
        decimal precio,
        decimal descuentoPct,
        DateOnly? vigenciaDesde,
        DateOnly? vigenciaHasta,
        string? observaciones,
        long? userId)
    {
        if (precio < 0)
            throw new ArgumentException("El precio no puede ser negativo.", nameof(precio));

        if (descuentoPct < 0 || descuentoPct > 100)
            throw new ArgumentException("El descuento debe estar entre 0 y 100%.", nameof(descuentoPct));

        if (vigenciaDesde.HasValue && vigenciaHasta.HasValue && vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException(
                "La fecha de vigencia hasta no puede ser anterior a la fecha desde.");

        Precio        = precio;
        DescuentoPct  = descuentoPct;
        VigenciaDesde = vigenciaDesde;
        VigenciaHasta = vigenciaHasta;
        Observaciones = observaciones?.Trim();
        SetUpdated(userId);
    }

    public decimal PrecioFinal =>
        Math.Round(Precio * (1 - DescuentoPct / 100), 4);

    public bool EstaVigente(DateOnly fecha)
    {
        if (!Activo) return false;
        if (VigenciaDesde.HasValue && fecha < VigenciaDesde.Value) return false;
        if (VigenciaHasta.HasValue && fecha > VigenciaHasta.Value) return false;
        return true;
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
