using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public sealed record CreateConteoCiclicoCommand(
    string Deposito,
    string Zona,
    string Frecuencia,
    DateOnly ProximoConteo,
    string Estado,
    decimal DivergenciaPct,
    string? Responsable,
    string? Observacion,
    string? NextStep,
    string? ExecutionNote) : IRequest<Result<long>>;

public sealed record UpdateConteoCiclicoCommand(
    long Id,
    string Deposito,
    string Zona,
    string Frecuencia,
    DateOnly ProximoConteo,
    string Estado,
    decimal DivergenciaPct,
    string? Responsable,
    string? Observacion,
    string? NextStep,
    string? ExecutionNote) : IRequest<Result>;

public sealed record DeleteConteoCiclicoCommand(long Id) : IRequest<Result>;
public sealed record SeedConteosCiclicosCommand() : IRequest<Result<int>>;

public class CreateConteoCiclicoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateConteoCiclicoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateConteoCiclicoCommand request, CancellationToken ct)
    {
        ConteoCiclicoPlan entity;
        try
        {
            entity = ConteoCiclicoPlan.Crear(
                request.Deposito,
                request.Zona,
                request.Frecuencia,
                request.ProximoConteo,
                request.Estado,
                request.DivergenciaPct,
                request.Responsable,
                request.Observacion,
                request.NextStep,
                request.ExecutionNote);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.ConteosCiclicos.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateConteoCiclicoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateConteoCiclicoCommand, Result>
{
    public async Task<Result> Handle(UpdateConteoCiclicoCommand request, CancellationToken ct)
    {
        var entity = await db.ConteosCiclicos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"No se encontró el conteo con ID {request.Id}.");

        try
        {
            entity.Actualizar(
                request.Deposito,
                request.Zona,
                request.Frecuencia,
                request.ProximoConteo,
                request.Estado,
                request.DivergenciaPct,
                request.Responsable,
                request.Observacion,
                request.NextStep,
                request.ExecutionNote);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class SeedConteosCiclicosCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SeedConteosCiclicosCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SeedConteosCiclicosCommand request, CancellationToken ct)
    {
        var defaults = new[]
        {
            ConteoCiclicoPlan.Crear(
                "Central",
                "A1-PICK",
                "Semanal",
                new DateOnly(2026, 3, 26),
                "programado",
                1.8m,
                "Equipo Picking AM",
                "Conteo corto de alta rotación previo al cierre semanal.",
                "Preparar equipo y validar cobertura antes del próximo conteo.",
                string.Empty),
            ConteoCiclicoPlan.Crear(
                "Planta Norte",
                "B1-MP",
                "Quincenal",
                new DateOnly(2026, 3, 24),
                "en-ejecucion",
                3.6m,
                "Control de planta",
                "Revisión focalizada sobre materia prima crítica y merma visible.",
                "Preparar equipo y validar cobertura antes del próximo conteo.",
                string.Empty),
            ConteoCiclicoPlan.Crear(
                "Obra Delta",
                "C1-OBR",
                "Mensual",
                new DateOnly(2026, 3, 29),
                "observado",
                6.2m,
                "Capataz de obra",
                "Requiere conciliación entre remitos consumidos y stock remanente.",
                "Conciliar diferencias y definir ajuste manual o re-conteo.",
                string.Empty)
        };

        var existentes = await db.ConteosCiclicos
            .AsNoTracking()
            .Select(x => new { x.Deposito, x.Zona })
            .ToListAsync(ct);

        var existentesKeys = existentes
            .Select(x => $"{x.Deposito}|{x.Zona}".ToUpperInvariant())
            .ToHashSet();

        var nuevos = defaults
            .Where(x => !existentesKeys.Contains($"{x.Deposito}|{x.Zona}".ToUpperInvariant()))
            .ToList();

        if (nuevos.Count == 0)
            return Result.Success(0);

        db.ConteosCiclicos.AddRange(nuevos);
        await db.SaveChangesAsync(ct);
        return Result.Success(nuevos.Count);
    }
}

public class DeleteConteoCiclicoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteConteoCiclicoCommand, Result>
{
    public async Task<Result> Handle(DeleteConteoCiclicoCommand request, CancellationToken ct)
    {
        var entity = await db.ConteosCiclicos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"No se encontró el conteo con ID {request.Id}.");

        db.ConteosCiclicos.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateConteoCiclicoCommandValidator : AbstractValidator<CreateConteoCiclicoCommand>
{
    public CreateConteoCiclicoCommandValidator()
    {
        RuleFor(x => x.Deposito).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Zona).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Frecuencia).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Estado).Must(BeValidState).WithMessage("El estado del conteo no es válido.");
        RuleFor(x => x.DivergenciaPct).GreaterThanOrEqualTo(0);
    }

    private static bool BeValidState(string estado)
        => estado is "programado" or "en-ejecucion" or "observado";
}

public class UpdateConteoCiclicoCommandValidator : AbstractValidator<UpdateConteoCiclicoCommand>
{
    public UpdateConteoCiclicoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Deposito).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Zona).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Frecuencia).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Estado).Must(BeValidState).WithMessage("El estado del conteo no es válido.");
        RuleFor(x => x.DivergenciaPct).GreaterThanOrEqualTo(0);
    }

    private static bool BeValidState(string estado)
        => estado is "programado" or "en-ejecucion" or "observado";
}
