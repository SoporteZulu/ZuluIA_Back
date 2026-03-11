using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class MovimientoCtaCte : BaseEntity
{
    public long TerceroId { get; private set; }
    public long? SucursalId { get; private set; }
    public long MonedaId { get; private set; }
    public long? ComprobanteId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Debe { get; private set; }
    public decimal Haber { get; private set; }
    public decimal Saldo { get; private set; }
    public string? Descripcion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private MovimientoCtaCte() { }

    public static MovimientoCtaCte Crear(
        long terceroId,
        long? sucursalId,
        long monedaId,
        long? comprobanteId,
        DateOnly fecha,
        decimal debe,
        decimal haber,
        decimal saldoResultante,
        string? descripcion)
    {
        return new MovimientoCtaCte
        {
            TerceroId     = terceroId,
            SucursalId    = sucursalId,
            MonedaId      = monedaId,
            ComprobanteId = comprobanteId,
            Fecha         = fecha,
            Debe          = debe,
            Haber         = haber,
            Saldo         = saldoResultante,
            Descripcion   = descripcion?.Trim(),
            CreatedAt     = DateTimeOffset.UtcNow
        };
    }
}