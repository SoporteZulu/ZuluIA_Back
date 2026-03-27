using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Logistica;

public class TransferenciaDepositoDetalle : BaseEntity
{
    public long TransferenciaDepositoId { get; private set; }
    public long ItemId { get; private set; }
    public decimal Cantidad { get; private set; }
    public string? Observacion { get; private set; }

    private TransferenciaDepositoDetalle() { }

    internal static TransferenciaDepositoDetalle Crear(long transferenciaDepositoId, long itemId, decimal cantidad, string? observacion = null)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad a transferir debe ser mayor a 0.");

        return new TransferenciaDepositoDetalle
        {
            TransferenciaDepositoId = transferenciaDepositoId,
            ItemId = itemId,
            Cantidad = cantidad,
            Observacion = observacion?.Trim()
        };
    }
}
