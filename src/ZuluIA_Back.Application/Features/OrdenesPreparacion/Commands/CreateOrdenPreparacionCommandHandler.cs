using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class CreateOrdenPreparacionCommandHandler(
    LogisticaInternaService service,
    IUnitOfWork uow)
    : IRequestHandler<CreateOrdenPreparacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateOrdenPreparacionCommand request,
        CancellationToken ct)
    {
        try
        {
            var orden = await service.CrearOrdenPreparacionAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(orden.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
