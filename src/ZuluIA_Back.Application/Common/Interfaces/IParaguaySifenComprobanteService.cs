using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Application.Common.Interfaces;

public interface IParaguaySifenComprobanteService
{
    Task<PreparacionSifenParaguayDto?> PrepararAsync(long comprobanteId, CancellationToken cancellationToken = default);

    Task<Result<ResultadoEnvioSifenParaguayDto>> EnviarAsync(
        Comprobante comprobante,
        CancellationToken cancellationToken = default);

    Task<Result<ResultadoEnvioSifenParaguayDto>> ConciliarEstadoAsync(
        Comprobante comprobante,
        CancellationToken cancellationToken = default);
}