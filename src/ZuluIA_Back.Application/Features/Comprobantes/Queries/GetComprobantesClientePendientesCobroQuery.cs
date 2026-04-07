using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

/// <summary>
/// Query para obtener comprobantes pendientes de cobro de un cliente
/// </summary>
public record GetComprobantesClientePendientesCobroQuery(
    long TerceroId,
    long? SucursalId = null,
    long? MonedaId = null,
    bool SoloVencidos = false,
    DateOnly? FechaHasta = null
) : IRequest<IReadOnlyList<ComprobantePendienteCobroDto>>;
