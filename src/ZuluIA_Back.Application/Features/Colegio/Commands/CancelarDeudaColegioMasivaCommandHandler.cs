using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CancelarDeudaColegioMasivaCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<CancelarDeudaColegioMasivaCommand, Result<IReadOnlyList<long>>>
{
    public async Task<Result<IReadOnlyList<long>>> Handle(CancelarDeudaColegioMasivaCommand request, CancellationToken ct)
    {
        try
        {
            var ids = await service.CancelarDeudaMasivaAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(ids);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<IReadOnlyList<long>>(ex.Message);
        }
    }
}
