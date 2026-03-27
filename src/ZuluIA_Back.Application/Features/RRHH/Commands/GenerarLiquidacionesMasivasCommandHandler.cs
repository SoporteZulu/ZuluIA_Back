using MediatR;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class GenerarLiquidacionesMasivasCommandHandler(RrhhService service, IUnitOfWork uow) : IRequestHandler<GenerarLiquidacionesMasivasCommand, Result<IReadOnlyList<long>>>
{
    public async Task<Result<IReadOnlyList<long>>> Handle(GenerarLiquidacionesMasivasCommand request, CancellationToken ct)
    {
        try
        {
            var ids = await service.GenerarLiquidacionesMasivasAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(ids);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<IReadOnlyList<long>>(ex.Message);
        }
    }
}
