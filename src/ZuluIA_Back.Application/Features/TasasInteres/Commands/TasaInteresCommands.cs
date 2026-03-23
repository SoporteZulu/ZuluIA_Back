using FluentValidation;
using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Application.Features.TasasInteres.Commands;

// ── Crear tasa de interés ──────────────────────────────────────────────────

public record CrearTasaInteresCommand(
    string Descripcion,
    decimal TasaMensual,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    long? UserId)
    : IRequest<Result<long>>;

public class CrearTasaInteresCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CrearTasaInteresCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearTasaInteresCommand request, CancellationToken ct)
    {
        var tasa = TasaInteres.Crear(
            request.Descripcion,
            request.TasaMensual,
            request.FechaDesde,
            request.FechaHasta,
            request.UserId);
        db.TasasInteres.Add(tasa);
        await db.SaveChangesAsync(ct);
        return Result.Success(tasa.Id);
    }
}

public class CrearTasaInteresCommandValidator : AbstractValidator<CrearTasaInteresCommand>
{
    public CrearTasaInteresCommandValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TasaMensual).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.FechaDesde).NotEqual(default(DateOnly));
    }
}

// ── Desactivar tasa ────────────────────────────────────────────────────────

public record DesactivarTasaInteresCommand(long Id, long? UserId) : IRequest<Result>;

public record ActivarTasaInteresCommand(long Id, long? UserId) : IRequest<Result>;

public class DesactivarTasaInteresCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DesactivarTasaInteresCommand, Result>
{
    public async Task<Result> Handle(DesactivarTasaInteresCommand request, CancellationToken ct)
    {
        var tasa = await db.TasasInteres.FindAsync([request.Id], ct);
        if (tasa is null) return Result.Failure("Tasa de interés no encontrada.");
        tasa.Desactivar(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivarTasaInteresCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivarTasaInteresCommand, Result>
{
    public async Task<Result> Handle(ActivarTasaInteresCommand request, CancellationToken ct)
    {
        var tasa = await db.TasasInteres.FindAsync([request.Id], ct);
        if (tasa is null) return Result.Failure("Tasa de interés no encontrada.");
        tasa.Activar(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DesactivarTasaInteresCommandValidator : AbstractValidator<DesactivarTasaInteresCommand>
{
    public DesactivarTasaInteresCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivarTasaInteresCommandValidator : AbstractValidator<ActivarTasaInteresCommand>
{
    public ActivarTasaInteresCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
