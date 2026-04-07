using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Fiscal;

public class HechaukaRegistro : AuditableEntity
{
    public long SucursalId { get; private set; }
    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public decimal TotalNetoGravado { get; private set; }
    public decimal TotalIva { get; private set; }
    public decimal TotalComprobantes { get; private set; }

    private HechaukaRegistro() { }

    public static HechaukaRegistro Crear(long sucursalId, DateOnly desde, DateOnly hasta, decimal totalNetoGravado, decimal totalIva, decimal totalComprobantes, long? userId)
    {
        var registro = new HechaukaRegistro
        {
            SucursalId = sucursalId,
            Desde = desde,
            Hasta = hasta,
            TotalNetoGravado = totalNetoGravado,
            TotalIva = totalIva,
            TotalComprobantes = totalComprobantes
        };

        registro.SetCreated(userId);
        return registro;
    }
}
