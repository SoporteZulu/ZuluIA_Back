using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Fiscal;

public class LiquidacionPrimariaGrano : AuditableEntity
{
    public long SucursalId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string NumeroLiquidacion { get; private set; } = string.Empty;
    public string Producto { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Total { get; private set; }
    public string? Observacion { get; private set; }

    private LiquidacionPrimariaGrano() { }

    public static LiquidacionPrimariaGrano Crear(long sucursalId, DateOnly fecha, string numeroLiquidacion, string producto, decimal cantidad, decimal precioUnitario, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numeroLiquidacion);
        ArgumentException.ThrowIfNullOrWhiteSpace(producto);
        if (cantidad <= 0 || precioUnitario <= 0)
            throw new InvalidOperationException("La cantidad y el precio unitario deben ser mayores a 0.");

        var entidad = new LiquidacionPrimariaGrano
        {
            SucursalId = sucursalId,
            Fecha = fecha,
            NumeroLiquidacion = numeroLiquidacion.Trim().ToUpperInvariant(),
            Producto = producto.Trim(),
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            Total = decimal.Round(cantidad * precioUnitario, 2, MidpointRounding.AwayFromZero),
            Observacion = observacion?.Trim()
        };

        entidad.SetCreated(userId);
        return entidad;
    }
}
