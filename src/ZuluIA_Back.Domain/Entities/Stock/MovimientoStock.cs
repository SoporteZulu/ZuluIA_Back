using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Stock;

public class MovimientoStock : BaseEntity
{
    public long ItemId { get; private set; }
    public long DepositoId { get; private set; }
    public DateTimeOffset Fecha { get; private set; }
    public TipoMovimientoStock TipoMovimiento { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal SaldoResultante { get; private set; }
    public string? OrigenTabla { get; private set; }
    public long? OrigenId { get; private set; }
    public string? Observacion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }

    private MovimientoStock() { }

    public static MovimientoStock Registrar(
        long itemId,
        long depositoId,
        TipoMovimientoStock tipo,
        decimal cantidad,
        decimal saldoResultante,
        string? origenTabla = null,
        long? origenId = null,
        string? observacion = null,
        long? userId = null)
    {
        if (cantidad == 0)
            throw new InvalidOperationException("La cantidad del movimiento no puede ser 0.");

        return new MovimientoStock
        {
            ItemId          = itemId,
            DepositoId      = depositoId,
            Fecha           = DateTimeOffset.UtcNow,
            TipoMovimiento  = tipo,
            Cantidad        = cantidad,
            SaldoResultante = saldoResultante,
            OrigenTabla     = origenTabla,
            OrigenId        = origenId,
            Observacion     = observacion,
            CreatedAt       = DateTimeOffset.UtcNow,
            CreatedBy       = userId
        };
    }
}