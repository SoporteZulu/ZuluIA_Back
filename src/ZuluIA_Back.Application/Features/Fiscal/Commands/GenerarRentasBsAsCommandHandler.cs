using MediatR;
using ZuluIA_Back.Application.Features.Fiscal.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarRentasBsAsCommandHandler(
    FiscalContabilidadLocalService service,
    IUnitOfWork uow)
    : IRequestHandler<GenerarRentasBsAsCommand, Result<long>>
{
    public async Task<Result<long>> Handle(GenerarRentasBsAsCommand request, CancellationToken ct)
    {
        var registro = await service.GenerarRentasBsAsAsync(request.SucursalId, request.Desde, request.Hasta, request.Observacion, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(registro.Id);
    }
}
