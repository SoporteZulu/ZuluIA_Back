using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class TesoreriaCierre : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long CajaCuentaId { get; private set; }
    public int NroCierre { get; private set; }
    public DateOnly Fecha { get; private set; }
    public bool EsApertura { get; private set; }
    public decimal SaldoInformado { get; private set; }
    public decimal SaldoSistema { get; private set; }
    public decimal TotalIngresos { get; private set; }
    public decimal TotalEgresos { get; private set; }
    public int CantidadMovimientos { get; private set; }
    public string? Observacion { get; private set; }

    private TesoreriaCierre() { }

    public static TesoreriaCierre RegistrarApertura(
        long sucursalId,
        long cajaCuentaId,
        int nroCierre,
        DateOnly fecha,
        decimal saldoInicial,
        string? observacion,
        long? userId)
    {
        var cierre = new TesoreriaCierre
        {
            SucursalId = sucursalId,
            CajaCuentaId = cajaCuentaId,
            NroCierre = nroCierre,
            Fecha = fecha,
            EsApertura = true,
            SaldoInformado = saldoInicial,
            SaldoSistema = saldoInicial,
            TotalIngresos = 0m,
            TotalEgresos = 0m,
            CantidadMovimientos = 0,
            Observacion = observacion?.Trim()
        };

        cierre.SetCreated(userId);
        return cierre;
    }

    public static TesoreriaCierre RegistrarCierre(
        long sucursalId,
        long cajaCuentaId,
        int nroCierre,
        DateOnly fecha,
        decimal saldoInformado,
        decimal saldoSistema,
        decimal totalIngresos,
        decimal totalEgresos,
        int cantidadMovimientos,
        string? observacion,
        long? userId)
    {
        var cierre = new TesoreriaCierre
        {
            SucursalId = sucursalId,
            CajaCuentaId = cajaCuentaId,
            NroCierre = nroCierre,
            Fecha = fecha,
            EsApertura = false,
            SaldoInformado = saldoInformado,
            SaldoSistema = saldoSistema,
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            CantidadMovimientos = cantidadMovimientos,
            Observacion = observacion?.Trim()
        };

        cierre.SetCreated(userId);
        return cierre;
    }
}
