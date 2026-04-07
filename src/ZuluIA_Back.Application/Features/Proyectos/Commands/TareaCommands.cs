using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Proyectos;

namespace ZuluIA_Back.Application.Features.Proyectos.Commands;

public record CreateTareaEstimadaCommand(
    long ProyectoId,
    long SucursalId,
    long? AsignadoA,
    string Descripcion,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    decimal HorasEstimadas,
    string? Observacion) : IRequest<Result<long>>;

public record UpdateTareaEstimadaCommand(
    long Id,
    long? AsignadoA,
    string Descripcion,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    decimal HorasEstimadas,
    string? Observacion) : IRequest<Result>;

public record DeactivateTareaEstimadaCommand(long Id) : IRequest<Result>;

public record ActivateTareaEstimadaCommand(long Id) : IRequest<Result>;

public record CreateTareaRealCommand(
    long ProyectoId,
    long SucursalId,
    long? TareaEstimadaId,
    long UsuarioId,
    DateOnly Fecha,
    string Descripcion,
    decimal HorasReales,
    string? Observacion) : IRequest<Result<long>>;

public record UpdateTareaRealCommand(
    long Id,
    string Descripcion,
    decimal HorasReales,
    string? Observacion) : IRequest<Result>;

public record ApproveTareaRealCommand(long Id) : IRequest<Result>;

public record DeleteTareaRealCommand(long Id) : IRequest<Result>;

public class CreateTareaEstimadaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateTareaEstimadaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTareaEstimadaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = TareaEstimada.Crear(
                request.ProyectoId,
                request.SucursalId,
                request.AsignadoA,
                request.Descripcion,
                request.FechaDesde,
                request.FechaHasta,
                request.HorasEstimadas,
                request.Observacion,
                userId: null);

            db.TareasEstimadas.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateTareaEstimadaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTareaEstimadaCommand, Result>
{
    public async Task<Result> Handle(UpdateTareaEstimadaCommand request, CancellationToken ct)
    {
        var entity = await db.TareasEstimadas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarea estimada {request.Id} no encontrada.");

        try
        {
            entity.Actualizar(
                request.Descripcion,
                request.FechaDesde,
                request.FechaHasta,
                request.HorasEstimadas,
                request.AsignadoA,
                request.Observacion,
                userId: null);

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class DeactivateTareaEstimadaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateTareaEstimadaCommand, Result>
{
    public async Task<Result> Handle(DeactivateTareaEstimadaCommand request, CancellationToken ct)
    {
        var entity = await db.TareasEstimadas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarea estimada {request.Id} no encontrada.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateTareaEstimadaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateTareaEstimadaCommand, Result>
{
    public async Task<Result> Handle(ActivateTareaEstimadaCommand request, CancellationToken ct)
    {
        var entity = await db.TareasEstimadas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarea estimada {request.Id} no encontrada.");

        entity.Activar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateTareaRealCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateTareaRealCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTareaRealCommand request, CancellationToken ct)
    {
        try
        {
            var entity = TareaReal.Registrar(
                request.ProyectoId,
                request.SucursalId,
                request.TareaEstimadaId,
                request.UsuarioId,
                request.Fecha,
                request.Descripcion,
                request.HorasReales,
                request.Observacion,
                userId: null);

            db.TareasReales.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateTareaRealCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTareaRealCommand, Result>
{
    public async Task<Result> Handle(UpdateTareaRealCommand request, CancellationToken ct)
    {
        var entity = await db.TareasReales.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarea real {request.Id} no encontrada.");

        try
        {
            entity.Actualizar(request.Descripcion, request.HorasReales, request.Observacion, userId: null);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class ApproveTareaRealCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ApproveTareaRealCommand, Result>
{
    public async Task<Result> Handle(ApproveTareaRealCommand request, CancellationToken ct)
    {
        var entity = await db.TareasReales.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarea real {request.Id} no encontrada.");

        entity.Aprobar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteTareaRealCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTareaRealCommand, Result>
{
    public async Task<Result> Handle(DeleteTareaRealCommand request, CancellationToken ct)
    {
        var entity = await db.TareasReales.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarea real {request.Id} no encontrada.");

        entity.Eliminar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateTareaEstimadaCommandValidator : AbstractValidator<CreateTareaEstimadaCommand>
{
    public CreateTareaEstimadaCommandValidator()
    {
        RuleFor(x => x.ProyectoId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.HorasEstimadas).GreaterThan(0);
    }
}

public class UpdateTareaEstimadaCommandValidator : AbstractValidator<UpdateTareaEstimadaCommand>
{
    public UpdateTareaEstimadaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.HorasEstimadas).GreaterThan(0);
    }
}

public class DeactivateTareaEstimadaCommandValidator : AbstractValidator<DeactivateTareaEstimadaCommand>
{
    public DeactivateTareaEstimadaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateTareaEstimadaCommandValidator : AbstractValidator<ActivateTareaEstimadaCommand>
{
    public ActivateTareaEstimadaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateTareaRealCommandValidator : AbstractValidator<CreateTareaRealCommand>
{
    public CreateTareaRealCommandValidator()
    {
        RuleFor(x => x.ProyectoId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.UsuarioId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.HorasReales).GreaterThan(0);
    }
}

public class UpdateTareaRealCommandValidator : AbstractValidator<UpdateTareaRealCommand>
{
    public UpdateTareaRealCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.HorasReales).GreaterThan(0);
    }
}

public class ApproveTareaRealCommandValidator : AbstractValidator<ApproveTareaRealCommand>
{
    public ApproveTareaRealCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteTareaRealCommandValidator : AbstractValidator<DeleteTareaRealCommand>
{
    public DeleteTareaRealCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
