using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Compras;

public class RequisicionCompra : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long SolicitanteId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public EstadoRequisicion Estado { get; private set; }
    public string? Observacion { get; private set; }

    private readonly List<RequisicionCompraItem> _items = [];
    public IReadOnlyCollection<RequisicionCompraItem> Items => _items.AsReadOnly();

    private RequisicionCompra() { }

    public static RequisicionCompra Crear(
        long sucursalId,
        long solicitanteId,
        DateOnly fecha,
        string descripcion,
        string? observacion,
        long? userId)
    {
        var req = new RequisicionCompra
        {
            SucursalId     = sucursalId,
            SolicitanteId  = solicitanteId,
            Fecha          = fecha,
            Descripcion    = descripcion.Trim(),
            Estado         = EstadoRequisicion.Borrador,
            Observacion    = observacion?.Trim()
        };

        req.SetCreated(userId);
        return req;
    }

    public void AgregarItem(RequisicionCompraItem item)
    {
        if (Estado != EstadoRequisicion.Borrador)
            throw new InvalidOperationException("Solo se pueden agregar ítems a requisiciones en borrador.");
        _items.Add(item);
    }

    public void Enviar(long? userId)
    {
        if (Estado != EstadoRequisicion.Borrador)
            throw new InvalidOperationException("Solo se pueden enviar requisiciones en borrador.");
        if (!_items.Any())
            throw new InvalidOperationException("La requisición debe tener al menos un ítem.");
        Estado = EstadoRequisicion.Enviada;
        SetUpdated(userId);
    }

    public void Aprobar(long? userId)
    {
        if (Estado != EstadoRequisicion.Enviada)
            throw new InvalidOperationException("Solo se pueden aprobar requisiciones enviadas.");
        Estado = EstadoRequisicion.Aprobada;
        SetUpdated(userId);
    }

    public void Rechazar(string? motivo, long? userId)
    {
        if (Estado != EstadoRequisicion.Enviada)
            throw new InvalidOperationException("Solo se pueden rechazar requisiciones enviadas.");
        Estado = EstadoRequisicion.Rechazada;
        Observacion = motivo?.Trim();
        SetUpdated(userId);
    }

    public void Cancelar(long? userId)
    {
        if (Estado == EstadoRequisicion.Procesada || Estado == EstadoRequisicion.Cancelada)
            throw new InvalidOperationException($"No se puede cancelar una requisición {Estado}.");
        Estado = EstadoRequisicion.Cancelada;
        SetDeleted();
        SetUpdated(userId);
    }

    public void MarcarProcesada(long? userId)
    {
        if (Estado != EstadoRequisicion.Aprobada)
            throw new InvalidOperationException("Solo se pueden procesar requisiciones aprobadas.");
        Estado = EstadoRequisicion.Procesada;
        SetUpdated(userId);
    }
}
