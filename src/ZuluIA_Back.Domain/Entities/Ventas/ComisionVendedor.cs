using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Ventas;

public class ComisionVendedor : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long VendedorId { get; private set; }
    /// <summary>Período en formato YYYYMM.</summary>
    public int Periodo { get; private set; }
    public decimal MontoBase { get; private set; }
    public decimal PorcentajeComision { get; private set; }
    public decimal MontoComision { get; private set; }
    public EstadoComision Estado { get; private set; }
    public string? Observacion { get; private set; }

    private ComisionVendedor() { }

    public static ComisionVendedor Crear(
        long sucursalId,
        long vendedorId,
        int periodo,
        decimal montoBase,
        decimal porcentajeComision,
        string? observacion,
        long? userId)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(montoBase);
        ArgumentOutOfRangeException.ThrowIfNegative(porcentajeComision);
        var comision = new ComisionVendedor
        {
            SucursalId         = sucursalId,
            VendedorId         = vendedorId,
            Periodo            = periodo,
            MontoBase          = montoBase,
            PorcentajeComision = porcentajeComision,
            MontoComision      = montoBase * porcentajeComision / 100,
            Estado             = EstadoComision.Pendiente,
            Observacion        = observacion?.Trim()
        };

        comision.SetCreated(userId);
        return comision;
    }

    public void Aprobar(long? userId)
    {
        if (Estado != EstadoComision.Pendiente)
            throw new InvalidOperationException("Solo se pueden aprobar comisiones pendientes.");
        Estado = EstadoComision.Aprobada;
        SetUpdated(userId);
    }

    public void MarcarPagada(long? userId)
    {
        if (Estado != EstadoComision.Aprobada)
            throw new InvalidOperationException("Solo se pueden pagar comisiones aprobadas.");
        Estado = EstadoComision.Pagada;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoComision.Anulada)
            throw new InvalidOperationException("La comisión ya está anulada.");
        Estado = EstadoComision.Anulada;
        SetDeleted();
        SetUpdated(userId);
    }
}
