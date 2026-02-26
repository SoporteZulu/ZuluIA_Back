using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class AnularComprobanteCommandHandler(
    IComprobanteRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularComprobanteCommand, Result>
{
    public async Task<Result> Handle(AnularComprobanteCommand request, CancellationToken ct)
    {
        var comprobante = await repo.GetByIdAsync(request.Id, ct);

        if (comprobante is null)
            return Result.Failure($"No se encontró el comprobante con ID {request.Id}.");

        try
        {
            comprobante.Anular(currentUser.UserId);
            repo.Update(comprobante);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}