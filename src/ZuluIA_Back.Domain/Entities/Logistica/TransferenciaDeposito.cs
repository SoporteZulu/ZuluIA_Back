using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Logistica;

public class TransferenciaDeposito : AuditableEntity
{
    public long? OrdenPreparacionId { get; private set; }
    public long SucursalId { get; private set; }
    public long DepositoOrigenId { get; private set; }
    public long DepositoDestinoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public EstadoTransferenciaDeposito Estado { get; private set; }
    public string? Observacion { get; private set; }
    public DateOnly? FechaConfirmacion { get; private set; }

    private readonly List<TransferenciaDepositoDetalle> _detalles = [];
    public IReadOnlyCollection<TransferenciaDepositoDetalle> Detalles => _detalles.AsReadOnly();

    private TransferenciaDeposito() { }

    public static TransferenciaDeposito Crear(long sucursalId, long depositoOrigenId, long depositoDestinoId, DateOnly fecha, string? observacion, long? userId)
    {
        if (depositoOrigenId == depositoDestinoId)
            throw new InvalidOperationException("El depósito de origen y destino no pueden ser el mismo.");

        var transferencia = new TransferenciaDeposito
        {
            SucursalId = sucursalId,
            DepositoOrigenId = depositoOrigenId,
            DepositoDestinoId = depositoDestinoId,
            Fecha = fecha,
            Estado = EstadoTransferenciaDeposito.Borrador,
            Observacion = observacion?.Trim()
        };

        transferencia.SetCreated(userId);
        return transferencia;
    }

    public void AgregarDetalle(long itemId, decimal cantidad, string? observacion = null)
    {
        if (Estado != EstadoTransferenciaDeposito.Borrador)
            throw new InvalidOperationException("Solo se pueden agregar detalles a una transferencia en borrador.");

        _detalles.Add(TransferenciaDepositoDetalle.Crear(Id, itemId, cantidad, observacion));
    }

    public void VincularOrdenPreparacion(long ordenPreparacionId, long? userId)
    {
        if (ordenPreparacionId <= 0)
            throw new InvalidOperationException("La orden de preparación asociada es obligatoria.");

        OrdenPreparacionId = ordenPreparacionId;
        SetUpdated(userId);
    }

    public void Confirmar(DateOnly fechaConfirmacion, long? userId)
    {
        if (Estado != EstadoTransferenciaDeposito.Borrador)
            throw new InvalidOperationException("Solo se puede confirmar una transferencia en borrador.");
        if (!_detalles.Any())
            throw new InvalidOperationException("La transferencia debe tener al menos un detalle.");

        Estado = EstadoTransferenciaDeposito.Confirmada;
        FechaConfirmacion = fechaConfirmacion;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoTransferenciaDeposito.Confirmada)
            throw new InvalidOperationException("No se puede anular una transferencia confirmada.");
        if (Estado == EstadoTransferenciaDeposito.Anulada)
            throw new InvalidOperationException("La transferencia ya está anulada.");

        Estado = EstadoTransferenciaDeposito.Anulada;
        SetDeleted();
        SetUpdated(userId);
    }
}
