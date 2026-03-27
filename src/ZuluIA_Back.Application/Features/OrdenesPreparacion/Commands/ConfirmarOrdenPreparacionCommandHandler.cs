using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class ConfirmarOrdenPreparacionCommandHandler(
    LogisticaInternaService service,
    IUnitOfWork uow)
    : IRequestHandler<ConfirmarOrdenPreparacionCommand, Result>
{
    public async Task<Result> Handle(ConfirmarOrdenPreparacionCommand request, CancellationToken ct)
    {
        try
        {
            await service.ConfirmarOrdenPreparacionAsync(request.Id, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
