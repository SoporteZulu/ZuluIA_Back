using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class RegistrarPickingOrdenPreparacionCommandHandler(LogisticaInternaService service, IUnitOfWork uow)
    : IRequestHandler<RegistrarPickingOrdenPreparacionCommand, Result>
{
    public async Task<Result> Handle(RegistrarPickingOrdenPreparacionCommand request, CancellationToken ct)
    {
        try
        {
            await service.RegistrarPickingAsync(request.Id, request.Detalles, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
