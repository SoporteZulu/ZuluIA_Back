using MediatR;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

/// <summary>
/// Obtiene el detalle completo de un cheque incluyendo historial
/// </summary>
public record GetChequeDetalleQuery(long ChequeId) 
    : IRequest<ChequeDetalleDto?>;

/// <summary>
/// Obtiene cheques propios paginados
/// </summary>
public record GetChequesPropiosPagedQuery(
    int Page,
    int PageSize,
    long? ChequeraId,
    EstadoCheque? Estado,
    long? CajaId,
    DateOnly? Desde,
    DateOnly? Hasta,
    string? NroCheque
) : IRequest<PagedResult<ChequePropio>>;

/// <summary>
/// Obtiene cheques de terceros pendientes de depósito
/// </summary>
public record GetChequesPendientesQuery(
    long? CajaId,
    DateOnly? HastaFechaVencimiento,
    bool SoloVencidos
) : IRequest<IReadOnlyList<ChequePendienteDto>>;

/// <summary>
/// Obtiene cheques depositados no acreditados
/// </summary>
public record GetChequesDepositadosQuery(
    long? CajaId,
    DateOnly? Desde,
    DateOnly? Hasta
) : IRequest<IReadOnlyList<ChequeDto>>;

/// <summary>
/// Obtiene el historial/ruta completa de un cheque
/// </summary>
public record GetChequeHistorialQuery(long ChequeId)
    : IRequest<IReadOnlyList<ChequeRutaItemDto>>;

/// <summary>
/// Obtiene cheques con filtros avanzados extendidos
/// </summary>
public record GetChequesAvanzadoQuery(
    int Page,
    int PageSize,
    long? CajaId,
    long? TerceroId,
    EstadoCheque? Estado,
    TipoCheque? Tipo,
    string? Banco,
    string? NroCheque,
    string? Titular,
    bool? EsALaOrden,
    bool? EsCruzado,
    DateOnly? FechaEmisionDesde,
    DateOnly? FechaEmisionHasta,
    DateOnly? FechaVencimientoDesde,
    DateOnly? FechaVencimientoHasta,
    decimal? ImporteMinimo,
    decimal? ImporteMaximo,
    long? ChequeraId,
    bool IncluirAnulados
) : IRequest<PagedResult<ChequeDto>>;
