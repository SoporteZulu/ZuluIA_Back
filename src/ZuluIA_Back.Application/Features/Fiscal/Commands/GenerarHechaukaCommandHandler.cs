using MediatR;
using ZuluIA_Back.Application.Features.Fiscal.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarHechaukaCommandHandler(
    FiscalContabilidadLocalService service,
    IUnitOfWork uow)
    : IRequestHandler<GenerarHechaukaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(GenerarHechaukaCommand request, CancellationToken ct)
    {
        var registro = await service.GenerarHechaukaAsync(request.SucursalId, request.Desde, request.Hasta, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(registro.Id);
    }
}
