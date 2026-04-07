using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class EnviarRequisicionCompraCommandHandler(
    IRequisicionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<EnviarRequisicionCompraCommand, Result>
{
    public async Task<Result> Handle(EnviarRequisicionCompraCommand request, CancellationToken ct)
    {
        var req = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (req is null) return Result.Failure("Requisición no encontrada.");
        req.Enviar(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AprobarRequisicionCompraCommandHandler(
    IRequisicionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AprobarRequisicionCompraCommand, Result>
{
    public async Task<Result> Handle(AprobarRequisicionCompraCommand request, CancellationToken ct)
    {
        var req = await repo.GetByIdAsync(request.Id, ct);
        if (req is null) return Result.Failure("Requisición no encontrada.");
        req.Aprobar(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class RechazarRequisicionCompraCommandHandler(
    IRequisicionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<RechazarRequisicionCompraCommand, Result>
{
    public async Task<Result> Handle(RechazarRequisicionCompraCommand request, CancellationToken ct)
    {
        var req = await repo.GetByIdAsync(request.Id, ct);
        if (req is null) return Result.Failure("Requisición no encontrada.");
        req.Rechazar(request.Motivo, currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CancelarRequisicionCompraCommandHandler(
    IRequisicionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CancelarRequisicionCompraCommand, Result>
{
    public async Task<Result> Handle(CancelarRequisicionCompraCommand request, CancellationToken ct)
    {
        var req = await repo.GetByIdAsync(request.Id, ct);
        if (req is null) return Result.Failure("Requisición no encontrada.");
        req.Cancelar(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
