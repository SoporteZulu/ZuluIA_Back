using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class UpdatePlanGeneralColegioCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<UpdatePlanGeneralColegioCommand, Result>
{
    public async Task<Result> Handle(UpdatePlanGeneralColegioCommand request, CancellationToken ct)
    {
        try
        {
            await service.ActualizarPlanGeneralAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
