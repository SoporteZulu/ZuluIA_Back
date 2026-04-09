namespace ZuluIA_Back.Application.Features.Compras.DTOs;

public class RequisicionCompraListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long SolicitanteId { get; set; }
    public string SolicitanteNombre { get; set; } = string.Empty;
    public string Solicitante => SolicitanteNombre;
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Motivo => Descripcion;
    public string Estado { get; set; } = string.Empty;
    public string EstadoLegacy { get; set; } = string.Empty;
    public string RequisicionReferencia => $"REQ-{Id}";
    public int CantidadItems { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class RequisicionCompraDto : RequisicionCompraListDto
{
    public string? Observacion { get; set; }
    public IReadOnlyList<RequisicionCompraItemDto> Items { get; set; } = [];
}

public class RequisicionCompraItemDto
{
    public long Id { get; set; }
    public long? ItemId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}
