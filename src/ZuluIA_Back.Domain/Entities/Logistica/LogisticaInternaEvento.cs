using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Logistica;

public class LogisticaInternaEvento : AuditableEntity
{
    public long? OrdenPreparacionId { get; private set; }
    public long? TransferenciaDepositoId { get; private set; }
    public TipoEventoLogisticaInterna TipoEvento { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;

    private LogisticaInternaEvento() { }

    public static LogisticaInternaEvento Registrar(long? ordenPreparacionId, long? transferenciaDepositoId, TipoEventoLogisticaInterna tipoEvento, DateOnly fecha, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (!ordenPreparacionId.HasValue && !transferenciaDepositoId.HasValue)
            throw new InvalidOperationException("El evento debe estar asociado a una orden o a una transferencia.");

        var evento = new LogisticaInternaEvento
        {
            OrdenPreparacionId = ordenPreparacionId,
            TransferenciaDepositoId = transferenciaDepositoId,
            TipoEvento = tipoEvento,
            Fecha = fecha,
            Descripcion = descripcion.Trim()
        };

        evento.SetCreated(userId);
        return evento;
    }
}
