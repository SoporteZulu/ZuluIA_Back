using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public partial record CrearBorradorVentaCommand(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    DateOnly? FechaEntregaCompromiso,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    IReadOnlyList<ComprobanteItemInput> Items,
    long? ListaPreciosId = null,
    long? VendedorId = null,
    long? CanalVentaId = null,
    long? CondicionPagoId = null,
    int? PlazoDias = null
) : IRequest<Result<long>>;

public partial record CrearBorradorVentaCommand
{
    public CrearBorradorVentaCommand(
        long sucursalId,
        long? puntoFacturacionId,
        long tipoComprobanteId,
        DateOnly fecha,
        DateOnly? fechaVencimiento,
        long terceroId,
        long monedaId,
        decimal cotizacion,
        decimal percepciones,
        string? observacion,
        long? comprobanteOrigenId,
        IReadOnlyList<ComprobanteItemInput> items,
        long? listaPreciosId = null,
        long? vendedorId = null,
        long? canalVentaId = null,
        long? condicionPagoId = null,
        int? plazoDias = null)
        : this(
            sucursalId,
            puntoFacturacionId,
            tipoComprobanteId,
            fecha,
            fechaVencimiento,
            null,
            terceroId,
            monedaId,
            cotizacion,
            percepciones,
            observacion,
            comprobanteOrigenId,
            items,
            listaPreciosId,
            vendedorId,
            canalVentaId,
            condicionPagoId,
            plazoDias)
    {
    }
}
