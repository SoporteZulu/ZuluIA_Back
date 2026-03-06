using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreatePlanCuentaCommandHandler(
    IPlanCuentasRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CreatePlanCuentaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreatePlanCuentaCommand request,
        CancellationToken ct)
    {
        if (await repo.ExisteCodigoAsync(
            request.EjercicioId, request.CodigoCuenta, null, ct))
            return Result.Failure<long>(
                $"Ya existe la cuenta '{request.CodigoCuenta}' en este ejercicio.");

        var cuenta = PlanCuenta.Crear(
            request.EjercicioId,
            request.IntegradoraId,
            request.CodigoCuenta,
            request.Denominacion,
            request.Nivel,
            request.OrdenNivel,
            request.Imputable,
            request.Tipo,
            request.SaldoNormal);

        await repo.AddAsync(cuenta, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cuenta.Id);
    }
}