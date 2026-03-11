namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class PlanCuentaDto
{
    public long Id { get; set; }
    public long EjercicioId { get; set; }
    public long? IntegradoraId { get; set; }
    public string CodigoCuenta { get; set; } = string.Empty;
    public string Denominacion { get; set; } = string.Empty;
    public short Nivel { get; set; }
    public string OrdenNivel { get; set; } = string.Empty;
    public bool Imputable { get; set; }
    public string? Tipo { get; set; }
    public char? SaldoNormal { get; set; }
    public IReadOnlyList<PlanCuentaDto> Subcuentas { get; set; } = [];
}