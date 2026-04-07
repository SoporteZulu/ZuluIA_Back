using MediatR;
using ZuluIA_Back.Application.Features.Contratos.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class RenovarContratosAutomaticamenteCommandHandler(ContratosService service, IUnitOfWork uow) : IRequestHandler<RenovarContratosAutomaticamenteCommand, Result<int>>
{
    public async Task<Result<int>> Handle(RenovarContratosAutomaticamenteCommand request, CancellationToken ct)
    {
        try
        {
            var total = await service.RenovarAutomaticamenteAsync(request.SucursalId, request.FechaCorte, request.PorcentajeAjuste, request.GenerarImpactoComercial, request.GenerarImpactoFinanciero, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(total);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }
}
