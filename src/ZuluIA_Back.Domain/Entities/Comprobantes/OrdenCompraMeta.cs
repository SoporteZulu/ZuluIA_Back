using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class OrdenCompraMeta : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long ProveedorId { get; private set; }
    public DateOnly? FechaEntregaReq { get; private set; }
    public string? CondicionesEntrega { get; private set; }
    public EstadoOrdenCompra EstadoOc { get; private set; }
    public bool Habilitada { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private OrdenCompraMeta() { }

    public static OrdenCompraMeta Crear(
        long comprobanteId,
        long proveedorId,
        DateOnly? fechaEntregaReq,
        string? condicionesEntrega)
    {
        return new OrdenCompraMeta
        {
            ComprobanteId      = comprobanteId,
            ProveedorId        = proveedorId,
            FechaEntregaReq    = fechaEntregaReq,
            CondicionesEntrega = condicionesEntrega?.Trim(),
            EstadoOc           = EstadoOrdenCompra.Pendiente,
            Habilitada         = true,
            CreatedAt          = DateTimeOffset.UtcNow
        };
    }

    public void Recibir()
    {
        if (EstadoOc == EstadoOrdenCompra.Recibida)
            throw new InvalidOperationException("La orden ya fue recibida.");
        EstadoOc = EstadoOrdenCompra.Recibida;
    }

    public void Cancelar()
    {
        if (EstadoOc == EstadoOrdenCompra.Cancelada)
            throw new InvalidOperationException("La orden ya está cancelada.");
        EstadoOc   = EstadoOrdenCompra.Cancelada;
        Habilitada = false;
    }

    public void SetFechaEntrega(DateOnly fecha) => FechaEntregaReq = fecha;
}