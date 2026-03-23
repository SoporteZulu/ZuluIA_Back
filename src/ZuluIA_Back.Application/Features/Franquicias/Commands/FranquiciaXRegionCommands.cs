using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Franquicias;

namespace ZuluIA_Back.Application.Features.Franquicias.Commands;

public record CreateFranquiciaXRegionCommand(
    long SucursalId,
    long RegionId,
    long? GrupoEconomicoId = null) : IRequest<Result<long>>;

public record UpdateFranquiciaXRegionCommand(
    long Id,
    long SucursalId,
    long RegionId,
    long? GrupoEconomicoId = null) : IRequest<Result>;

public record DeleteFranquiciaXRegionCommand(long Id) : IRequest<Result>;

public class CreateFranquiciaXRegionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateFranquiciaXRegionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateFranquiciaXRegionCommand request, CancellationToken ct)
    {
        if (!await db.Sucursales.AsNoTracking().AnyAsync(x => x.Id == request.SucursalId, ct))
            return Result.Failure<long>($"Sucursal {request.SucursalId} no encontrada.");

        if (!await db.Regiones.AsNoTracking().AnyAsync(x => x.Id == request.RegionId, ct))
            return Result.Failure<long>($"Region {request.RegionId} no encontrada.");

        if (request.GrupoEconomicoId.HasValue
            && !await db.GrupoEconomicos.AsNoTracking().AnyAsync(x => x.Id == request.GrupoEconomicoId.Value, ct))
            return Result.Failure<long>($"Grupo economico {request.GrupoEconomicoId.Value} no encontrado.");

        var exists = await db.FranquiciasXRegiones.AsNoTracking()
            .AnyAsync(x => x.SucursalId == request.SucursalId && x.RegionId == request.RegionId, ct);
        if (exists)
            return Result.Failure<long>("Ya existe una asignacion para esa sucursal y region.");

        var entity = FranquiciaXRegion.Crear(request.SucursalId, request.RegionId, request.GrupoEconomicoId);
        db.FranquiciasXRegiones.Add(entity);
        await db.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}

public class UpdateFranquiciaXRegionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateFranquiciaXRegionCommand, Result>
{
    public async Task<Result> Handle(UpdateFranquiciaXRegionCommand request, CancellationToken ct)
    {
        var entity = await db.FranquiciasXRegiones.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Asignacion franquicia-region {request.Id} no encontrada.");

        if (!await db.Sucursales.AsNoTracking().AnyAsync(x => x.Id == request.SucursalId, ct))
            return Result.Failure($"Sucursal {request.SucursalId} no encontrada.");

        if (!await db.Regiones.AsNoTracking().AnyAsync(x => x.Id == request.RegionId, ct))
            return Result.Failure($"Region {request.RegionId} no encontrada.");

        if (request.GrupoEconomicoId.HasValue
            && !await db.GrupoEconomicos.AsNoTracking().AnyAsync(x => x.Id == request.GrupoEconomicoId.Value, ct))
            return Result.Failure($"Grupo economico {request.GrupoEconomicoId.Value} no encontrado.");

        var exists = await db.FranquiciasXRegiones.AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.SucursalId == request.SucursalId && x.RegionId == request.RegionId, ct);
        if (exists)
            return Result.Failure("Ya existe una asignacion para esa sucursal y region.");

        entity.Actualizar(request.SucursalId, request.RegionId, request.GrupoEconomicoId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteFranquiciaXRegionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteFranquiciaXRegionCommand, Result>
{
    public async Task<Result> Handle(DeleteFranquiciaXRegionCommand request, CancellationToken ct)
    {
        var entity = await db.FranquiciasXRegiones.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Asignacion franquicia-region {request.Id} no encontrada.");

        db.FranquiciasXRegiones.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateFranquiciaXRegionCommandValidator : AbstractValidator<CreateFranquiciaXRegionCommand>
{
    public CreateFranquiciaXRegionCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.RegionId).GreaterThan(0);
    }
}

public class UpdateFranquiciaXRegionCommandValidator : AbstractValidator<UpdateFranquiciaXRegionCommand>
{
    public UpdateFranquiciaXRegionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.RegionId).GreaterThan(0);
    }
}

public class DeleteFranquiciaXRegionCommandValidator : AbstractValidator<DeleteFranquiciaXRegionCommand>
{
    public DeleteFranquiciaXRegionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}