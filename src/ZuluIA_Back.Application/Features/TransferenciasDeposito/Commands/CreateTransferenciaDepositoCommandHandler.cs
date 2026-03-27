using MediatR;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class CreateTransferenciaDepositoCommandHandler(LogisticaInternaService service, IUnitOfWork uow)
    : IRequestHandler<CreateTransferenciaDepositoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTransferenciaDepositoCommand request, CancellationToken ct)
    {
        try
        {
            var transferencia = await service.CrearTransferenciaAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(transferencia.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
