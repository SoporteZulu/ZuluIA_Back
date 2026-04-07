using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record EgresarEmpleadoCommand(long EmpleadoId, DateOnly FechaEgreso) : IRequest<Result>;

public class EgresarEmpleadoCommandHandler(IEmpleadoRepository repo, IUnitOfWork uow)
    : IRequestHandler<EgresarEmpleadoCommand, Result>
{
    public async Task<Result> Handle(EgresarEmpleadoCommand request, CancellationToken ct)
    {
        var empleado = await repo.GetByIdAsync(request.EmpleadoId, ct);
        if (empleado is null)
            return Result.Failure($"No se encontro el empleado con ID {request.EmpleadoId}.");

        empleado.Egresar(request.FechaEgreso);
        repo.Update(empleado);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record MarcarLiquidacionPagadaCommand(long EmpleadoId, long LiquidacionId) : IRequest<Result>;

public class MarcarLiquidacionPagadaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<MarcarLiquidacionPagadaCommand, Result>
{
    public async Task<Result> Handle(MarcarLiquidacionPagadaCommand request, CancellationToken ct)
    {
        var liquidacion = await db.LiquidacionesSueldo
            .FirstOrDefaultAsync(x => x.Id == request.LiquidacionId && x.EmpleadoId == request.EmpleadoId, ct);

        if (liquidacion is null)
            return Result.Failure($"No se encontro la liquidacion {request.LiquidacionId} para el empleado {request.EmpleadoId}.");

        if (liquidacion.Pagada)
            return Result.Failure("La liquidacion ya se encuentra marcada como pagada.");

        liquidacion.MarcarComoPagada();
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddEmpleadoAreaCommand(long EmpleadoId, long AreaId, int Orden) : IRequest<Result<long>>;

public class AddEmpleadoAreaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddEmpleadoAreaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddEmpleadoAreaCommand request, CancellationToken ct)
    {
        var empleado = await db.Empleados.FindAsync([request.EmpleadoId], ct);
        if (empleado is null)
            return Result.Failure<long>($"Empleado {request.EmpleadoId} no encontrado.");

        var yaAsignada = await db.EmpleadosXArea
            .AnyAsync(x => x.EmpleadoId == request.EmpleadoId && x.AreaId == request.AreaId, ct);

        if (yaAsignada)
            return Result.Failure<long>("El empleado ya esta asignado a esa area.");

        EmpleadoXArea asignacion;
        try
        {
            asignacion = EmpleadoXArea.Crear(request.EmpleadoId, request.AreaId, request.Orden);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.EmpleadosXArea.Add(asignacion);
        await db.SaveChangesAsync(ct);

        return Result.Success(asignacion.Id);
    }
}

public record RemoveEmpleadoAreaCommand(long EmpleadoId, long EmpleadoAreaId) : IRequest<Result>;

public class RemoveEmpleadoAreaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RemoveEmpleadoAreaCommand, Result>
{
    public async Task<Result> Handle(RemoveEmpleadoAreaCommand request, CancellationToken ct)
    {
        var asignacion = await db.EmpleadosXArea
            .FirstOrDefaultAsync(x => x.EmpleadoId == request.EmpleadoId && x.Id == request.EmpleadoAreaId, ct);

        if (asignacion is null)
            return Result.Failure("Asignacion no encontrada.");

        var perfiles = db.EmpleadosXPerfil.Where(p => p.EmpleadoXAreaId == request.EmpleadoAreaId);
        db.EmpleadosXPerfil.RemoveRange(perfiles);

        db.EmpleadosXArea.Remove(asignacion);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddEmpleadoPerfilCommand(long EmpleadoId, long EmpleadoAreaId, long PerfilId, int Orden)
    : IRequest<Result<long>>;

public class AddEmpleadoPerfilCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddEmpleadoPerfilCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddEmpleadoPerfilCommand request, CancellationToken ct)
    {
        var asignacion = await db.EmpleadosXArea
            .FirstOrDefaultAsync(x => x.EmpleadoId == request.EmpleadoId && x.Id == request.EmpleadoAreaId, ct);

        if (asignacion is null)
            return Result.Failure<long>("Asignacion area-empleado no encontrada.");

        EmpleadoXPerfil perfil;
        try
        {
            perfil = EmpleadoXPerfil.Crear(request.EmpleadoAreaId, request.PerfilId, request.Orden);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.EmpleadosXPerfil.Add(perfil);
        await db.SaveChangesAsync(ct);

        return Result.Success(perfil.Id);
    }
}

public record RemoveEmpleadoPerfilCommand(long EmpleadoAreaId, long EmpleadoPerfilId) : IRequest<Result>;

public class RemoveEmpleadoPerfilCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RemoveEmpleadoPerfilCommand, Result>
{
    public async Task<Result> Handle(RemoveEmpleadoPerfilCommand request, CancellationToken ct)
    {
        var perfil = await db.EmpleadosXPerfil
            .FirstOrDefaultAsync(p => p.EmpleadoXAreaId == request.EmpleadoAreaId && p.Id == request.EmpleadoPerfilId, ct);

        if (perfil is null)
            return Result.Failure("Asignacion de perfil no encontrada.");

        db.EmpleadosXPerfil.Remove(perfil);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class EgresarEmpleadoCommandValidator : AbstractValidator<EgresarEmpleadoCommand>
{
    public EgresarEmpleadoCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).GreaterThan(0);
    }
}

public class MarcarLiquidacionPagadaCommandValidator : AbstractValidator<MarcarLiquidacionPagadaCommand>
{
    public MarcarLiquidacionPagadaCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).GreaterThan(0);
        RuleFor(x => x.LiquidacionId).GreaterThan(0);
    }
}

public class AddEmpleadoAreaCommandValidator : AbstractValidator<AddEmpleadoAreaCommand>
{
    public AddEmpleadoAreaCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).GreaterThan(0);
        RuleFor(x => x.AreaId).GreaterThan(0);
    }
}

public class RemoveEmpleadoAreaCommandValidator : AbstractValidator<RemoveEmpleadoAreaCommand>
{
    public RemoveEmpleadoAreaCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).GreaterThan(0);
        RuleFor(x => x.EmpleadoAreaId).GreaterThan(0);
    }
}

public class AddEmpleadoPerfilCommandValidator : AbstractValidator<AddEmpleadoPerfilCommand>
{
    public AddEmpleadoPerfilCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).GreaterThan(0);
        RuleFor(x => x.EmpleadoAreaId).GreaterThan(0);
        RuleFor(x => x.PerfilId).GreaterThan(0);
    }
}

public class RemoveEmpleadoPerfilCommandValidator : AbstractValidator<RemoveEmpleadoPerfilCommand>
{
    public RemoveEmpleadoPerfilCommandValidator()
    {
        RuleFor(x => x.EmpleadoAreaId).GreaterThan(0);
        RuleFor(x => x.EmpleadoPerfilId).GreaterThan(0);
    }
}
