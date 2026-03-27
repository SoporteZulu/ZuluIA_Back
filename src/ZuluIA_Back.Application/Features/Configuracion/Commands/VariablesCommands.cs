using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Application.Features.Configuracion.Commands;

public record CreateAspectoCommand(
    string Codigo,
    string Descripcion,
    long? AspectoPadreId,
    int Orden,
    int Nivel,
    string? CodigoEstructura,
    string? Observacion) : IRequest<Result<long>>;

public record UpdateAspectoCommand(
    long Id,
    string Descripcion,
    long? AspectoPadreId,
    int Orden,
    int Nivel,
    string? CodigoEstructura,
    string? Observacion) : IRequest<Result>;

public record DeleteAspectoCommand(long Id) : IRequest<Result>;

public record CreateVariableCommand(
    string Codigo,
    string Descripcion,
    long? TipoVariableId,
    long? TipoComprobanteId,
    long? AspectoId,
    int Nivel,
    int Orden,
    string? CodigoEstructura,
    string? Observacion,
    string? Condicionante,
    bool Editable) : IRequest<Result<long>>;

public record UpdateVariableCommand(
    long Id,
    string Descripcion,
    long? TipoVariableId,
    long? TipoComprobanteId,
    long? AspectoId,
    int Nivel,
    int Orden,
    string? CodigoEstructura,
    string? Observacion,
    string? Condicionante,
    bool Editable) : IRequest<Result>;

public record DeleteVariableCommand(long Id) : IRequest<Result>;

public record CreateOpcionVariableCommand(string Codigo, string Descripcion, string? Observaciones) : IRequest<Result<long>>;

public record UpdateOpcionVariableCommand(long Id, string Codigo, string Descripcion, string? Observaciones) : IRequest<Result>;

public record DeleteOpcionVariableCommand(long Id) : IRequest<Result>;

public record AddVariableDetalleCommand(
    long VariableId,
    long? OpcionVariableId,
    bool AplicaPuntajePenalizacion,
    bool VisualizarOpcion,
    decimal? PorcentajeIncidencia,
    decimal? ValorObjetivo) : IRequest<Result<long>>;

public record UpdateVariableDetalleCommand(
    long VariableId,
    long DetalleId,
    long? OpcionVariableId,
    bool AplicaPuntajePenalizacion,
    bool VisualizarOpcion,
    decimal? PorcentajeIncidencia,
    decimal? ValorObjetivo) : IRequest<Result>;

public record DeleteVariableDetalleCommand(long VariableId, long DetalleId) : IRequest<Result>;

public class CreateAspectoCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateAspectoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateAspectoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.Aspectos.AnyAsync(a => a.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>($"Ya existe un aspecto con código '{request.Codigo}'.");

        try
        {
            var entity = Aspecto.Crear(
                request.Codigo,
                request.Descripcion,
                request.AspectoPadreId,
                request.Orden,
                request.Nivel,
                request.CodigoEstructura,
                request.Observacion);

            db.Aspectos.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateAspectoCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateAspectoCommand, Result>
{
    public async Task<Result> Handle(UpdateAspectoCommand request, CancellationToken ct)
    {
        var entity = await db.Aspectos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Aspecto {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(
                request.Descripcion,
                request.AspectoPadreId,
                request.Orden,
                request.Nivel,
                request.CodigoEstructura,
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

public class DeleteAspectoCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteAspectoCommand, Result>
{
    public async Task<Result> Handle(DeleteAspectoCommand request, CancellationToken ct)
    {
        var entity = await db.Aspectos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Aspecto {request.Id} no encontrado.");

        var tieneVariables = await db.Variables.AnyAsync(v => v.AspectoId == request.Id, ct);
        if (tieneVariables)
            return Result.Failure("No se puede eliminar un aspecto que tiene variables asociadas.");

        var tieneHijos = await db.Aspectos.AnyAsync(a => a.AspectoPadreId == request.Id, ct);
        if (tieneHijos)
            return Result.Failure("No se puede eliminar un aspecto que tiene sub-aspectos.");

        db.Aspectos.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateVariableCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateVariableCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateVariableCommand request, CancellationToken ct)
    {
        try
        {
            var entity = Variable.Crear(
                request.Codigo,
                request.Descripcion,
                request.TipoVariableId,
                request.TipoComprobanteId,
                request.AspectoId,
                request.Nivel,
                request.Orden,
                request.CodigoEstructura,
                request.Observacion,
                request.Condicionante,
                request.Editable);

            db.Variables.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateVariableCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateVariableCommand, Result>
{
    public async Task<Result> Handle(UpdateVariableCommand request, CancellationToken ct)
    {
        var entity = await db.Variables.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Variable {request.Id} no encontrada.");

        try
        {
            entity.Actualizar(
                request.Descripcion,
                request.TipoVariableId,
                request.TipoComprobanteId,
                request.AspectoId,
                request.Nivel,
                request.Orden,
                request.CodigoEstructura,
                request.Observacion,
                request.Condicionante,
                request.Editable);

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class DeleteVariableCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteVariableCommand, Result>
{
    public async Task<Result> Handle(DeleteVariableCommand request, CancellationToken ct)
    {
        var entity = await db.Variables.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Variable {request.Id} no encontrada.");

        db.Variables.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateOpcionVariableCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateOpcionVariableCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateOpcionVariableCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.OpcionesVariable.AnyAsync(o => o.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>($"Ya existe una opción con código '{request.Codigo}'.");

        try
        {
            var entity = OpcionVariable.Crear(request.Codigo, request.Descripcion, request.Observaciones);
            db.OpcionesVariable.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateOpcionVariableCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateOpcionVariableCommand, Result>
{
    public async Task<Result> Handle(UpdateOpcionVariableCommand request, CancellationToken ct)
    {
        var entity = await db.OpcionesVariable.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Opción de variable {request.Id} no encontrada.");

        try
        {
            entity.Actualizar(request.Codigo, request.Descripcion, request.Observaciones);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class DeleteOpcionVariableCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteOpcionVariableCommand, Result>
{
    public async Task<Result> Handle(DeleteOpcionVariableCommand request, CancellationToken ct)
    {
        var entity = await db.OpcionesVariable.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Opción de variable {request.Id} no encontrada.");

        db.OpcionesVariable.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AddVariableDetalleCommandHandler(IApplicationDbContext db) : IRequestHandler<AddVariableDetalleCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddVariableDetalleCommand request, CancellationToken ct)
    {
        var variableExists = await db.Variables.AnyAsync(v => v.Id == request.VariableId, ct);
        if (!variableExists)
            return Result.Failure<long>($"Variable {request.VariableId} no encontrada.");

        try
        {
            var entity = VariableDetalle.Crear(
                request.VariableId,
                request.OpcionVariableId,
                request.AplicaPuntajePenalizacion,
                request.VisualizarOpcion,
                request.PorcentajeIncidencia,
                request.ValorObjetivo);

            db.VariablesDetalle.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateVariableDetalleCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateVariableDetalleCommand, Result>
{
    public async Task<Result> Handle(UpdateVariableDetalleCommand request, CancellationToken ct)
    {
        var entity = await db.VariablesDetalle
            .FirstOrDefaultAsync(v => v.Id == request.DetalleId && v.VariableId == request.VariableId, ct);

        if (entity is null)
            return Result.Failure("Detalle no encontrado.");

        entity.Actualizar(
            request.OpcionVariableId,
            request.AplicaPuntajePenalizacion,
            request.VisualizarOpcion,
            request.PorcentajeIncidencia,
            request.ValorObjetivo);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteVariableDetalleCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteVariableDetalleCommand, Result>
{
    public async Task<Result> Handle(DeleteVariableDetalleCommand request, CancellationToken ct)
    {
        var entity = await db.VariablesDetalle
            .FirstOrDefaultAsync(v => v.Id == request.DetalleId && v.VariableId == request.VariableId, ct);

        if (entity is null)
            return Result.Failure("Detalle no encontrado.");

        db.VariablesDetalle.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateAspectoCommandValidator : AbstractValidator<CreateAspectoCommand>
{
    public CreateAspectoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class UpdateAspectoCommandValidator : AbstractValidator<UpdateAspectoCommand>
{
    public UpdateAspectoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class DeleteAspectoCommandValidator : AbstractValidator<DeleteAspectoCommand>
{
    public DeleteAspectoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateVariableCommandValidator : AbstractValidator<CreateVariableCommand>
{
    public CreateVariableCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class UpdateVariableCommandValidator : AbstractValidator<UpdateVariableCommand>
{
    public UpdateVariableCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class DeleteVariableCommandValidator : AbstractValidator<DeleteVariableCommand>
{
    public DeleteVariableCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateOpcionVariableCommandValidator : AbstractValidator<CreateOpcionVariableCommand>
{
    public CreateOpcionVariableCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class UpdateOpcionVariableCommandValidator : AbstractValidator<UpdateOpcionVariableCommand>
{
    public UpdateOpcionVariableCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class DeleteOpcionVariableCommandValidator : AbstractValidator<DeleteOpcionVariableCommand>
{
    public DeleteOpcionVariableCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AddVariableDetalleCommandValidator : AbstractValidator<AddVariableDetalleCommand>
{
    public AddVariableDetalleCommandValidator()
    {
        RuleFor(x => x.VariableId).GreaterThan(0);
    }
}

public class UpdateVariableDetalleCommandValidator : AbstractValidator<UpdateVariableDetalleCommand>
{
    public UpdateVariableDetalleCommandValidator()
    {
        RuleFor(x => x.VariableId).GreaterThan(0);
        RuleFor(x => x.DetalleId).GreaterThan(0);
    }
}

public class DeleteVariableDetalleCommandValidator : AbstractValidator<DeleteVariableDetalleCommand>
{
    public DeleteVariableDetalleCommandValidator()
    {
        RuleFor(x => x.VariableId).GreaterThan(0);
        RuleFor(x => x.DetalleId).GreaterThan(0);
    }
}