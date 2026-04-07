using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class IniciarOrdenPreparacionCommandHandler(LogisticaInternaService service, IUnitOfWork uow)
    : IRequestHandler<IniciarOrdenPreparacionCommand, Result>
{
    public async Task<Result> Handle(IniciarOrdenPreparacionCommand request, CancellationToken ct)
    {
        try
        {
            await service.IniciarOrdenPreparacionAsync(request.Id, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
