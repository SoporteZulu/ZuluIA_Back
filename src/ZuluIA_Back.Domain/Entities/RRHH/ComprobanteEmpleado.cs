using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.RRHH;

public class ComprobanteEmpleado : AuditableEntity
{
    public long EmpleadoId { get; private set; }
    public long LiquidacionSueldoId { get; private set; }
    public long SucursalId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public decimal Total { get; private set; }
    public long MonedaId { get; private set; }
    public string? Observacion { get; private set; }

    private ComprobanteEmpleado() { }

    public static ComprobanteEmpleado Crear(long empleadoId, long liquidacionSueldoId, long sucursalId, DateOnly fecha, string tipo, string numero, decimal total, long monedaId, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipo);
        ArgumentException.ThrowIfNullOrWhiteSpace(numero);
        if (total <= 0)
            throw new InvalidOperationException("El total del comprobante de empleado debe ser mayor a 0.");

        var comprobante = new ComprobanteEmpleado
        {
            EmpleadoId = empleadoId,
            LiquidacionSueldoId = liquidacionSueldoId,
            SucursalId = sucursalId,
            Fecha = fecha,
            Tipo = tipo.Trim().ToUpperInvariant(),
            Numero = numero.Trim().ToUpperInvariant(),
            Total = total,
            MonedaId = monedaId,
            Observacion = observacion?.Trim()
        };

        comprobante.SetCreated(userId);
        return comprobante;
    }
}
