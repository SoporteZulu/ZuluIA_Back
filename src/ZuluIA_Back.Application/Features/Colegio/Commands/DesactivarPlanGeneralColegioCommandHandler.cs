using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class DesactivarPlanGeneralColegioCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<DesactivarPlanGeneralColegioCommand, Result>
{
    public async Task<Result> Handle(DesactivarPlanGeneralColegioCommand request, CancellationToken ct)
    {
        try
        {
            await service.DesactivarPlanGeneralAsync(request.Id, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
