using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Fiscal;

public class RentasBsAsRegistro : AuditableEntity
{
    public long SucursalId { get; private set; }
    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public decimal TotalPercepciones { get; private set; }
    public decimal TotalRetenciones { get; private set; }
    public string? Observacion { get; private set; }

    private RentasBsAsRegistro() { }

    public static RentasBsAsRegistro Crear(long sucursalId, DateOnly desde, DateOnly hasta, decimal totalPercepciones, decimal totalRetenciones, string? observacion, long? userId)
    {
        var registro = new RentasBsAsRegistro
        {
            SucursalId = sucursalId,
            Desde = desde,
            Hasta = hasta,
            TotalPercepciones = totalPercepciones,
            TotalRetenciones = totalRetenciones,
            Observacion = observacion?.Trim()
        };

        registro.SetCreated(userId);
        return registro;
    }
}
