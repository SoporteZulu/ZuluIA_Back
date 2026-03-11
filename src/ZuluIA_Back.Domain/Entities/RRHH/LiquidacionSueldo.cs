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
            Observacion      = observacion?.Trim(),
            CreatedAt        = DateTimeOffset.UtcNow
        };
    }

    public void MarcarComoPagada() => Pagada = true;
}