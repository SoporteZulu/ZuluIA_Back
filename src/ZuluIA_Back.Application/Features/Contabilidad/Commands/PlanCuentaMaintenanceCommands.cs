using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record UpdatePlanCuentaCommand(
    long Id,
    string Denominacion,
    bool Imputable,
    string? Tipo,
    char? SaldoNormal) : IRequest<Result>;

public record CreatePlanCuentaParametroCommand(
    long EjercicioId,
    long CuentaId,
    string Tabla,
    long IdRegistro) : IRequest<Result<long>>;

public record DeletePlanCuentaParametroCommand(long Id) : IRequest<Result>;

public class UpdatePlanCuentaCommandHandler(
    IPlanCuentasRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<UpdatePlanCuentaCommand, Result>
{
    public async Task<Result> Handle(UpdatePlanCuentaCommand request, CancellationToken ct)
    {
        var cuenta = await repo.GetByIdAsync(request.Id, ct);
        if (cuenta is null)
            return Result.Failure($"No se encontro la cuenta con ID {request.Id}.");

        try
        {
            cuenta.Actualizar(
                request.Denominacion,
                request.Imputable,
                request.Tipo,
                request.SaldoNormal);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreatePlanCuentaParametroCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreatePlanCuentaParametroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePlanCuentaParametroCommand request, CancellationToken ct)
    {
        var cuentaExists = await db.PlanCuentas
            .AnyAsync(x => x.Id == request.CuentaId && x.EjercicioId == request.EjercicioId, ct);

        if (!cuentaExists)
            return Result.Failure<long>("La cuenta indicada no existe para el ejercicio especificado.");

        PlanCuentaParametro param;
        try
        {
            param = PlanCuentaParametro.Crear(
                request.EjercicioId,
                request.CuentaId,
                request.Tabla,
                request.IdRegistro);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.PlanCuentasParametros.Add(param);
        await db.SaveChangesAsync(ct);

        return Result.Success(param.Id);
    }
}

public class DeletePlanCuentaParametroCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeletePlanCuentaParametroCommand, Result>
{
    public async Task<Result> Handle(DeletePlanCuentaParametroCommand request, CancellationToken ct)
    {
        var param = await db.PlanCuentasParametros.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
        if (param is null)
            return Result.Failure($"Parametro {request.Id} no encontrado.");

        db.PlanCuentasParametros.Remove(param);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class UpdatePlanCuentaCommandValidator : AbstractValidator<UpdatePlanCuentaCommand>
{
    public UpdatePlanCuentaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Denominacion).NotEmpty().MaximumLength(300);
    }
}

public class CreatePlanCuentaParametroCommandValidator : AbstractValidator<CreatePlanCuentaParametroCommand>
{
    public CreatePlanCuentaParametroCommandValidator()
    {
        RuleFor(x => x.EjercicioId).GreaterThan(0);
        RuleFor(x => x.CuentaId).GreaterThan(0);
        RuleFor(x => x.Tabla).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IdRegistro).GreaterThan(0);
    }
}

public class DeletePlanCuentaParametroCommandValidator : AbstractValidator<DeletePlanCuentaParametroCommand>
{
    public DeletePlanCuentaParametroCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
