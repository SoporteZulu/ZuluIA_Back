namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class EjercicioDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string MascaraCuentaContable { get; set; } = string.Empty;
    public bool Cerrado { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyList<EjercicioSucursalDto> Sucursales { get; set; } = [];
}

public class EjercicioSucursalDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string SucursalDescripcion { get; set; } = string.Empty;
    public bool UsaContabilidad { get; set; }
}