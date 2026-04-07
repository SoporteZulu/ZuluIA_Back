using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public record NotaPedidoImportacionInput(
    string ReferenciaExterna,
    long SucursalId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    IReadOnlyList<ComprobanteItemInput> Items);

public record ImportarNotasPedidoCommand(
    IReadOnlyList<NotaPedidoImportacionInput> NotasPedido,
    string? Observacion = null) : IRequest<Result<long>>;
