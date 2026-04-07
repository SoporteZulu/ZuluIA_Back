using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Contratos;

public class ContratoImpacto : AuditableEntity
{
    public long ContratoId { get; private set; }
    public TipoImpactoContrato Tipo { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Importe { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;

    private ContratoImpacto() { }

    public static ContratoImpacto Registrar(long contratoId, TipoImpactoContrato tipo, DateOnly fecha, decimal importe, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (importe < 0)
            throw new InvalidOperationException("El importe del impacto no puede ser negativo.");

        var impacto = new ContratoImpacto
        {
            ContratoId = contratoId,
            Tipo = tipo,
            Fecha = fecha,
            Importe = importe,
            Descripcion = descripcion.Trim()
        };
        impacto.SetCreated(userId);
        return impacto;
    }
}
