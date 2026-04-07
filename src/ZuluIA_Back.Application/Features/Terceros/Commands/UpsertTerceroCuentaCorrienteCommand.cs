using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

/// <summary>
/// Actualiza la configuración de cuenta corriente del tercero.
/// </summary>
public record UpsertTerceroCuentaCorrienteCommand(
    long TerceroId,
    decimal? LimiteSaldo,
    DateOnly? VigenciaLimiteSaldoDesde,
    DateOnly? VigenciaLimiteSaldoHasta,
    decimal? LimiteCreditoTotal,
    DateOnly? VigenciaLimiteCreditoTotalDesde,
    DateOnly? VigenciaLimiteCreditoTotalHasta) : IRequest<Result<TerceroCuentaCorrienteDto>>;
