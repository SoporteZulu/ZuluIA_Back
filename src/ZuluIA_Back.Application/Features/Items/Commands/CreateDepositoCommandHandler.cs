using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateDepositoCommandHandler(
    IDepositoRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateDepositoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateDepositoCommand request,
        CancellationToken ct)
    {
        // Si es default, quitar el default actual
        if (request.EsDefault)
        {
            var defaultActual = await repo.GetDefaultBySucursalAsync(
                request.SucursalId, ct);

            if (defaultActual is not null)
            {
                defaultActual.UnsetDefault();
                repo.Update(defaultActual);
            }
        }

        var deposito = Deposito.Crear(
            request.SucursalId,
            request.Descripcion,
            request.EsDefault);

        await repo.AddAsync(deposito, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(deposito.Id);
    }
}