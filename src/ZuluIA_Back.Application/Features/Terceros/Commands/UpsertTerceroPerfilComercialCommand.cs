using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record UpsertTerceroPerfilComercialCommand(
    long TerceroId,
    long? ZonaComercialId,
    string? Rubro,
    string? Subrubro,
    string? Sector,
    string? CondicionCobranza,
    string RiesgoCrediticio,
    decimal? SaldoMaximoVigente,
    string? VigenciaSaldo,
    DateOnly? VigenciaSaldoDesde,
    DateOnly? VigenciaSaldoHasta,
    string? CondicionVenta,
    string? PlazoCobro,
    string? FacturadorPorDefecto,
    decimal? MinimoFacturaMipymes,
    string? ObservacionComercial) : IRequest<Result<TerceroPerfilComercialDto>>;
