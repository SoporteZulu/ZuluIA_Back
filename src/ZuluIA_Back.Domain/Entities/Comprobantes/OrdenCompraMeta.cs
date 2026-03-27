using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class OrdenCompraMeta : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long ProveedorId { get; private set; }
    public DateOnly? FechaEntregaReq { get; private set; }
    public string? CondicionesEntrega { get; private set; }
    public decimal CantidadTotal { get; private set; }
    public decimal CantidadRecibida { get; private set; }
    public DateOnly? FechaUltimaRecepcion { get; private set; }
    public EstadoOrdenCompra EstadoOc { get; private set; }
    public bool Habilitada { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private OrdenCompraMeta() { }

    public static OrdenCompraMeta Crear(
        long comprobanteId,
        long proveedorId,
        DateOnly? fechaEntregaReq,
        string? condicionesEntrega,
        decimal cantidadTotal)
    {
        if (cantidadTotal <= 0)
            throw new InvalidOperationException("La orden de compra debe tener una cantidad total mayor a 0.");

        return new OrdenCompraMeta
        {
            ComprobanteId      = comprobanteId,
            ProveedorId        = proveedorId,
            FechaEntregaReq    = fechaEntregaReq,
            CondicionesEntrega = condicionesEntrega?.Trim(),
            CantidadTotal      = cantidadTotal,
            CantidadRecibida   = 0,
            EstadoOc           = EstadoOrdenCompra.Pendiente,
            Habilitada         = true,
            CreatedAt          = DateTimeOffset.UtcNow
        };
    }

    public void Recibir()
    {
        RegistrarRecepcion(CantidadTotal - CantidadRecibida, FechaEntregaReq ?? DateOnly.FromDateTime(DateTime.Today));
    }

    public void RegistrarRecepcion(decimal cantidadRecibida, DateOnly fechaRecepcion)
    {
        if (EstadoOc == EstadoOrdenCompra.Cancelada)
            throw new InvalidOperationException("No se puede recibir una orden cancelada.");

        if (EstadoOc == EstadoOrdenCompra.Recibida)
            throw new InvalidOperationException("La orden ya fue recibida completamente.");

        if (cantidadRecibida <= 0)
            throw new InvalidOperationException("La cantidad recibida debe ser mayor a 0.");

        var saldoPendiente = CantidadTotal - CantidadRecibida;
        if (cantidadRecibida > saldoPendiente)
            throw new InvalidOperationException("La cantidad recibida supera el saldo pendiente de la orden.");

        CantidadRecibida += cantidadRecibida;
        FechaUltimaRecepcion = fechaRecepcion;

        EstadoOc = CantidadRecibida >= CantidadTotal
            ? EstadoOrdenCompra.Recibida
            : EstadoOrdenCompra.ParcialmenteRecibida;
    }

    public void Cancelar()
    {
        if (EstadoOc == EstadoOrdenCompra.Recibida)
            throw new InvalidOperationException("No se puede cancelar una orden ya recibida.");

        if (EstadoOc == EstadoOrdenCompra.Cancelada)
            throw new InvalidOperationException("La orden ya está cancelada.");
        EstadoOc   = EstadoOrdenCompra.Cancelada;
        Habilitada = false;
    }

    public void SetFechaEntrega(DateOnly fecha) => FechaEntregaReq = fecha;
}