using MediatR;
using ZuluIA_Back.Application.Features.Contratos.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class RegistrarImpactoContratoCommandHandler(ContratosService service, IUnitOfWork uow) : IRequestHandler<RegistrarImpactoContratoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarImpactoContratoCommand request, CancellationToken ct)
    {
        try
        {
            var impacto = await service.RegistrarImpactoManualAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(impacto.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
