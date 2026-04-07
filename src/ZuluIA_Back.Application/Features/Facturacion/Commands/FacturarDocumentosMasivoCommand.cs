using MediatR;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record FacturarDocumentosMasivoCommand(
    IReadOnlyList<long> ComprobanteOrigenIds,
    long TipoComprobanteDestinoId,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion,
    OperacionStockVenta OperacionStock,
    OperacionCuentaCorrienteVenta OperacionCuentaCorriente,
    bool AutorizarAfip = false,
    bool UsarCaea = false,
    string? ClaveIdempotencia = null) : IRequest<Result<long>>;
