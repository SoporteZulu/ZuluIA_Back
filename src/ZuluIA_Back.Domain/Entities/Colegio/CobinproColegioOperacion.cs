using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Colegio;

public class CobinproColegioOperacion : AuditableEntity
{
    public long CedulonId { get; private set; }
    public long TerceroId { get; private set; }
    public long SucursalId { get; private set; }
    public long? CobroId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Importe { get; private set; }
    public string ReferenciaExterna { get; private set; } = string.Empty;
    public EstadoCobinproColegio Estado { get; private set; }
    public string? Observacion { get; private set; }

    private CobinproColegioOperacion() { }

    public static CobinproColegioOperacion Registrar(long cedulonId, long terceroId, long sucursalId, long? cobroId, DateOnly fecha, decimal importe, string referenciaExterna, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenciaExterna);
        if (importe <= 0)
            throw new InvalidOperationException("El importe COBINPRO debe ser mayor a 0.");

        var operacion = new CobinproColegioOperacion
        {
            CedulonId = cedulonId,
            TerceroId = terceroId,
            SucursalId = sucursalId,
            CobroId = cobroId,
            Fecha = fecha,
            Importe = importe,
            ReferenciaExterna = referenciaExterna.Trim().ToUpperInvariant(),
            Estado = EstadoCobinproColegio.Registrado,
            Observacion = observacion?.Trim()
        };

        operacion.SetCreated(userId);
        return operacion;
    }

    public void Confirmar(string? observacion, long? userId)
    {
        Estado = EstadoCobinproColegio.Confirmado;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }

    public void Rechazar(string? observacion, long? userId)
    {
        Estado = EstadoCobinproColegio.Rechazado;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }
}
