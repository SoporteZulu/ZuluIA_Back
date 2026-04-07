using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class AnularOrdenPreparacionCommandHandler(
    LogisticaInternaService service,
    IUnitOfWork uow)
    : IRequestHandler<AnularOrdenPreparacionCommand, Result>
{
    public async Task<Result> Handle(AnularOrdenPreparacionCommand request, CancellationToken ct)
    {
        try
        {
            await service.AnularOrdenPreparacionAsync(request.Id, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
