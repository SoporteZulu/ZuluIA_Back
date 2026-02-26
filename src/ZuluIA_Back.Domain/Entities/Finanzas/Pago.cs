using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Finanzas;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Pago : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TerceroId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;
    public decimal Total { get; private set; }
    public string? Observacion { get; private set; }
    public EstadoPago Estado { get; private set; }

    private readonly List<PagoMedio> _medios = [];
    public IReadOnlyCollection<PagoMedio> Medios => _medios.AsReadOnly();

    private Pago() { }

    public static Pago Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        long monedaId,
        decimal cotizacion,
        string? observacion,
        long? userId)
    {
        var pago = new Pago
        {
            SucursalId  = sucursalId,
            TerceroId   = terceroId,
            Fecha       = fecha,
            MonedaId    = monedaId,
            Cotizacion  = cotizacion,
            Observacion = observacion?.Trim(),
            Estado      = EstadoPago.Activo,
            Total       = 0
        };

        pago.SetCreated(userId);
        return pago;
    }

    public void AgregarMedio(PagoMedio medio)
    {
        if (Estado != EstadoPago.Activo)
            throw new InvalidOperationException("No se pueden agregar medios a un pago anulado.");

        _medios.Add(medio);
        Total = _medios.Sum(m => m.Importe);
        AddDomainEvent(new PagoRegistradoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoPago.Anulado)
            throw new InvalidOperationException("El pago ya está anulado.");

        Estado = EstadoPago.Anulado;
        SetDeleted();
        SetUpdated(userId);
        AddDomainEvent(new PagoAnuladoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
    }
}