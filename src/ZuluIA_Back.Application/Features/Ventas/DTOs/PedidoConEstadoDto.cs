using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.DTOs;

public class PedidoConEstadoDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string SucursalRazonSocial { get; set; } = string.Empty;
    public long TipoComprobanteId { get; set; }
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaEntregaCompromiso { get; set; }
    public long TerceroId { get; set; }
    public string ClienteRazonSocial { get; set; } = string.Empty;
    public string? ClienteLegajo { get; set; }
    public string? ClienteCuit { get; set; }
    public string? ClienteCondicionIva { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public EstadoPedido? EstadoPedido { get; set; }
    public string EstadoPedidoDescripcion { get; set; } = string.Empty;
    public bool EsAtrasado { get; set; }
    public int CantidadRenglones { get; set; }
    public int RenglonesCompletados { get; set; }
    public int RenglonesPendientes { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
    public string? Observacion { get; set; }
    public IReadOnlyList<PedidoItemConEstadoDto> Items { get; set; } = [];
}

public class PedidoItemConEstadoDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public string? Concepto { get; set; }
    public decimal CantidadPedida { get; set; }
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadPendiente { get; set; }
    public decimal Diferencia { get; set; }
    public EstadoEntregaItem? EstadoEntrega { get; set; }
    public string EstadoEntregaDescripcion { get; set; } = string.Empty;
    public bool EsAtrasado { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal TotalLinea { get; set; }
}
