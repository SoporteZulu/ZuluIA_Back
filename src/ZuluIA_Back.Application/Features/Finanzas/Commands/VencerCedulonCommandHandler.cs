using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class VencerCedulonCommandHandler(
    ICedulonRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<VencerCedulonCommand, Result<string>>
{
    public async Task<Result<string>> Handle(VencerCedulonCommand request, CancellationToken ct)
    {
        var cedulon = await repo.GetByIdAsync(request.Id, ct);

        if (cedulon is null)
            return Result.Failure<string>($"No se encontro el cedulon con ID {request.Id}.");

        cedulon.Vencer(currentUser.UserId);
        repo.Update(cedulon);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cedulon.Estado.ToString().ToUpperInvariant());
    }
}