using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class EmitirComprobanteCommandHandler(
    IComprobanteRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<EmitirComprobanteCommand, Result>
{
    public async Task<Result> Handle(EmitirComprobanteCommand request, CancellationToken ct)
    {
        var comprobante = await repo.GetByIdAsync(request.Id, ct);

        if (comprobante is null)
            return Result.Failure($"No se encontró el comprobante con ID {request.Id}.");

        try
        {
            comprobante.Emitir(request.Cae, request.FechaVtoCae, currentUser.UserId);
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