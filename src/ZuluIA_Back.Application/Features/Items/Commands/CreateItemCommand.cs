using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record ItemAtributoComercialInput(long AtributoComercialId, string Valor);
public record ItemComponenteInput(long ComponenteId, decimal Cantidad, long? UnidadMedidaId = null);

public record CreateItemCommand(
    string Codigo,
    string Descripcion,
    string? DescripcionAdicional,
    string? CodigoBarras,
    long UnidadMedidaId,
    long AlicuotaIvaId,
    long? AlicuotaIvaCompraId,
    long MonedaId,
    bool EsProducto,
    bool EsServicio,
    bool EsFinanciero,
    bool ManejaStock,
    long? CategoriaId,
    decimal PrecioCosto,
    decimal PrecioVenta,
    decimal StockMinimo,
    decimal? StockMaximo,
    decimal? PuntoReposicion,
    decimal? StockSeguridad,
    decimal? Peso,
    decimal? Volumen,
    string? CodigoAfip,
    long? SucursalId,
    long? MarcaId = null,
    // ── Fase 1: Campos Esenciales de Ventas ──────────────────────────────────
    bool? AplicaVentas = null,
    bool? AplicaCompras = null,
    decimal? PorcentajeGanancia = null,
    decimal? PorcentajeMaximoDescuento = null,
    bool EsRpt = false,
    bool EsSistema = false,
    string? CodigoAlternativo = null,
    bool EsTrazable = false,
    bool PermiteFraccionamiento = false,
    int? DiasVencimientoLimite = null,
    long? DepositoDefaultId = null,
    long? ImpuestoInternoId = null,
    IReadOnlyList<ItemAtributoComercialInput>? AtributosComerciales = null,
    IReadOnlyList<ItemComponenteInput>? Componentes = null
) : IRequest<Result<long>>;
