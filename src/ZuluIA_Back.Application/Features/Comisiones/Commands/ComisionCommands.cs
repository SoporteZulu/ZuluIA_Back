using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Application.Features.Comisiones.Commands;

// ── Registrar comisión de un vendedor ──────────────────────────────────────

public record RegistrarComisionVendedorCommand(
    long SucursalId,
    long VendedorId,
    int Periodo,
    decimal MontoBase,
    decimal PorcentajeComision,
    string? Observacion,
    long? UserId)
    : IRequest<Result<long>>;

public class RegistrarComisionVendedorCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegistrarComisionVendedorCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarComisionVendedorCommand request, CancellationToken ct)
    {
        var yaExiste = await db.ComisionesVendedor
            .AnyAsync(c => c.SucursalId == request.SucursalId
                        && c.VendedorId == request.VendedorId
                        && c.Periodo == request.Periodo
                        && c.DeletedAt == null, ct);
        if (yaExiste)
            return Result.Failure<long>("Ya existe una comisión para este vendedor en el período indicado.");

        var comision = ComisionVendedor.Crear(
            request.SucursalId,
            request.VendedorId,
            request.Periodo,
            request.MontoBase,
            request.PorcentajeComision,
            request.Observacion,
            request.UserId);
        db.ComisionesVendedor.Add(comision);
        await db.SaveChangesAsync(ct);
        return Result.Success(comision.Id);
    }
}

public class RegistrarComisionVendedorCommandValidator : AbstractValidator<RegistrarComisionVendedorCommand>
{
    public RegistrarComisionVendedorCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.VendedorId).GreaterThan(0);
        RuleFor(x => x.Periodo).InclusiveBetween(190001, 209912)
            .WithMessage("El periodo debe tener formato YYYYMM válido.");
        RuleFor(x => x.MontoBase).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PorcentajeComision).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

// ── Aprobar comisión ────────────────────────────────────────────────────────

public record AprobarComisionCommand(long Id, long? UserId) : IRequest<Result>;

public class AprobarComisionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AprobarComisionCommand, Result>
{
    public async Task<Result> Handle(AprobarComisionCommand request, CancellationToken ct)
    {
        var comision = await db.ComisionesVendedor.FindAsync([request.Id], ct);
        if (comision is null) return Result.Failure("Comisión no encontrada.");
        comision.Aprobar(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Anular comisión ─────────────────────────────────────────────────────────

public record AnularComisionCommand(long Id, long? UserId) : IRequest<Result>;

public class AnularComisionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AnularComisionCommand, Result>
{
    public async Task<Result> Handle(AnularComisionCommand request, CancellationToken ct)
    {
        var comision = await db.ComisionesVendedor.FindAsync([request.Id], ct);
        if (comision is null) return Result.Failure("Comisión no encontrada.");
        comision.Anular(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
