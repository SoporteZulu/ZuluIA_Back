using MediatR;
using ZuluIA_Back.Application.Features.Fiscal.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarSalidaRegulatoriaCommandHandler(
    FiscalContabilidadLocalService service,
    IUnitOfWork uow)
    : IRequestHandler<GenerarSalidaRegulatoriaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(GenerarSalidaRegulatoriaCommand request, CancellationToken ct)
    {
        var salida = await service.GenerarSalidaRegulatoriaAsync(request.Tipo, request.SucursalId, request.Desde, request.Hasta, request.NombreArchivo, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(salida.Id);
    }
}
