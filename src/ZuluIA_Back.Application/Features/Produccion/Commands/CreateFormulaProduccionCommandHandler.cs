using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class CreateFormulaProduccionCommandHandler(
    IFormulaProduccionRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateFormulaProduccionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateFormulaProduccionCommand request,
        CancellationToken ct)
    {
        if (await repo.ExisteCodigoAsync(request.Codigo, null, ct))
            return Result.Failure<long>(
                $"Ya existe una fórmula con código '{request.Codigo}'.");

        if (!request.Ingredientes.Any())
            return Result.Failure<long>(
                "La fórmula debe tener al menos un ingrediente.");

        var formula = FormulaProduccion.Crear(
            request.Codigo,
            request.Descripcion,
            request.ItemResultadoId,
            request.CantidadResultado,
            request.UnidadMedidaId,
            request.Observacion,
            currentUser.UserId);

        foreach (var ing in request.Ingredientes)
        {
            formula.AgregarIngrediente(FormulaIngrediente.Crear(
                0,
                ing.ItemId,
                ing.Cantidad,
                ing.UnidadMedidaId,
                ing.EsOpcional,
                ing.Orden));
        }

        await repo.AddAsync(formula, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(formula.Id);
    }
}