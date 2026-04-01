using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Common;

internal static class PedidoWorkflowRules
{
    public static bool EsPedido(string? codigo, string? descripcion)
    {
        var codigoNormalizado = codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        var descripcionNormalizada = descripcion?.Trim().ToUpperInvariant() ?? string.Empty;

        return codigoNormalizado.Contains("PED")
            || codigoNormalizado.Contains("NP")
            || descripcionNormalizada.Contains("PEDIDO");
    }

    public static bool EsRemito(string? codigo, string? descripcion, bool afectaStock)
    {
        var codigoNormalizado = codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        var descripcionNormalizada = descripcion?.Trim().ToUpperInvariant() ?? string.Empty;

        return afectaStock
            || codigoNormalizado.Contains("REM")
            || descripcionNormalizada.Contains("REMITO");
    }

    public static bool EsFactura(string? codigo, string? descripcion)
    {
        var codigoNormalizado = codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        var descripcionNormalizada = descripcion?.Trim().ToUpperInvariant() ?? string.Empty;

        return codigoNormalizado.Contains("FAC")
            || descripcionNormalizada.Contains("FACTURA");
    }

    public static string ObtenerDescripcionEstadoPedido(EstadoPedido? estadoPedido) =>
        estadoPedido switch
        {
            EstadoPedido.Pendiente => "Pendiente",
            EstadoPedido.EnProceso => "En proceso",
            EstadoPedido.Completado => "Completado",
            EstadoPedido.Cerrado => "Cerrado",
            EstadoPedido.Anulado => "Anulado",
            _ => "Sin estado"
        };

    public static string ObtenerDescripcionEstadoEntrega(EstadoEntregaItem? estadoEntrega) =>
        estadoEntrega switch
        {
            EstadoEntregaItem.NoEntregado => "No entregado",
            EstadoEntregaItem.EntregaParcial => "Entrega parcial",
            EstadoEntregaItem.EntregaCompleta => "Entrega completa",
            EstadoEntregaItem.EntregaSobrepasada => "Entrega sobrepasada",
            _ => "Sin estado"
        };
}
