using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ConsultarCtgCartaPorteCommandHandler(
    CartaPorteWorkflowService workflowService,
    IUnitOfWork uow)
    : IRequestHandler<ConsultarCtgCartaPorteCommand, Result>
{
    public async Task<Result> Handle(ConsultarCtgCartaPorteCommand request, CancellationToken ct)
    {
        try
        {
            await workflowService.ConsultarCtgAsync(
                request.CartaPorteId,
                request.FechaConsulta,
                request.NroCtg,
                request.Error,
                request.Observacion,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
