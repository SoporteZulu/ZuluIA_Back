using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateCedulonCommandHandler(
    ICedulonRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCedulonCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCedulonCommand request, CancellationToken ct)
    {
        Cedulon cedulon;
        try
        {
            cedulon = Cedulon.Crear(
                request.TerceroId,
                request.SucursalId,
                request.PlanPagoId,
                request.NroCedulon,
                request.FechaEmision,
                request.FechaVencimiento,
                request.Importe,
                currentUser.UserId);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(cedulon, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cedulon.Id);
    }
}
