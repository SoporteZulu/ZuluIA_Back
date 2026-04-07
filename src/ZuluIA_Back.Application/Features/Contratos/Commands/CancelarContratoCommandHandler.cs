using MediatR;
using ZuluIA_Back.Application.Features.Contratos.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class CancelarContratoCommandHandler(ContratosService service, IUnitOfWork uow) : IRequestHandler<CancelarContratoCommand, Result>
{
    public async Task<Result> Handle(CancelarContratoCommand request, CancellationToken ct)
    {
        try
        {
            await service.CancelarAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
