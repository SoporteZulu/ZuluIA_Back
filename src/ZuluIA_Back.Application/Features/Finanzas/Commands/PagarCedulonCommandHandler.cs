using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class PagarCedulonCommandHandler(
    ICedulonRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<PagarCedulonCommand, Result<PagarCedulonResult>>
{
    public async Task<Result<PagarCedulonResult>> Handle(PagarCedulonCommand request, CancellationToken ct)
    {
        var cedulon = await repo.GetByIdAsync(request.Id, ct);

        if (cedulon is null)
            return Result.Failure<PagarCedulonResult>($"No se encontro el cedulon con ID {request.Id}.");

        cedulon.RegistrarPago(request.Importe, currentUser.UserId);
        repo.Update(cedulon);
        await uow.SaveChangesAsync(ct);

        var result = new PagarCedulonResult(
            cedulon.ImportePagado,
            cedulon.Importe - cedulon.ImportePagado,
            cedulon.Estado.ToString().ToUpperInvariant());

        return Result.Success(result);
    }
}