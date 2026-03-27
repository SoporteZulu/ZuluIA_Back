using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Franquicias;

namespace ZuluIA_Back.Application.Features.Franquicias.Commands;

public record CreateFranquiciaVariableXUsuarioCommand(
    long PlanTrabajoId,
    long UsuarioId,
    long VariableId,
    string Valor,
    long? UserId = null) : IRequest<Result<long>>;

public record UpdateFranquiciaVariableXUsuarioCommand(
    long Id,
    string Valor,
    long? UserId = null) : IRequest<Result>;

public record DeleteFranquiciaVariableXUsuarioCommand(long Id) : IRequest<Result>;

public class CreateFranquiciaVariableXUsuarioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateFranquiciaVariableXUsuarioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateFranquiciaVariableXUsuarioCommand request, CancellationToken ct)
    {
        if (!await db.PlanesTrabajo.AsNoTracking().AnyAsync(x => x.Id == request.PlanTrabajoId, ct))
            return Result.Failure<long>($"Plan de trabajo {request.PlanTrabajoId} no encontrado.");

        if (!await db.Usuarios.AsNoTracking().AnyAsync(x => x.Id == request.UsuarioId, ct))
            return Result.Failure<long>($"Usuario {request.UsuarioId} no encontrado.");

        if (!await db.Variables.AsNoTracking().AnyAsync(x => x.Id == request.VariableId, ct))
            return Result.Failure<long>($"Variable {request.VariableId} no encontrada.");

        var exists = await db.FranquiciasVariablesXUsuarios.AsNoTracking()
            .AnyAsync(x => x.PlanTrabajoId == request.PlanTrabajoId
                && x.UsuarioId == request.UsuarioId
                && x.VariableId == request.VariableId, ct);
        if (exists)
            return Result.Failure<long>("Ya existe una asignacion para ese plan, usuario y variable.");

        FranquiciaVariableXUsuario entity;
        try
        {
            entity = FranquiciaVariableXUsuario.Crear(
                request.PlanTrabajoId,
                request.UsuarioId,
                request.VariableId,
                request.Valor,
                request.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.FranquiciasVariablesXUsuarios.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateFranquiciaVariableXUsuarioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateFranquiciaVariableXUsuarioCommand, Result>
{
    public async Task<Result> Handle(UpdateFranquiciaVariableXUsuarioCommand request, CancellationToken ct)
    {
        var entity = await db.FranquiciasVariablesXUsuarios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Asignacion de variable por usuario {request.Id} no encontrada.");

        try
        {
            entity.ActualizarValor(request.Valor, request.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteFranquiciaVariableXUsuarioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteFranquiciaVariableXUsuarioCommand, Result>
{
    public async Task<Result> Handle(DeleteFranquiciaVariableXUsuarioCommand request, CancellationToken ct)
    {
        var entity = await db.FranquiciasVariablesXUsuarios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Asignacion de variable por usuario {request.Id} no encontrada.");

        db.FranquiciasVariablesXUsuarios.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateFranquiciaVariableXUsuarioCommandValidator : AbstractValidator<CreateFranquiciaVariableXUsuarioCommand>
{
    public CreateFranquiciaVariableXUsuarioCommandValidator()
    {
        RuleFor(x => x.PlanTrabajoId).GreaterThan(0);
        RuleFor(x => x.UsuarioId).GreaterThan(0);
        RuleFor(x => x.VariableId).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class UpdateFranquiciaVariableXUsuarioCommandValidator : AbstractValidator<UpdateFranquiciaVariableXUsuarioCommand>
{
    public UpdateFranquiciaVariableXUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class DeleteFranquiciaVariableXUsuarioCommandValidator : AbstractValidator<DeleteFranquiciaVariableXUsuarioCommand>
{
    public DeleteFranquiciaVariableXUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}