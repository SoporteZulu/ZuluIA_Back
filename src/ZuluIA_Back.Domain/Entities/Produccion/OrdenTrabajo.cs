using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Produccion;

public class OrdenTrabajo : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long FormulaId { get; private set; }
    public long DepositoOrigenId { get; private set; }
    public long DepositoDestinoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public DateOnly? FechaFinPrevista { get; private set; }
    public DateOnly? FechaFinReal { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal? CantidadProducida { get; private set; }
    public EstadoOrdenTrabajo Estado { get; private set; }
    public string? Observacion { get; private set; }

    private OrdenTrabajo() { }

    public static OrdenTrabajo Crear(
        long sucursalId,
        long formulaId,
        long depositoOrigenId,
        long depositoDestinoId,
        DateOnly fecha,
        DateOnly? fechaFinPrevista,
        decimal cantidad,
        string? observacion,
        long? userId)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException(
                "La cantidad a producir debe ser mayor a 0.");

        var ot = new OrdenTrabajo
        {
            SucursalId        = sucursalId,
            FormulaId         = formulaId,
            DepositoOrigenId  = depositoOrigenId,
            DepositoDestinoId = depositoDestinoId,
            Fecha             = fecha,
            FechaFinPrevista  = fechaFinPrevista,
            Cantidad          = cantidad,
            Estado            = EstadoOrdenTrabajo.Pendiente,
            Observacion       = observacion?.Trim()
        };

        ot.SetCreated(userId);
        return ot;
    }

    public void Iniciar(long? userId)
    {
        if (Estado != EstadoOrdenTrabajo.Pendiente)
            throw new InvalidOperationException(
                $"No se puede iniciar una OT en estado {Estado}.");

        Estado = EstadoOrdenTrabajo.EnProceso;
        SetUpdated(userId);
    }

    public void Finalizar(DateOnly fechaFinReal, long? userId)
        => Finalizar(fechaFinReal, Cantidad, userId);

    public void Finalizar(DateOnly fechaFinReal, decimal cantidadProducida, long? userId)
    {
        if (Estado != EstadoOrdenTrabajo.EnProceso)
            throw new InvalidOperationException(
                $"No se puede finalizar una OT en estado {Estado}.");

        if (cantidadProducida <= 0)
            throw new InvalidOperationException("La cantidad producida debe ser mayor a 0.");

        Estado       = EstadoOrdenTrabajo.Finalizada;
        FechaFinReal = fechaFinReal;
        CantidadProducida = cantidadProducida;
        SetUpdated(userId);
    }

    public void AjustarCantidad(decimal cantidad, string? observacion, long? userId)
    {
        if (Estado == EstadoOrdenTrabajo.Finalizada)
            throw new InvalidOperationException("No se puede ajustar una OT finalizada.");

        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad ajustada debe ser mayor a 0.");

        Cantidad = cantidad;
        if (!string.IsNullOrWhiteSpace(observacion))
            Observacion = observacion.Trim();
        SetUpdated(userId);
    }

    public void Cancelar(long? userId)
    {
        if (Estado == EstadoOrdenTrabajo.Finalizada)
            throw new InvalidOperationException(
                "No se puede cancelar una OT finalizada.");

        Estado = EstadoOrdenTrabajo.Cancelada;
        SetDeleted();
        SetUpdated(userId);
    }
}