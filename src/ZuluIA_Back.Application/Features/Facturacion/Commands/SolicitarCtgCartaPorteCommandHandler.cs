using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class SolicitarCtgCartaPorteCommandHandler(
    CartaPorteWorkflowService workflowService,
    IUnitOfWork uow)
    : IRequestHandler<SolicitarCtgCartaPorteCommand, Result>
{
    public async Task<Result> Handle(SolicitarCtgCartaPorteCommand request, CancellationToken ct)
    {
        try
        {
            await workflowService.SolicitarCtgAsync(
                request.CartaPorteId,
                request.FechaSolicitud,
                request.Observacion,
                request.EsReintento,
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
