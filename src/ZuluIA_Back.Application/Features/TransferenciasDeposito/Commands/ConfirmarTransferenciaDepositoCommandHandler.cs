using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class ConfirmarTransferenciaDepositoCommandHandler(LogisticaInternaService service, IUnitOfWork uow)
    : IRequestHandler<ConfirmarTransferenciaDepositoCommand, Result>
{
    public async Task<Result> Handle(ConfirmarTransferenciaDepositoCommand request, CancellationToken ct)
    {
        try
        {
            await service.ConfirmarTransferenciaAsync(request.Id, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
