using MediatR;
using ZuluIA_Back.Application.Features.Diagnosticos.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class EvaluarPlanillaDiagnosticaCommandHandler(
    DiagnosticoPlanTrabajoService service,
    IUnitOfWork uow)
    : IRequestHandler<EvaluarPlanillaDiagnosticaCommand, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(EvaluarPlanillaDiagnosticaCommand request, CancellationToken ct)
    {
        try
        {
            var resultado = await service.EvaluarAsync(request.PlanillaId, request.Respuestas, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<decimal>(ex.Message);
        }
    }
}
