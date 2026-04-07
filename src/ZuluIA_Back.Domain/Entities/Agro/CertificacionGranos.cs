using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Agro;

public class CertificacionGranos : AuditableEntity
{
    public long LiquidacionId { get; private set; }
    public string NroCertificado { get; private set; } = string.Empty;
    public DateOnly FechaEmision { get; private set; }
    public decimal PesoNeto { get; private set; }
    public decimal Humedad { get; private set; }
    public decimal Impureza { get; private set; }
    public string? CalidadObservaciones { get; private set; }

    private CertificacionGranos() { }

    public static CertificacionGranos Crear(
        long liquidacionId,
        string nroCertificado,
        DateOnly fechaEmision,
        decimal pesoNeto,
        decimal humedad,
        decimal impureza,
        string? calidadObservaciones,
        long? userId)
    {
        var cert = new CertificacionGranos
        {
            LiquidacionId        = liquidacionId,
            NroCertificado       = nroCertificado.Trim(),
            FechaEmision         = fechaEmision,
            PesoNeto             = pesoNeto,
            Humedad              = humedad,
            Impureza             = impureza,
            CalidadObservaciones = calidadObservaciones?.Trim()
        };

        cert.SetCreated(userId);
        return cert;
    }
}
