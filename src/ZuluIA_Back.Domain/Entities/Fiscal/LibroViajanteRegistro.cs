using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Fiscal;

public class LibroViajanteRegistro : AuditableEntity
{
    public long SucursalId { get; private set; }
    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public long? VendedorId { get; private set; }
    public decimal TotalVentas { get; private set; }
    public decimal TotalComision { get; private set; }

    private LibroViajanteRegistro() { }

    public static LibroViajanteRegistro Crear(long sucursalId, DateOnly desde, DateOnly hasta, long? vendedorId, decimal totalVentas, decimal totalComision, long? userId)
    {
        if (hasta < desde)
            throw new InvalidOperationException("La fecha hasta no puede ser anterior a la fecha desde.");

        var registro = new LibroViajanteRegistro
        {
            SucursalId = sucursalId,
            Desde = desde,
            Hasta = hasta,
            VendedorId = vendedorId,
            TotalVentas = totalVentas,
            TotalComision = totalComision
        };

        registro.SetCreated(userId);
        return registro;
    }
}
