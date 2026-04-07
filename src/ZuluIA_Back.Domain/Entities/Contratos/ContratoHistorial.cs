using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Contratos;

public class ContratoHistorial : AuditableEntity
{
    public long ContratoId { get; private set; }
    public TipoEventoContrato TipoEvento { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal? Importe { get; private set; }

    private ContratoHistorial() { }

    public static ContratoHistorial Registrar(long contratoId, TipoEventoContrato tipoEvento, DateOnly fecha, string descripcion, decimal? importe, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var item = new ContratoHistorial
        {
            ContratoId = contratoId,
            TipoEvento = tipoEvento,
            Fecha = fecha,
            Descripcion = descripcion.Trim(),
            Importe = importe
        };
        item.SetCreated(userId);
        return item;
    }
}
