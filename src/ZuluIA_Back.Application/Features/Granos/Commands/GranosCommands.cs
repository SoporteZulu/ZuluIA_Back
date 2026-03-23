using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Agro;

namespace ZuluIA_Back.Application.Features.Granos.Commands;

// ── Crear liquidación ──────────────────────────────────────────────────────

public record CrearLiquidacionGranosCommand(
    long SucursalId,
    long TerceroId,
    string Producto,
    DateOnly Fecha,
    decimal Cantidad,
    decimal PrecioBase,
    long? UserId)
    : IRequest<Result<long>>;

public class CrearLiquidacionGranosCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CrearLiquidacionGranosCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearLiquidacionGranosCommand request, CancellationToken ct)
    {
        var liq = LiquidacionGranos.Crear(
            request.SucursalId, request.TerceroId, request.Producto,
            request.Fecha, request.Cantidad, request.PrecioBase, request.UserId);
        db.LiquidacionesGranos.Add(liq);
        await db.SaveChangesAsync(ct);
        return Result.Success(liq.Id);
    }
}

public class CrearLiquidacionGranosCommandValidator : AbstractValidator<CrearLiquidacionGranosCommand>
{
    public CrearLiquidacionGranosCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Producto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.PrecioBase).GreaterThan(0);
    }
}

// ── Agregar concepto ───────────────────────────────────────────────────────

public record AgregarConceptoLiquidacionCommand(
    long LiquidacionId,
    string Concepto,
    decimal Importe,
    bool EsDeduccion,
    long? UserId)
    : IRequest<Result>;

public class AgregarConceptoLiquidacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AgregarConceptoLiquidacionCommand, Result>
{
    public async Task<Result> Handle(AgregarConceptoLiquidacionCommand request, CancellationToken ct)
    {
        var liq = await db.LiquidacionesGranos
            .Include(l => l.Conceptos)
            .FirstOrDefaultAsync(l => l.Id == request.LiquidacionId, ct);
        if (liq is null) return Result.Failure("Liquidación no encontrada.");

        var concepto = LiquidacionGranosConcepto.Crear(
            request.LiquidacionId, request.Concepto, request.Importe, request.EsDeduccion);
        liq.AgregarConcepto(concepto);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Agregar certificación ──────────────────────────────────────────────────

public record AgregarCertificacionCommand(
    long LiquidacionId,
    string NroCertificado,
    DateOnly FechaEmision,
    decimal PesoNeto,
    decimal Humedad,
    decimal Impureza,
    string? CalidadObservaciones,
    long? UserId)
    : IRequest<Result<long>>;

public class AgregarCertificacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AgregarCertificacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AgregarCertificacionCommand request, CancellationToken ct)
    {
        var existe = await db.LiquidacionesGranos
            .AnyAsync(l => l.Id == request.LiquidacionId && l.DeletedAt == null, ct);
        if (!existe) return Result.Failure<long>("Liquidación no encontrada.");

        var cert = CertificacionGranos.Crear(
            request.LiquidacionId, request.NroCertificado, request.FechaEmision,
            request.PesoNeto, request.Humedad, request.Impureza,
            request.CalidadObservaciones, request.UserId);
        db.CertificacionesGranos.Add(cert);
        await db.SaveChangesAsync(ct);
        return Result.Success(cert.Id);
    }
}

// ── Emitir liquidación ─────────────────────────────────────────────────────

public record EmitirLiquidacionGranosCommand(long Id, long? UserId) : IRequest<Result>;

public class EmitirLiquidacionGranosCommandHandler(IApplicationDbContext db)
    : IRequestHandler<EmitirLiquidacionGranosCommand, Result>
{
    public async Task<Result> Handle(EmitirLiquidacionGranosCommand request, CancellationToken ct)
    {
        var liq = await db.LiquidacionesGranos.FindAsync([request.Id], ct);
        if (liq is null) return Result.Failure("Liquidación no encontrada.");
        liq.Emitir(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Anular liquidación ─────────────────────────────────────────────────────

public record AnularLiquidacionGranosCommand(long Id, long? UserId) : IRequest<Result>;

public class AnularLiquidacionGranosCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AnularLiquidacionGranosCommand, Result>
{
    public async Task<Result> Handle(AnularLiquidacionGranosCommand request, CancellationToken ct)
    {
        var liq = await db.LiquidacionesGranos.FindAsync([request.Id], ct);
        if (liq is null) return Result.Failure("Liquidación no encontrada.");
        liq.Anular(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
