using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class CuentaCorriente : BaseEntity
{
    public long TerceroId { get; private set; }
    public long? SucursalId { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Saldo { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CuentaCorriente() { }

    public static CuentaCorriente Crear(
        long terceroId,
        long? sucursalId,
        long monedaId)
    {
        return new CuentaCorriente
        {
            TerceroId  = terceroId,
            SucursalId = sucursalId,
            MonedaId   = monedaId,
            Saldo      = 0,
            UpdatedAt  = DateTimeOffset.UtcNow
        };
    }

    public void Debitar(decimal importe)
    {
        if (importe <= 0)
            throw new InvalidOperationException(
                "El importe a debitar debe ser mayor a 0.");
        Saldo    += importe;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Acreditar(decimal importe)
    {
        if (importe <= 0)
            throw new InvalidOperationException(
                "El importe a acreditar debe ser mayor a 0.");
        Saldo    -= importe;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}