using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public record CreateDocumentoVentaRequest(
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
    int? PlazoDias = null);

public record CreateNotaDebitoVentaRequest(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    long MotivoDebitoId,
    string? MotivoDebitoObservacion,
    IReadOnlyList<ComprobanteItemInput> Items,
    long? ListaPreciosId = null,
    long? VendedorId = null,
    long? CanalVentaId = null,
    long? CondicionPagoId = null,
    int? PlazoDias = null,
    bool Emitir = true);

public record EmitirRemitoRequest(bool EsValorizado, bool AfectaStock = true);
public record EmitirRemitosMasivoRequest(IReadOnlyList<long> ComprobanteIds, bool EsValorizado);
public record UpsertRemitoCotRequest(string Numero, DateOnly FechaVigencia, string? Descripcion);
public record EmitirDocumentoVentaRequest(bool AfectaStock, bool AfectaCuentaCorriente);
public record EmitirNotaDebitoVentaRequest(bool AfectaCuentaCorriente = true);

public record ConvertirDocumentoVentaRequest(
    long TipoComprobanteDestinoId,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    DateOnly? FechaEntregaCompromiso,
    string? Observacion,
    bool AfectaStock,
    bool AfectaCuentaCorriente,
    bool EsCreditoCuentaCorriente = false);

public record VincularDocumentoVentaRequest(long ComprobanteDestinoId);

public record RegistrarDevolucionVentaRequest(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    IReadOnlyList<ComprobanteItemInput> Items,
    bool ReingresaStock = true,
    bool AcreditaCuentaCorriente = true,
    MotivoDevolucion MotivoDevolucion = MotivoDevolucion.Otro,
    string? ObservacionDevolucion = null,
    long? AutorizadorDevolucionId = null);

public record CerrarPedidoRequest(string? MotivoCierre);

public record CerrarPedidosMasivoRequest(
    long? SucursalId,
    long? TerceroId,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta,
    DateOnly? FechaEntregaDesde,
    DateOnly? FechaEntregaHasta,
    bool SoloPendientes,
    string? MotivoCierre);
