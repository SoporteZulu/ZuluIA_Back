using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Retencion : BaseEntity
{
    public long? PagoId { get; private set; }
    public long? CobroId { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public decimal Importe { get; private set; }
    public string? NroCertificado { get; private set; }
    public DateOnly Fecha { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Retencion() { }

    public static Retencion CrearEnPago(
        long pagoId,
        string tipo,
        decimal importe,
        string? nroCertificado,
        DateOnly fecha)
    {
        ValidarRetencion(tipo, importe);
        return new Retencion
        {
            PagoId         = pagoId,
            Tipo           = tipo.Trim().ToUpperInvariant(),
            Importe        = importe,
            NroCertificado = nroCertificado?.Trim(),
            Fecha          = fecha,
            CreatedAt      = DateTimeOffset.UtcNow
        };
    }

    public static Retencion CrearEnCobro(
        long cobroId,
        string tipo,
        decimal importe,
        string? nroCertificado,
        DateOnly fecha)
    {
        ValidarRetencion(tipo, importe);
        return new Retencion
        {
            CobroId        = cobroId,
            Tipo           = tipo.Trim().ToUpperInvariant(),
            Importe        = importe,
            NroCertificado = nroCertificado?.Trim(),
            Fecha          = fecha,
            CreatedAt      = DateTimeOffset.UtcNow
        };
    }

    private static void ValidarRetencion(string tipo, decimal importe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipo);
        if (importe <= 0)
            throw new InvalidOperationException(
                "El importe de la retención debe ser mayor a 0.");
    }
}