using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Franquicias;

namespace ZuluIA_Back.Application.Features.Franquicias.Commands;

public record CreateGrupoEconomicoCommand(
    string Codigo,
    string Descripcion,
    long? UserId = null) : IRequest<Result<long>>;

public record UpdateGrupoEconomicoCommand(
    long Id,
    string Descripcion,
    long? UserId = null) : IRequest<Result>;

public record DeleteGrupoEconomicoCommand(
    long Id,
    long? UserId = null) : IRequest<Result>;

public record ActivateGrupoEconomicoCommand(
    long Id,
    long? UserId = null) : IRequest<Result>;

public class CreateGrupoEconomicoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateGrupoEconomicoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateGrupoEconomicoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.GrupoEconomicos.AsNoTracking().AnyAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>($"Ya existe un grupo economico con codigo '{request.Codigo}'.");

        GrupoEconomico entity;
        try
        {
            entity = GrupoEconomico.Crear(codigo, request.Descripcion, request.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.GrupoEconomicos.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateGrupoEconomicoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateGrupoEconomicoCommand, Result>
{
    public async Task<Result> Handle(UpdateGrupoEconomicoCommand request, CancellationToken ct)
    {
        var entity = await db.GrupoEconomicos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Grupo economico {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(request.Descripcion, request.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteGrupoEconomicoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteGrupoEconomicoCommand, Result>
{
    public async Task<Result> Handle(DeleteGrupoEconomicoCommand request, CancellationToken ct)
    {
        var entity = await db.GrupoEconomicos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Grupo economico {request.Id} no encontrado.");

        entity.Desactivar(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateGrupoEconomicoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateGrupoEconomicoCommand, Result>
{
    public async Task<Result> Handle(ActivateGrupoEconomicoCommand request, CancellationToken ct)
    {
        var entity = await db.GrupoEconomicos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Grupo economico {request.Id} no encontrado.");

        entity.Activar(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateGrupoEconomicoCommandValidator : AbstractValidator<CreateGrupoEconomicoCommand>
{
    public CreateGrupoEconomicoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateGrupoEconomicoCommandValidator : AbstractValidator<UpdateGrupoEconomicoCommand>
{
    public UpdateGrupoEconomicoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteGrupoEconomicoCommandValidator : AbstractValidator<DeleteGrupoEconomicoCommand>
{
    public DeleteGrupoEconomicoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateGrupoEconomicoCommandValidator : AbstractValidator<ActivateGrupoEconomicoCommand>
{
    public ActivateGrupoEconomicoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}