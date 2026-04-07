using MediatR;
using ZuluIA_Back.Application.Features.Cheques.DTOs;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

/// <summary>
/// Query para obtener cheques de terceros disponibles para endosar/usar en pagos
/// </summary>
public record GetChequesTerceroDisponiblesQuery(
    long? BancoId = null,
    long? MonedaId = null,
    decimal? ImporteMinimo = null,
    decimal? ImporteMaximo = null,
    DateOnly? FechaPagoDesde = null,
    DateOnly? FechaPagoHasta = null,
    string? Plaza = null
) : IRequest<IReadOnlyList<ChequeDisponibleDto>>;
