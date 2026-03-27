using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class AnularTransferenciaDepositoCommandHandler(LogisticaInternaService service, IUnitOfWork uow)
    : IRequestHandler<AnularTransferenciaDepositoCommand, Result>
{
    public async Task<Result> Handle(AnularTransferenciaDepositoCommand request, CancellationToken ct)
    {
        try
        {
            await service.AnularTransferenciaAsync(request.Id, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
