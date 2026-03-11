namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class BalanceSumasYSaldosDto
{
    public long EjercicioId { get; set; }
    public string EjercicioDescripcion { get; set; } = string.Empty;
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public IReadOnlyList<BalanceLineaDto> Lineas { get; set; } = [];
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public decimal TotalSaldoDeudor { get; set; }
    public decimal TotalSaldoAcreedor { get; set; }
}

public class BalanceLineaDto
{
    public long CuentaId { get; set; }
    public string CodigoCuenta { get; set; } = string.Empty;
    public string Denominacion { get; set; } = string.Empty;
    public short Nivel { get; set; }
    public decimal SumasDebe { get; set; }
    public decimal SumasHaber { get; set; }
    public decimal SaldoDeudor { get; set; }
    public decimal SaldoAcreedor { get; set; }
}