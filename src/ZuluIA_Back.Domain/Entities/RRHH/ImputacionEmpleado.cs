using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.RRHH;

public class ImputacionEmpleado : AuditableEntity
{
    public long LiquidacionSueldoId { get; private set; }
    public long? ComprobanteEmpleadoId { get; private set; }
    public long? TesoreriaMovimientoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Importe { get; private set; }
    public string? Observacion { get; private set; }

    private ImputacionEmpleado() { }

    public static ImputacionEmpleado Registrar(long liquidacionSueldoId, long? comprobanteEmpleadoId, long? tesoreriaMovimientoId, DateOnly fecha, decimal importe, string? observacion, long? userId)
    {
        if (importe <= 0)
            throw new InvalidOperationException("El importe de la imputación de empleado debe ser mayor a 0.");

        var imputacion = new ImputacionEmpleado
        {
            LiquidacionSueldoId = liquidacionSueldoId,
            ComprobanteEmpleadoId = comprobanteEmpleadoId,
            TesoreriaMovimientoId = tesoreriaMovimientoId,
            Fecha = fecha,
            Importe = importe,
            Observacion = observacion?.Trim()
        };

        imputacion.SetCreated(userId);
        return imputacion;
    }
}
