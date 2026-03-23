namespace ZuluIA_Back.Application.Features.Recibos.DTOs;

public class ReciboListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Serie { get; set; } = string.Empty;
    public int Numero { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public long? CobroId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ReciboDto : ReciboListDto
{
    public string? Observacion { get; set; }
    public IReadOnlyList<ReciboItemDto> Items { get; set; } = [];
}

public class ReciboItemDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Importe { get; set; }
}
