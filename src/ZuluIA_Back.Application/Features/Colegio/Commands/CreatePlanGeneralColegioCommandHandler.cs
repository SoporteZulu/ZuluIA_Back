using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CreatePlanGeneralColegioCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<CreatePlanGeneralColegioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePlanGeneralColegioCommand request, CancellationToken ct)
    {
        try
        {
            var plan = await service.CrearPlanGeneralAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(plan.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
