using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class DespacharOrdenPreparacionCommandHandler(LogisticaInternaService service, IUnitOfWork uow)
    : IRequestHandler<DespacharOrdenPreparacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(DespacharOrdenPreparacionCommand request, CancellationToken ct)
    {
        try
        {
            var transferencia = await service.DespacharOrdenPreparacionAsync(request.Id, request.DepositoDestinoId, request.Fecha, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(transferencia.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
