using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CancelarDeudaColegioCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<CancelarDeudaColegioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CancelarDeudaColegioCommand request, CancellationToken ct)
    {
        try
        {
            var id = await service.CancelarDeudaAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
