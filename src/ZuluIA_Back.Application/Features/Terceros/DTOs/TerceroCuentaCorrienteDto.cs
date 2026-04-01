namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

/// <summary>
/// Configuración comercial de cuenta corriente del cliente.
/// Separa explícitamente el límite de saldo del límite de crédito total
/// para evitar ambigüedades respecto del legacy `frmCliente`.
/// </summary>
public class TerceroCuentaCorrienteDto
{
    public decimal? LimiteSaldo { get; set; }
    public DateOnly? VigenciaLimiteSaldoDesde { get; set; }
    public DateOnly? VigenciaLimiteSaldoHasta { get; set; }
    public decimal? LimiteCreditoTotal { get; set; }
    public DateOnly? VigenciaLimiteCreditoTotalDesde { get; set; }
    public DateOnly? VigenciaLimiteCreditoTotalHasta { get; set; }
}
