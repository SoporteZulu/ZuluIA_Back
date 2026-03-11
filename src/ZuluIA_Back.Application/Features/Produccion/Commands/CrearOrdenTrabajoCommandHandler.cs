using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class CrearOrdenTrabajoCommandHandler(
    IOrdenTrabajoRepository repo,
    IFormulaProduccionRepository formulaRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CrearOrdenTrabajoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CrearOrdenTrabajoCommand request,
        CancellationToken ct)
    {
        var formula = await formulaRepo.GetByIdAsync(request.FormulaId, ct);
        if (formula is null)
            return Result.Failure<long>(
                $"No se encontró la fórmula con ID {request.FormulaId}.");

        if (!formula.Activo)
            return Result.Failure<long>(
                $"La fórmula '{formula.Codigo}' está inactiva.");

        var ot = OrdenTrabajo.Crear(
            request.SucursalId,
            request.FormulaId,
            request.DepositoOrigenId,
            request.DepositoDestinoId,
            request.Fecha,
            request.FechaFinPrevista,
            request.Cantidad,
            request.Observacion,
            currentUser.UserId);

        await repo.AddAsync(ot, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(ot.Id);
    }
}