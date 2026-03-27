using MediatR;
using ZuluIA_Back.Application.Features.Fiscal.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarLibroViajantesCommandHandler(
    FiscalContabilidadLocalService service,
    IUnitOfWork uow)
    : IRequestHandler<GenerarLibroViajantesCommand, Result<int>>
{
    public async Task<Result<int>> Handle(GenerarLibroViajantesCommand request, CancellationToken ct)
    {
        var registros = await service.GenerarLibroViajantesAsync(request.SucursalId, request.Desde, request.Hasta, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(registros.Count);
    }
}
