using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Logistica;

/// <summary>
/// Orden de preparación / picking para despacho de mercaderías.
/// Equivale a clsOrdenDePreparacion / frmOrdenDePreparacion del VB6.
/// Permite preparar físicamente los ítems de una venta o transferencia antes de emitir el remito.
/// </summary>
public class OrdenPreparacion : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long? ComprobanteOrigenId { get; private set; }
    public long? TerceroId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public EstadoOrdenPreparacion Estado { get; private set; }
    public string? Observacion { get; private set; }
    public DateOnly? FechaConfirmacion { get; private set; }

    private readonly List<OrdenPreparacionDetalle> _detalles = [];
    public IReadOnlyCollection<OrdenPreparacionDetalle> Detalles => _detalles.AsReadOnly();

    private OrdenPreparacion() { }

    public static OrdenPreparacion Crear(
        long sucursalId,
        long? comprobanteOrigenId,
        long? terceroId,
        DateOnly fecha,
        string? observacion,
        long? userId)
    {
        var orden = new OrdenPreparacion
        {
            SucursalId          = sucursalId,
            ComprobanteOrigenId = comprobanteOrigenId,
            TerceroId           = terceroId,
            Fecha               = fecha,
            Estado              = EstadoOrdenPreparacion.Pendiente,
            Observacion         = observacion?.Trim()
        };

        orden.SetCreated(userId);
        return orden;
    }

    public void AgregarDetalle(long itemId, long depositoId, decimal cantidad, string? observacion = null)
    {
        if (Estado != EstadoOrdenPreparacion.Pendiente && Estado != EstadoOrdenPreparacion.EnProceso)
            throw new InvalidOperationException("No se pueden agregar detalles a una orden que no está pendiente o en proceso.");

        _detalles.Add(OrdenPreparacionDetalle.Crear(Id, itemId, depositoId, cantidad, observacion));
    }

    public void IniciarPreparacion(long? userId)
    {
        if (Estado != EstadoOrdenPreparacion.Pendiente)
            throw new InvalidOperationException("Solo se puede iniciar una orden en estado Pendiente.");

        if (!_detalles.Any())
            throw new InvalidOperationException("No se puede iniciar una orden sin detalles.");

        Estado = EstadoOrdenPreparacion.EnProceso;
        SetUpdated(userId);
    }

    public void Confirmar(DateOnly fechaConfirmacion, long? userId)
    {
        if (Estado != EstadoOrdenPreparacion.EnProceso)
            throw new InvalidOperationException("Solo se puede confirmar una orden en estado En Proceso.");

        Estado             = EstadoOrdenPreparacion.Completada;
        FechaConfirmacion  = fechaConfirmacion;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoOrdenPreparacion.Completada)
            throw new InvalidOperationException("No se puede anular una orden ya completada.");
        if (Estado == EstadoOrdenPreparacion.Anulada)
            throw new InvalidOperationException("La orden ya está anulada.");

        Estado = EstadoOrdenPreparacion.Anulada;
        SetDeleted();
        SetUpdated(userId);
    }
}
