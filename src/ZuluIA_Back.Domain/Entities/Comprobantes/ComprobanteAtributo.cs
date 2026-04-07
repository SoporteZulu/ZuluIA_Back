using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class ComprobanteAtributo : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public string Clave { get; private set; } = string.Empty;
    public string? Valor { get; private set; }
    public string? TipoDato { get; private set; }

    public Comprobante Comprobante { get; private set; } = null!;

    private ComprobanteAtributo()
    {
    }

    public static ComprobanteAtributo Crear(long comprobanteId, string clave, string? valor, string? tipoDato, long? userId)
    {
        if (comprobanteId <= 0)
            throw new ArgumentException("El comprobante es obligatorio.", nameof(comprobanteId));

        if (string.IsNullOrWhiteSpace(clave))
            throw new ArgumentException("La clave del atributo es obligatoria.", nameof(clave));

        var atributo = new ComprobanteAtributo
        {
            ComprobanteId = comprobanteId,
            Clave = clave.Trim(),
            Valor = string.IsNullOrWhiteSpace(valor) ? null : valor.Trim(),
            TipoDato = string.IsNullOrWhiteSpace(tipoDato) ? null : tipoDato.Trim()
        };

        atributo.SetCreated(userId);
        return atributo;
    }

    public void ActualizarValor(string? valor, long? userId)
    {
        Valor = string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        SetUpdated(userId);
    }
}
