using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class UpsertAfipWsfeConfiguracionCommandHandler(
    AfipWsfeService afipService,
    IUnitOfWork uow)
    : IRequestHandler<UpsertAfipWsfeConfiguracionCommand, Result<AfipWsfeConfiguracionDto>>
{
    public async Task<Result<AfipWsfeConfiguracionDto>> Handle(UpsertAfipWsfeConfiguracionCommand request, CancellationToken ct)
    {
        try
        {
            var config = await afipService.UpsertConfiguracionAsync(
                request.PuntoFacturacionId,
                request.Habilitado,
                request.Produccion,
                request.UsaCaeaPorDefecto,
                request.CuitEmisor,
                request.CertificadoAlias,
                request.Observacion,
                ct);

            await uow.SaveChangesAsync(ct);

            return Result.Success(new AfipWsfeConfiguracionDto
            {
                Id = config.Id,
                SucursalId = config.SucursalId,
                PuntoFacturacionId = config.PuntoFacturacionId,
                Habilitado = config.Habilitado,
                Produccion = config.Produccion,
                UsaCaeaPorDefecto = config.UsaCaeaPorDefecto,
                CuitEmisor = config.CuitEmisor,
                CertificadoAlias = config.CertificadoAlias,
                Observacion = config.Observacion
            });
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<AfipWsfeConfiguracionDto>(ex.Message);
        }
    }
}
