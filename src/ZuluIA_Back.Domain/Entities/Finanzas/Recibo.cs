using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Recibo : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TerceroId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string Serie { get; private set; } = string.Empty;
    public int Numero { get; private set; }
    public decimal Total { get; private set; }
    public string? Observacion { get; private set; }
    public EstadoRecibo Estado { get; private set; }
    public long? CobroId { get; private set; }

    private readonly List<ReciboItem> _items = [];
    public IReadOnlyCollection<ReciboItem> Items => _items.AsReadOnly();

    private Recibo() { }

    public static Recibo Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        string serie,
        int numero,
        string? observacion,
        long? cobroId,
        long? userId)
    {
        var recibo = new Recibo
        {
            SucursalId  = sucursalId,
            TerceroId   = terceroId,
            Fecha       = fecha,
            Serie       = serie.Trim().ToUpperInvariant(),
            Numero      = numero,
            Estado      = EstadoRecibo.Emitido,
            Observacion = observacion?.Trim(),
            CobroId     = cobroId,
            Total       = 0
        };

        recibo.SetCreated(userId);
        return recibo;
    }

    public void AgregarItem(ReciboItem item)
    {
        if (Estado != EstadoRecibo.Emitido)
            throw new InvalidOperationException("No se pueden agregar ítems a un recibo anulado.");

        _items.Add(item);
        RecalcularTotal();
    }

    private void RecalcularTotal() =>
        Total = _items.Sum(x => x.Importe);

    public void Anular(long? userId)
    {
        if (Estado == EstadoRecibo.Anulado)
            throw new InvalidOperationException("El recibo ya está anulado.");

        Estado = EstadoRecibo.Anulado;
        SetDeleted();
        SetUpdated(userId);
    }
}
