using MediatR;
using ZuluIA_Back.Application.Features.Contratos.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class RenovarContratoCommandHandler(ContratosService service, IUnitOfWork uow) : IRequestHandler<RenovarContratoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RenovarContratoCommand request, CancellationToken ct)
    {
        try
        {
            var contrato = await service.RenovarAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(contrato.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
