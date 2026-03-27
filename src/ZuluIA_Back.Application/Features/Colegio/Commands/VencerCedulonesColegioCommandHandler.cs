using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class VencerCedulonesColegioCommandHandler(ColegioService service, IUnitOfWork uow) : IRequestHandler<VencerCedulonesColegioCommand, Result<int>>
{
    public async Task<Result<int>> Handle(VencerCedulonesColegioCommand request, CancellationToken ct)
    {
        try
        {
            var count = await service.VencerCedulonesAsync(request.LoteId, request.SucursalId, request.FechaCorte, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(count);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }
}
