using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class ComprobanteCot : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public string Numero { get; private set; } = string.Empty;
    public DateOnly FechaVigencia { get; private set; }
    public string? Descripcion { get; private set; }

    public Comprobante Comprobante { get; private set; } = null!;

    private ComprobanteCot()
    {
    }

    public static ComprobanteCot Crear(
        long comprobanteId,
        string numero,
        DateOnly fechaVigencia,
        DateOnly fechaEmisionComprobante,
        string? descripcion,
        long? userId)
    {
        if (comprobanteId <= 0)
            throw new ArgumentException("El comprobante es obligatorio.", nameof(comprobanteId));

        Validar(numero, fechaVigencia, fechaEmisionComprobante);

        var cot = new ComprobanteCot
        {
            ComprobanteId = comprobanteId,
            Numero = numero.Trim(),
            FechaVigencia = fechaVigencia,
            Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim()
        };

        cot.SetCreated(userId);
        return cot;
    }

    public void Actualizar(string numero, DateOnly fechaVigencia, DateOnly fechaEmisionComprobante, string? descripcion, long? userId)
    {
        Validar(numero, fechaVigencia, fechaEmisionComprobante);

        Numero = numero.Trim();
        FechaVigencia = fechaVigencia;
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        SetUpdated(userId);
    }

    private static void Validar(string numero, DateOnly fechaVigencia, DateOnly fechaEmisionComprobante)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("El número de COT es obligatorio.", nameof(numero));

        if (numero.Trim().Length < 6)
            throw new ArgumentException("El número de COT debe tener al menos 6 caracteres.", nameof(numero));

        if (fechaVigencia < fechaEmisionComprobante)
            throw new ArgumentException("La fecha de vigencia del COT no puede ser anterior a la fecha de emisión del remito.", nameof(fechaVigencia));
    }
}
