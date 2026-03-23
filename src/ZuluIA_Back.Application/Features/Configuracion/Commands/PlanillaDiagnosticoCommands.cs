using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Application.Features.Configuracion.Commands;

public record CreatePlantillaDiagnosticoCommand(
    string Descripcion,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    string? Observaciones) : IRequest<Result<long>>;

public record UpdatePlantillaDiagnosticoCommand(
    long Id,
    string Descripcion,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    string? Observaciones) : IRequest<Result>;

public record DeletePlantillaDiagnosticoCommand(long Id) : IRequest<Result>;

public record AddPlantillaDiagnosticoDetalleCommand(
    long PlantillaId,
    long? VariableDetalleId,
    decimal PorcentajeIncidencia,
    decimal? ValorObjetivo) : IRequest<Result<long>>;

public record UpdatePlantillaDiagnosticoDetalleCommand(
    long PlantillaId,
    long DetalleId,
    long? VariableDetalleId,
    decimal PorcentajeIncidencia,
    decimal? ValorObjetivo) : IRequest<Result>;

public record DeletePlantillaDiagnosticoDetalleCommand(long PlantillaId, long DetalleId) : IRequest<Result>;

public record CreatePlanillaDiagnosticoCommand(
    long? ClienteId,
    long? PlantillaId,
    long? TipoPlanillaId,
    long? PlanillaPadreId,
    long? EstadoId,
    DateTime? FechaEvaluacion,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    string? Observaciones) : IRequest<Result<long>>;

public record UpdatePlanillaDiagnosticoCommand(
    long Id,
    long? ClienteId,
    long? PlantillaId,
    long? TipoPlanillaId,
    long? PlanillaPadreId,
    long? EstadoId,
    DateTime? FechaEvaluacion,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    string? Observaciones) : IRequest<Result>;

public record DeletePlanillaDiagnosticoCommand(long Id) : IRequest<Result>;

public record AddPlanillaDiagnosticoDetalleCommand(
    long PlanillaId,
    long? VariableDetalleId,
    long? OpcionVariableId,
    decimal PuntajeVariable,
    decimal Valor,
    decimal PorcentajeIncidencia,
    decimal? ValorObjetivo) : IRequest<Result<long>>;

public record UpdatePlanillaDiagnosticoDetalleCommand(
    long PlanillaId,
    long DetalleId,
    long? OpcionVariableId,
    decimal PuntajeVariable,
    decimal Valor,
    decimal PorcentajeIncidencia,
    decimal? ValorObjetivo) : IRequest<Result>;

public record DeletePlanillaDiagnosticoDetalleCommand(long PlanillaId, long DetalleId) : IRequest<Result>;

public class CreatePlantillaDiagnosticoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreatePlantillaDiagnosticoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePlantillaDiagnosticoCommand request, CancellationToken ct)
    {
        try
        {
            var entity = PlantillaDiagnostico.Crear(request.Descripcion, request.FechaDesde, request.FechaHasta, request.Observaciones);
            db.PlantillasDiagnostico.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdatePlantillaDiagnosticoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePlantillaDiagnosticoCommand, Result>
{
    public async Task<Result> Handle(UpdatePlantillaDiagnosticoCommand request, CancellationToken ct)
    {
        var entity = await db.PlantillasDiagnostico.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Plantilla {request.Id} no encontrada.");

        try
        {
            entity.Actualizar(request.Descripcion, request.FechaDesde, request.FechaHasta, request.Observaciones);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class DeletePlantillaDiagnosticoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeletePlantillaDiagnosticoCommand, Result>
{
    public async Task<Result> Handle(DeletePlantillaDiagnosticoCommand request, CancellationToken ct)
    {
        var entity = await db.PlantillasDiagnostico.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Plantilla {request.Id} no encontrada.");

        db.PlantillasDiagnostico.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AddPlantillaDiagnosticoDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddPlantillaDiagnosticoDetalleCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddPlantillaDiagnosticoDetalleCommand request, CancellationToken ct)
    {
        var plantillaExists = await db.PlantillasDiagnostico.AnyAsync(x => x.Id == request.PlantillaId, ct);
        if (!plantillaExists)
            return Result.Failure<long>($"Plantilla {request.PlantillaId} no encontrada.");

        var entity = PlantillaDiagnosticoDetalle.Crear(
            request.PlantillaId,
            request.VariableDetalleId,
            request.PorcentajeIncidencia,
            request.ValorObjetivo);

        db.PlantillasDiagnosticoDetalle.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdatePlantillaDiagnosticoDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePlantillaDiagnosticoDetalleCommand, Result>
{
    public async Task<Result> Handle(UpdatePlantillaDiagnosticoDetalleCommand request, CancellationToken ct)
    {
        var entity = await db.PlantillasDiagnosticoDetalle
            .FirstOrDefaultAsync(x => x.Id == request.DetalleId && x.PlantillaId == request.PlantillaId, ct);

        if (entity is null)
            return Result.Failure("Detalle de plantilla no encontrado.");

        entity.Actualizar(request.VariableDetalleId, request.PorcentajeIncidencia, request.ValorObjetivo);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeletePlantillaDiagnosticoDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeletePlantillaDiagnosticoDetalleCommand, Result>
{
    public async Task<Result> Handle(DeletePlantillaDiagnosticoDetalleCommand request, CancellationToken ct)
    {
        var entity = await db.PlantillasDiagnosticoDetalle
            .FirstOrDefaultAsync(x => x.Id == request.DetalleId && x.PlantillaId == request.PlantillaId, ct);

        if (entity is null)
            return Result.Failure("Detalle de plantilla no encontrado.");

        db.PlantillasDiagnosticoDetalle.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreatePlanillaDiagnosticoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreatePlanillaDiagnosticoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePlanillaDiagnosticoCommand request, CancellationToken ct)
    {
        var entity = PlanillaDiagnostico.Crear(
            request.ClienteId,
            request.PlantillaId,
            request.TipoPlanillaId,
            request.PlanillaPadreId,
            request.EstadoId,
            request.FechaEvaluacion,
            request.FechaDesde,
            request.FechaHasta,
            request.Observaciones);

        db.PlanillasDiagnostico.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdatePlanillaDiagnosticoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePlanillaDiagnosticoCommand, Result>
{
    public async Task<Result> Handle(UpdatePlanillaDiagnosticoCommand request, CancellationToken ct)
    {
        var entity = await db.PlanillasDiagnostico.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Planilla {request.Id} no encontrada.");

        entity.Actualizar(
            request.ClienteId,
            request.PlantillaId,
            request.TipoPlanillaId,
            request.PlanillaPadreId,
            request.EstadoId,
            request.FechaEvaluacion,
            request.FechaDesde,
            request.FechaHasta,
            request.Observaciones);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeletePlanillaDiagnosticoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeletePlanillaDiagnosticoCommand, Result>
{
    public async Task<Result> Handle(DeletePlanillaDiagnosticoCommand request, CancellationToken ct)
    {
        var entity = await db.PlanillasDiagnostico.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Planilla {request.Id} no encontrada.");

        db.PlanillasDiagnostico.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AddPlanillaDiagnosticoDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddPlanillaDiagnosticoDetalleCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddPlanillaDiagnosticoDetalleCommand request, CancellationToken ct)
    {
        var planillaExists = await db.PlanillasDiagnostico.AnyAsync(x => x.Id == request.PlanillaId, ct);
        if (!planillaExists)
            return Result.Failure<long>($"Planilla {request.PlanillaId} no encontrada.");

        var entity = PlanillaDiagnosticoDetalle.Crear(
            request.PlanillaId,
            request.VariableDetalleId,
            request.OpcionVariableId,
            request.PuntajeVariable,
            request.Valor,
            request.PorcentajeIncidencia,
            request.ValorObjetivo);

        db.PlanillasDiagnosticoDetalle.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdatePlanillaDiagnosticoDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePlanillaDiagnosticoDetalleCommand, Result>
{
    public async Task<Result> Handle(UpdatePlanillaDiagnosticoDetalleCommand request, CancellationToken ct)
    {
        var entity = await db.PlanillasDiagnosticoDetalle
            .FirstOrDefaultAsync(x => x.Id == request.DetalleId && x.PlanillaId == request.PlanillaId, ct);

        if (entity is null)
            return Result.Failure("Detalle de planilla no encontrado.");

        entity.Actualizar(
            request.OpcionVariableId,
            request.PuntajeVariable,
            request.Valor,
            request.PorcentajeIncidencia,
            request.ValorObjetivo);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeletePlanillaDiagnosticoDetalleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeletePlanillaDiagnosticoDetalleCommand, Result>
{
    public async Task<Result> Handle(DeletePlanillaDiagnosticoDetalleCommand request, CancellationToken ct)
    {
        var entity = await db.PlanillasDiagnosticoDetalle
            .FirstOrDefaultAsync(x => x.Id == request.DetalleId && x.PlanillaId == request.PlanillaId, ct);

        if (entity is null)
            return Result.Failure("Detalle de planilla no encontrado.");

        db.PlanillasDiagnosticoDetalle.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreatePlantillaDiagnosticoCommandValidator : AbstractValidator<CreatePlantillaDiagnosticoCommand>
{
    public CreatePlantillaDiagnosticoCommandValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class UpdatePlantillaDiagnosticoCommandValidator : AbstractValidator<UpdatePlantillaDiagnosticoCommand>
{
    public UpdatePlantillaDiagnosticoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class DeletePlantillaDiagnosticoCommandValidator : AbstractValidator<DeletePlantillaDiagnosticoCommand>
{
    public DeletePlantillaDiagnosticoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AddPlantillaDiagnosticoDetalleCommandValidator : AbstractValidator<AddPlantillaDiagnosticoDetalleCommand>
{
    public AddPlantillaDiagnosticoDetalleCommandValidator()
    {
        RuleFor(x => x.PlantillaId).GreaterThan(0);
    }
}

public class UpdatePlantillaDiagnosticoDetalleCommandValidator : AbstractValidator<UpdatePlantillaDiagnosticoDetalleCommand>
{
    public UpdatePlantillaDiagnosticoDetalleCommandValidator()
    {
        RuleFor(x => x.PlantillaId).GreaterThan(0);
        RuleFor(x => x.DetalleId).GreaterThan(0);
    }
}

public class DeletePlantillaDiagnosticoDetalleCommandValidator : AbstractValidator<DeletePlantillaDiagnosticoDetalleCommand>
{
    public DeletePlantillaDiagnosticoDetalleCommandValidator()
    {
        RuleFor(x => x.PlantillaId).GreaterThan(0);
        RuleFor(x => x.DetalleId).GreaterThan(0);
    }
}

public class CreatePlanillaDiagnosticoCommandValidator : AbstractValidator<CreatePlanillaDiagnosticoCommand>
{
    public CreatePlanillaDiagnosticoCommandValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .When(x => x.ClienteId.HasValue);

        RuleFor(x => x.PlantillaId)
            .GreaterThan(0)
            .When(x => x.PlantillaId.HasValue);
    }
}

public class UpdatePlanillaDiagnosticoCommandValidator : AbstractValidator<UpdatePlanillaDiagnosticoCommand>
{
    public UpdatePlanillaDiagnosticoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeletePlanillaDiagnosticoCommandValidator : AbstractValidator<DeletePlanillaDiagnosticoCommand>
{
    public DeletePlanillaDiagnosticoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AddPlanillaDiagnosticoDetalleCommandValidator : AbstractValidator<AddPlanillaDiagnosticoDetalleCommand>
{
    public AddPlanillaDiagnosticoDetalleCommandValidator()
    {
        RuleFor(x => x.PlanillaId).GreaterThan(0);
    }
}

public class UpdatePlanillaDiagnosticoDetalleCommandValidator : AbstractValidator<UpdatePlanillaDiagnosticoDetalleCommand>
{
    public UpdatePlanillaDiagnosticoDetalleCommandValidator()
    {
        RuleFor(x => x.PlanillaId).GreaterThan(0);
        RuleFor(x => x.DetalleId).GreaterThan(0);
    }
}

public class DeletePlanillaDiagnosticoDetalleCommandValidator : AbstractValidator<DeletePlanillaDiagnosticoDetalleCommand>
{
    public DeletePlanillaDiagnosticoDetalleCommandValidator()
    {
        RuleFor(x => x.PlanillaId).GreaterThan(0);
        RuleFor(x => x.DetalleId).GreaterThan(0);
    }
}