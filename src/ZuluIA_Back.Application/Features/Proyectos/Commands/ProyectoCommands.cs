using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Proyectos;

namespace ZuluIA_Back.Application.Features.Proyectos.Commands;

public record CreateProyectoCommand(
    string Codigo,
    string Descripcion,
    long SucursalId,
    long? TerceroId,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    int TotalCuotas,
    bool SoloPadre,
    string? Observacion) : IRequest<Result<long>>;

public record UpdateProyectoCommand(
    long Id,
    string Descripcion,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    int TotalCuotas,
    bool SoloPadre,
    string? Observacion) : IRequest<Result>;

public record FinalizarProyectoCommand(long Id) : IRequest<Result>;

public record AnularProyectoCommand(long Id) : IRequest<Result>;

public record ReactivarProyectoCommand(long Id) : IRequest<Result>;

public record AsignarComprobanteProyectoCommand(
    long ProyectoId,
    long ComprobanteId,
    decimal Porcentaje,
    decimal Importe,
    int NroCuota,
    string? Observacion) : IRequest<Result<long>>;

public record DesasignarComprobanteProyectoCommand(long ProyectoId, long LinkId) : IRequest<Result>;

public class CreateProyectoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateProyectoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateProyectoCommand request, CancellationToken ct)
    {
        try
        {
            var entity = Proyecto.Crear(
                request.Codigo,
                request.Descripcion,
                request.SucursalId,
                request.TerceroId,
                request.FechaInicio,
                request.FechaFin,
                request.TotalCuotas,
                request.SoloPadre,
                request.Observacion);

            await db.Proyectos.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateProyectoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateProyectoCommand, Result>
{
    public async Task<Result> Handle(UpdateProyectoCommand request, CancellationToken ct)
    {
        var entity = await db.Proyectos.FindAsync([request.Id], ct);
        if (entity is null)
            return Result.Failure($"No se encontró el proyecto con ID {request.Id}.");

        try
        {
            entity.Actualizar(
                request.Descripcion,
                request.FechaInicio,
                request.FechaFin,
                request.TotalCuotas,
                request.SoloPadre,
                request.Observacion);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class FinalizarProyectoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<FinalizarProyectoCommand, Result>
{
    public async Task<Result> Handle(FinalizarProyectoCommand request, CancellationToken ct)
    {
        var entity = await db.Proyectos.FindAsync([request.Id], ct);
        if (entity is null)
            return Result.Failure($"No se encontró el proyecto con ID {request.Id}.");

        entity.Finalizar();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AnularProyectoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AnularProyectoCommand, Result>
{
    public async Task<Result> Handle(AnularProyectoCommand request, CancellationToken ct)
    {
        var entity = await db.Proyectos.FindAsync([request.Id], ct);
        if (entity is null)
            return Result.Failure($"No se encontró el proyecto con ID {request.Id}.");

        entity.Anular();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ReactivarProyectoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ReactivarProyectoCommand, Result>
{
    public async Task<Result> Handle(ReactivarProyectoCommand request, CancellationToken ct)
    {
        var entity = await db.Proyectos.FindAsync([request.Id], ct);
        if (entity is null)
            return Result.Failure($"No se encontró el proyecto con ID {request.Id}.");

        entity.Reactivar();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AsignarComprobanteProyectoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AsignarComprobanteProyectoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AsignarComprobanteProyectoCommand request, CancellationToken ct)
    {
        var proyectoExiste = await db.Proyectos.AnyAsync(x => x.Id == request.ProyectoId, ct);
        if (!proyectoExiste)
            return Result.Failure<long>($"No se encontró el proyecto con ID {request.ProyectoId}.");

        try
        {
            var entity = ComprobanteProyecto.Crear(
                request.ComprobanteId,
                request.ProyectoId,
                request.Porcentaje,
                request.Importe,
                request.NroCuota,
                request.Observacion);

            await db.ComprobantesProyectos.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class DesasignarComprobanteProyectoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DesasignarComprobanteProyectoCommand, Result>
{
    public async Task<Result> Handle(DesasignarComprobanteProyectoCommand request, CancellationToken ct)
    {
        var entity = await db.ComprobantesProyectos
            .FirstOrDefaultAsync(x => x.Id == request.LinkId && x.ProyectoId == request.ProyectoId, ct);

        if (entity is null)
            return Result.Failure("No se encontró la asignación del comprobante para el proyecto.");

        entity.Deshabilitar();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateProyectoCommandValidator : AbstractValidator<CreateProyectoCommand>
{
    public CreateProyectoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TotalCuotas).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProyectoCommandValidator : AbstractValidator<UpdateProyectoCommand>
{
    public UpdateProyectoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalCuotas).GreaterThanOrEqualTo(0);
    }
}

public class FinalizarProyectoCommandValidator : AbstractValidator<FinalizarProyectoCommand>
{
    public FinalizarProyectoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AnularProyectoCommandValidator : AbstractValidator<AnularProyectoCommand>
{
    public AnularProyectoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ReactivarProyectoCommandValidator : AbstractValidator<ReactivarProyectoCommand>
{
    public ReactivarProyectoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AsignarComprobanteProyectoCommandValidator : AbstractValidator<AsignarComprobanteProyectoCommand>
{
    public AsignarComprobanteProyectoCommandValidator()
    {
        RuleFor(x => x.ProyectoId).GreaterThan(0);
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.Porcentaje).InclusiveBetween(0, 100);
    }
}

public class DesasignarComprobanteProyectoCommandValidator : AbstractValidator<DesasignarComprobanteProyectoCommand>
{
    public DesasignarComprobanteProyectoCommandValidator()
    {
        RuleFor(x => x.ProyectoId).GreaterThan(0);
        RuleFor(x => x.LinkId).GreaterThan(0);
    }
}
