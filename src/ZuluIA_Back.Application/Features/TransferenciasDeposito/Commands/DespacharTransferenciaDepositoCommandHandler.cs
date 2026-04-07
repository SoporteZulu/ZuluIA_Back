using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class DespacharTransferenciaDepositoCommandHandler(LogisticaInternaService service, IUnitOfWork uow)
    : IRequestHandler<DespacharTransferenciaDepositoCommand, Result>
{
    public async Task<Result> Handle(DespacharTransferenciaDepositoCommand request, CancellationToken ct)
    {
        try
        {
            await service.DespacharTransferenciaAsync(request.Id, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
