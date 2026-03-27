using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class AnularCartaPorteWorkflowCommandHandler(
    CartaPorteWorkflowService workflowService,
    IUnitOfWork uow)
    : IRequestHandler<AnularCartaPorteWorkflowCommand, Result>
{
    public async Task<Result> Handle(AnularCartaPorteWorkflowCommand request, CancellationToken ct)
    {
        try
        {
            await workflowService.AnularAsync(request.CartaPorteId, request.Fecha, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
