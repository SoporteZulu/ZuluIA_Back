using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.DTOs;

public class PedidoVinculacionesDto
{
    public long PedidoId { get; set; }
    public string PedidoNumero { get; set; } = string.Empty;
    public EstadoPedido? EstadoPedido { get; set; }
    public string EstadoPedidoDescripcion { get; set; } = string.Empty;
    public IReadOnlyList<ComprobanteVinculadoDto> RemitosGenerados { get; set; } = [];
    public IReadOnlyList<ComprobanteVinculadoDto> FacturasAsociadas { get; set; } = [];
    public IReadOnlyList<CumplimientoRenglonDto> CumplimientoPorRenglon { get; set; } = [];
}

public class ComprobanteVinculadoDto
{
    public long Id { get; set; }
    public long TipoComprobanteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class CumplimientoRenglonDto
{
    public long ComprobanteItemId { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public decimal CantidadPedida { get; set; }
    public decimal CantidadEntregada { get; set; }
    public decimal Diferencia { get; set; }
    public string EstadoEntregaDescripcion { get; set; } = string.Empty;
    public bool EsAtrasado { get; set; }
    public IReadOnlyList<EntregaDetalleDto> Entregas { get; set; } = [];
}

public class EntregaDetalleDto
{
    public long ComprobanteId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal CantidadEntregada { get; set; }
}
