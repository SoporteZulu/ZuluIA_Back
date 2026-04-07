using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.RRHH;

public class LiquidacionSueldo : BaseEntity
{
    public long EmpleadoId { get; private set; }
    public long SucursalId { get; private set; }
    public int Anio { get; private set; }
    public int Mes { get; private set; }
    public decimal SueldoBasico { get; private set; }
    public decimal TotalHaberes { get; private set; }
    public decimal TotalDescuentos { get; private set; }
    public decimal Neto { get; private set; }
    public long MonedaId { get; private set; }
    public bool Pagada { get; private set; }
    public decimal ImporteImputado { get; private set; }
    public DateOnly? FechaPago { get; private set; }
    public long? ComprobanteEmpleadoId { get; private set; }
    public string? Observacion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private LiquidacionSueldo() { }

    public static LiquidacionSueldo Crear(
        long empleadoId,
        long sucursalId,
        int anio,
        int mes,
        decimal sueldoBasico,
        decimal totalHaberes,
        decimal totalDescuentos,
        long monedaId,
        string? observacion)
    {
        if (anio < 2000 || anio > 2100)
            throw new InvalidOperationException("El año es inválido.");

        if (mes < 1 || mes > 12)
            throw new InvalidOperationException("El mes es inválido.");

        return new LiquidacionSueldo
        {
            EmpleadoId       = empleadoId,
            SucursalId       = sucursalId,
            Anio             = anio,
            Mes              = mes,
            SueldoBasico     = sueldoBasico,
            TotalHaberes     = totalHaberes,
            TotalDescuentos  = totalDescuentos,
            Neto             = totalHaberes - totalDescuentos,
            MonedaId         = monedaId,
            Pagada           = false,
            ImporteImputado  = 0,
            Observacion      = observacion?.Trim(),
            CreatedAt        = DateTimeOffset.UtcNow
        };
    }

    public decimal SaldoPendiente => Neto - ImporteImputado;

    public void RegistrarImputacion(decimal importe, DateOnly fechaPago)
    {
        if (importe <= 0)
            throw new InvalidOperationException("El importe imputado debe ser mayor a 0.");
        if (ImporteImputado + importe > Neto)
            throw new InvalidOperationException("El importe imputado supera el neto de la liquidación.");

        ImporteImputado += importe;
        FechaPago = fechaPago;
        Pagada = ImporteImputado >= Neto;
    }

    public void AsociarComprobanteEmpleado(long comprobanteEmpleadoId)
    {
        if (comprobanteEmpleadoId <= 0)
            throw new InvalidOperationException("El comprobante de empleado es obligatorio.");

        ComprobanteEmpleadoId = comprobanteEmpleadoId;
    }

    public void MarcarComoPagada()
    {
        ImporteImputado = Neto;
        Pagada = true;
    }

    public void Actualizar(decimal sueldoBasico, decimal totalHaberes, decimal totalDescuentos, long monedaId, string? observacion)
    {
        if (Pagada || ImporteImputado > 0)
            throw new InvalidOperationException("No se puede actualizar una liquidación que ya posee imputaciones.");

        SueldoBasico = sueldoBasico;
        TotalHaberes = totalHaberes;
        TotalDescuentos = totalDescuentos;
        Neto = totalHaberes - totalDescuentos;
        MonedaId = monedaId;
        Observacion = observacion?.Trim();
    }
}