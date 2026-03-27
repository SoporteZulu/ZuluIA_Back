using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record CreatePeriodoContableCommand(
    string Periodo,
    DateOnly FechaInicio,
    DateOnly FechaFin) : IRequest<Result<long>>;

public record CerrarPeriodoContableCommand(long Id) : IRequest<Result<PeriodoContableEstadoResult>>;

public record AbrirPeriodoContableCommand(long Id) : IRequest<Result<PeriodoContableEstadoResult>>;

public record PeriodoContableEstadoResult(long Id, string Periodo, bool Abierto);

public class CreatePeriodoContableCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreatePeriodoContableCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePeriodoContableCommand request, CancellationToken ct)
    {
        var periodo = request.Periodo.Trim();
        if (await db.PeriodosContables.AnyAsync(x => x.Periodo == periodo, ct))
            return Result.Failure<long>($"Ya existe un período contable '{request.Periodo}'.");

        PeriodoContable entity;
        try
        {
            entity = PeriodoContable.Crear(periodo, request.FechaInicio, request.FechaFin);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.PeriodosContables.Add(entity);
        await db.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}

public class CerrarPeriodoContableCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CerrarPeriodoContableCommand, Result<PeriodoContableEstadoResult>>
{
    public async Task<Result<PeriodoContableEstadoResult>> Handle(CerrarPeriodoContableCommand request, CancellationToken ct)
    {
        var entity = await db.PeriodosContables.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<PeriodoContableEstadoResult>($"Período contable {request.Id} no encontrado.");

        try
        {
            entity.Cerrar();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<PeriodoContableEstadoResult>(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success(new PeriodoContableEstadoResult(entity.Id, entity.Periodo, entity.Abierto));
    }
}

public class AbrirPeriodoContableCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AbrirPeriodoContableCommand, Result<PeriodoContableEstadoResult>>
{
    public async Task<Result<PeriodoContableEstadoResult>> Handle(AbrirPeriodoContableCommand request, CancellationToken ct)
    {
        var entity = await db.PeriodosContables.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<PeriodoContableEstadoResult>($"Período contable {request.Id} no encontrado.");

        try
        {
            entity.Abrir();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<PeriodoContableEstadoResult>(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success(new PeriodoContableEstadoResult(entity.Id, entity.Periodo, entity.Abierto));
    }
}

public class CreatePeriodoContableCommandValidator : AbstractValidator<CreatePeriodoContableCommand>
{
    public CreatePeriodoContableCommandValidator()
    {
        RuleFor(x => x.Periodo).NotEmpty();
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
    }
}

public class CerrarPeriodoContableCommandValidator : AbstractValidator<CerrarPeriodoContableCommand>
{
    public CerrarPeriodoContableCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AbrirPeriodoContableCommandValidator : AbstractValidator<AbrirPeriodoContableCommand>
{
    public AbrirPeriodoContableCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
