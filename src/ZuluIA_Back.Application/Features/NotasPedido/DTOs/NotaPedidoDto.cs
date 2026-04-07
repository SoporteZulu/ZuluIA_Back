namespace ZuluIA_Back.Application.Features.NotasPedido.DTOs;

public class NotaPedidoListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public long? VendedorId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class NotaPedidoDto : NotaPedidoListDto
{
    public string? Observacion { get; set; }
    public IReadOnlyList<NotaPedidoItemDto> Items { get; set; } = [];
}

public class NotaPedidoItemDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CantidadPendiente { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Bonificacion { get; set; }
    public decimal Subtotal { get; set; }
}
