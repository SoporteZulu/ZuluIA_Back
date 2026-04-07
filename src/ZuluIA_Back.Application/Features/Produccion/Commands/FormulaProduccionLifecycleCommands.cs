using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record UpdateFormulaProduccionCommand(
    long Id,
    string Descripcion,
    decimal CantidadResultado,
    string? Observacion) : IRequest<Result>;

public record DeactivateFormulaProduccionCommand(long Id) : IRequest<Result>;

public record ActivateFormulaProduccionCommand(long Id) : IRequest<Result>;

public class UpdateFormulaProduccionCommandHandler(
    IFormulaProduccionRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateFormulaProduccionCommand, Result>
{
    public async Task<Result> Handle(UpdateFormulaProduccionCommand request, CancellationToken ct)
    {
        var formula = await repo.GetByIdAsync(request.Id, ct);
        if (formula is null)
            return Result.Failure($"No se encontró la fórmula con ID {request.Id}.");

        try
        {
            formula.Actualizar(request.Descripcion, request.CantidadResultado, request.Observacion, currentUser.UserId);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(formula);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeactivateFormulaProduccionCommandHandler(
    IFormulaProduccionRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DeactivateFormulaProduccionCommand, Result>
{
    public async Task<Result> Handle(DeactivateFormulaProduccionCommand request, CancellationToken ct)
    {
        var formula = await repo.GetByIdAsync(request.Id, ct);
        if (formula is null)
            return Result.Failure($"No se encontró la fórmula con ID {request.Id}.");

        formula.Desactivar(currentUser.UserId);
        repo.Update(formula);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class ActivateFormulaProduccionCommandHandler(
    IFormulaProduccionRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivateFormulaProduccionCommand, Result>
{
    public async Task<Result> Handle(ActivateFormulaProduccionCommand request, CancellationToken ct)
    {
        var formula = await repo.GetByIdAsync(request.Id, ct);
        if (formula is null)
            return Result.Failure($"No se encontró la fórmula con ID {request.Id}.");

        formula.Activar(currentUser.UserId);
        repo.Update(formula);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
