using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class RecibirOrdenCompraCommandHandler(
    IRepository<OrdenCompraMeta> repo,
    IUnitOfWork uow)
    : IRequestHandler<RecibirOrdenCompraCommand, Result>
{
    public async Task<Result> Handle(RecibirOrdenCompraCommand request, CancellationToken ct)
    {
        var orden = await repo.GetByIdAsync(request.Id, ct);
        if (orden is null)
            return Result.Failure($"No se encontro la OC con ID {request.Id}.");

        try
        {
            orden.Recibir();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(orden);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class CancelarOrdenCompraCommandHandler(
    IRepository<OrdenCompraMeta> repo,
    IUnitOfWork uow)
    : IRequestHandler<CancelarOrdenCompraCommand, Result>
{
    public async Task<Result> Handle(CancelarOrdenCompraCommand request, CancellationToken ct)
    {
        var orden = await repo.GetByIdAsync(request.Id, ct);
        if (orden is null)
            return Result.Failure($"No se encontro la OC con ID {request.Id}.");

        try
        {
            orden.Cancelar();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(orden);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
