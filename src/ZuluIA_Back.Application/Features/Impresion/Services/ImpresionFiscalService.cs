using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Impresion.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Application.Features.Impresion.Interfaces;

namespace ZuluIA_Back.Application.Features.Impresion.Services;

public class ImpresionFiscalService(
    IMediator mediator,
    IEnumerable<IImpresoraFiscalAdapter> adapters)
{
    public async Task<ResultadoImpresionFiscalDto> ImprimirComprobanteAsync(long comprobanteId, MarcaImpresoraFiscal marca, CancellationToken ct)
    {
        var comprobante = await mediator.Send(new GetComprobanteDetalleQuery(comprobanteId), ct)
            ?? throw new InvalidOperationException($"No se encontró el comprobante ID {comprobanteId}.");

        var adapter = adapters.FirstOrDefault(x => x.Marca == marca)
            ?? throw new InvalidOperationException($"No se encontró un adaptador para la impresora {marca}.");

        return adapter.Imprimir(comprobante);
    }
}
