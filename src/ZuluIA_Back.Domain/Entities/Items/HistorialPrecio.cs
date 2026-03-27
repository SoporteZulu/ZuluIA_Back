namespace ZuluIA_Back.Domain.Entities.Items;

using ZuluIA_Back.Domain.Common;

/// <summary>
/// Registro histórico de un cambio de precio de un ítem.
/// Mapea a la tabla historial_precios.
/// </summary>
public class HistorialPrecio : BaseEntity
{
    public long ItemId { get; private set; }
    public decimal PrecioCostoAnterior { get; private set; }
    public decimal PrecioVentaAnterior { get; private set; }
    public decimal PrecioCostoNuevo    { get; private set; }
    public decimal PrecioVentaNuevo    { get; private set; }
    public DateTimeOffset FechaCambio  { get; private set; }
    public long? UsuarioId { get; private set; }
    public string? Motivo  { get; private set; }

    private HistorialPrecio() { }

    public static HistorialPrecio Crear(
        long itemId,
        decimal costAnterior,
        decimal ventaAnterior,
        decimal costNuevo,
        decimal ventaNuevo,
        long? usuarioId,
        string? motivo = null)
    {
        return new HistorialPrecio
        {
            ItemId              = itemId,
            PrecioCostoAnterior = costAnterior,
            PrecioVentaAnterior = ventaAnterior,
            PrecioCostoNuevo    = costNuevo,
            PrecioVentaNuevo    = ventaNuevo,
            FechaCambio         = DateTimeOffset.UtcNow,
            UsuarioId           = usuarioId,
            Motivo              = motivo?.Trim(),
        };
    }
}
