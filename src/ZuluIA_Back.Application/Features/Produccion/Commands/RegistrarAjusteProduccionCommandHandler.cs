using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class RegistrarAjusteProduccionCommandHandler(
    ProduccionService produccionService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<RegistrarAjusteProduccionCommand, Result>
{
    public async Task<Result> Handle(RegistrarAjusteProduccionCommand request, CancellationToken ct)
    {
        try
        {
            await produccionService.AjustarSegunFormulaAsync(
                request.FormulaId,
                request.DepositoOrigenId,
                request.DepositoDestinoId,
                request.Cantidad,
                request.Observacion,
                currentUser.UserId,
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
