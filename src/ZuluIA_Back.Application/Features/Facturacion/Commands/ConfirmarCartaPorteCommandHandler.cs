using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ConfirmarCartaPorteCommandHandler(
    CartaPorteWorkflowService workflowService,
    IUnitOfWork uow)
    : IRequestHandler<ConfirmarCartaPorteCommand, Result>
{
    public async Task<Result> Handle(ConfirmarCartaPorteCommand request, CancellationToken ct)
    {
        try
        {
            await workflowService.ConfirmarAsync(request.CartaPorteId, request.Fecha, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
