using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.CRM;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Application.Features.CRM.Commands;

public record CreateContactoCrmCommand(long PersonaId, long PersonaContactoId, long? TipoRelacionId) : IRequest<Result<long>>;
public record UpdateContactoCrmCommand(long Id, long? TipoRelacionId) : IRequest<Result>;
public record DeleteContactoCrmCommand(long Id) : IRequest<Result>;

public record CreateCrmTipoComunicadoCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;
public record UpdateCrmTipoComunicadoCommand(long Id, string Descripcion) : IRequest<Result>;
public record DeactivateCrmTipoComunicadoCommand(long Id) : IRequest<Result>;
public record ActivateCrmTipoComunicadoCommand(long Id) : IRequest<Result>;

public record CreateCrmMotivoCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;
public record UpdateCrmMotivoCommand(long Id, string Descripcion) : IRequest<Result>;
public record DeactivateCrmMotivoCommand(long Id) : IRequest<Result>;
public record ActivateCrmMotivoCommand(long Id) : IRequest<Result>;

public record CreateCrmInteresCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;
public record UpdateCrmInteresCommand(long Id, string Descripcion) : IRequest<Result>;
public record DeactivateCrmInteresCommand(long Id) : IRequest<Result>;
public record ActivateCrmInteresCommand(long Id) : IRequest<Result>;

public record CreateCrmCampanaCommand(long SucursalId, string Nombre, string? Descripcion, DateOnly FechaInicio, DateOnly FechaFin, decimal? Presupuesto) : IRequest<Result<long>>;
public record UpdateCrmCampanaCommand(long Id, string Nombre, string? Descripcion, DateOnly FechaInicio, DateOnly FechaFin, decimal? Presupuesto) : IRequest<Result>;
public record CloseCrmCampanaCommand(long Id) : IRequest<Result>;

public record CreateCrmComunicadoCommand(long SucursalId, long TerceroId, long? CampanaId, long? TipoId, DateOnly Fecha, string Asunto, string? Contenido, long? UsuarioId) : IRequest<Result<long>>;
public record UpdateCrmComunicadoCommand(long Id, string Asunto, string? Contenido) : IRequest<Result>;
public record DeleteCrmComunicadoCommand(long Id) : IRequest<Result>;

public record CreateCrmSeguimientoCommand(long SucursalId, long TerceroId, long? MotivoId, long? InteresId, long? CampanaId, DateOnly Fecha, string Descripcion, DateOnly? ProximaAccion, long? UsuarioId) : IRequest<Result<long>>;
public record UpdateCrmSeguimientoCommand(long Id, string Descripcion, DateOnly? ProximaAccion) : IRequest<Result>;
public record DeleteCrmSeguimientoCommand(long Id) : IRequest<Result>;

public class CreateContactoCrmCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateContactoCrmCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateContactoCrmCommand request, CancellationToken ct)
    {
        try
        {
            var entity = Contacto.Crear(request.PersonaId, request.PersonaContactoId, request.TipoRelacionId);
            db.Contactos.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateContactoCrmCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateContactoCrmCommand, Result>
{
    public async Task<Result> Handle(UpdateContactoCrmCommand request, CancellationToken ct)
    {
        var entity = await db.Contactos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Contacto {request.Id} no encontrado.");

        entity.ActualizarTipoRelacion(request.TipoRelacionId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteContactoCrmCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteContactoCrmCommand, Result>
{
    public async Task<Result> Handle(DeleteContactoCrmCommand request, CancellationToken ct)
    {
        var entity = await db.Contactos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Contacto {request.Id} no encontrado.");

        db.Contactos.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmTipoComunicadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCrmTipoComunicadoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmTipoComunicadoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.CrmTiposComunicado.AnyAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un tipo de comunicado con ese codigo.");

        var entity = CrmTipoComunicado.Crear(codigo, request.Descripcion, userId: null);
        db.CrmTiposComunicado.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateCrmTipoComunicadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCrmTipoComunicadoCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmTipoComunicadoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmTiposComunicado.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo de comunicado {request.Id} no encontrado.");

        entity.Actualizar(request.Descripcion, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeactivateCrmTipoComunicadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateCrmTipoComunicadoCommand, Result>
{
    public async Task<Result> Handle(DeactivateCrmTipoComunicadoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmTiposComunicado.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo de comunicado {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateCrmTipoComunicadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateCrmTipoComunicadoCommand, Result>
{
    public async Task<Result> Handle(ActivateCrmTipoComunicadoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmTiposComunicado.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo de comunicado {request.Id} no encontrado.");

        entity.Activar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmMotivoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCrmMotivoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmMotivoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.CrmMotivos.AnyAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un motivo con ese codigo.");

        var entity = CrmMotivo.Crear(codigo, request.Descripcion, userId: null);
        db.CrmMotivos.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateCrmMotivoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCrmMotivoCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmMotivoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmMotivos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Motivo {request.Id} no encontrado.");

        entity.Actualizar(request.Descripcion, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeactivateCrmMotivoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateCrmMotivoCommand, Result>
{
    public async Task<Result> Handle(DeactivateCrmMotivoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmMotivos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Motivo {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateCrmMotivoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateCrmMotivoCommand, Result>
{
    public async Task<Result> Handle(ActivateCrmMotivoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmMotivos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Motivo {request.Id} no encontrado.");

        entity.Activar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmInteresCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCrmInteresCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmInteresCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.CrmIntereses.AnyAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un interes con ese codigo.");

        var entity = CrmInteres.Crear(codigo, request.Descripcion, userId: null);
        db.CrmIntereses.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateCrmInteresCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCrmInteresCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmInteresCommand request, CancellationToken ct)
    {
        var entity = await db.CrmIntereses.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Interes {request.Id} no encontrado.");

        entity.Actualizar(request.Descripcion, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeactivateCrmInteresCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateCrmInteresCommand, Result>
{
    public async Task<Result> Handle(DeactivateCrmInteresCommand request, CancellationToken ct)
    {
        var entity = await db.CrmIntereses.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Interes {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateCrmInteresCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateCrmInteresCommand, Result>
{
    public async Task<Result> Handle(ActivateCrmInteresCommand request, CancellationToken ct)
    {
        var entity = await db.CrmIntereses.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Interes {request.Id} no encontrado.");

        entity.Activar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmCampanaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCrmCampanaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmCampanaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = CrmCampana.Crear(request.SucursalId, request.Nombre, request.Descripcion, request.FechaInicio, request.FechaFin, request.Presupuesto, userId: null);
            db.CrmCampanas.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateCrmCampanaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCrmCampanaCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmCampanaCommand request, CancellationToken ct)
    {
        var entity = await db.CrmCampanas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Campana {request.Id} no encontrada.");

        entity.Actualizar(request.Nombre, request.Descripcion, request.FechaInicio, request.FechaFin, request.Presupuesto, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CloseCrmCampanaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CloseCrmCampanaCommand, Result>
{
    public async Task<Result> Handle(CloseCrmCampanaCommand request, CancellationToken ct)
    {
        var entity = await db.CrmCampanas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Campana {request.Id} no encontrada.");

        entity.Cerrar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmComunicadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCrmComunicadoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmComunicadoCommand request, CancellationToken ct)
    {
        try
        {
            var entity = CrmComunicado.Crear(request.SucursalId, request.TerceroId, request.CampanaId, request.TipoId, request.Fecha, request.Asunto, request.Contenido, request.UsuarioId);
            db.CrmComunicados.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateCrmComunicadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCrmComunicadoCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmComunicadoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmComunicados.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Comunicado {request.Id} no encontrado.");

        entity.Actualizar(request.Asunto, request.Contenido, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteCrmComunicadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteCrmComunicadoCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmComunicadoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmComunicados.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Comunicado {request.Id} no encontrado.");

        db.CrmComunicados.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmSeguimientoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCrmSeguimientoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmSeguimientoCommand request, CancellationToken ct)
    {
        try
        {
            var entity = CrmSeguimiento.Crear(request.SucursalId, request.TerceroId, request.MotivoId, request.InteresId, request.CampanaId, request.Fecha, request.Descripcion, request.ProximaAccion, request.UsuarioId);
            db.CrmSeguimientos.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateCrmSeguimientoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCrmSeguimientoCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmSeguimientoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmSeguimientos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Seguimiento {request.Id} no encontrado.");

        entity.Actualizar(request.Descripcion, request.ProximaAccion, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteCrmSeguimientoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteCrmSeguimientoCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmSeguimientoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmSeguimientos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Seguimiento {request.Id} no encontrado.");

        db.CrmSeguimientos.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateContactoCrmCommandValidator : AbstractValidator<CreateContactoCrmCommand>
{
    public CreateContactoCrmCommandValidator()
    {
        RuleFor(x => x.PersonaId).GreaterThan(0);
        RuleFor(x => x.PersonaContactoId).GreaterThan(0);
    }
}

public class UpdateContactoCrmCommandValidator : AbstractValidator<UpdateContactoCrmCommand>
{
    public UpdateContactoCrmCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class DeleteContactoCrmCommandValidator : AbstractValidator<DeleteContactoCrmCommand>
{
    public DeleteContactoCrmCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmTipoComunicadoCommandValidator : AbstractValidator<CreateCrmTipoComunicadoCommand>
{
    public CreateCrmTipoComunicadoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateCrmTipoComunicadoCommandValidator : AbstractValidator<UpdateCrmTipoComunicadoCommand>
{
    public UpdateCrmTipoComunicadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeactivateCrmTipoComunicadoCommandValidator : AbstractValidator<DeactivateCrmTipoComunicadoCommand>
{
    public DeactivateCrmTipoComunicadoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class ActivateCrmTipoComunicadoCommandValidator : AbstractValidator<ActivateCrmTipoComunicadoCommand>
{
    public ActivateCrmTipoComunicadoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmMotivoCommandValidator : AbstractValidator<CreateCrmMotivoCommand>
{
    public CreateCrmMotivoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateCrmMotivoCommandValidator : AbstractValidator<UpdateCrmMotivoCommand>
{
    public UpdateCrmMotivoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeactivateCrmMotivoCommandValidator : AbstractValidator<DeactivateCrmMotivoCommand>
{
    public DeactivateCrmMotivoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class ActivateCrmMotivoCommandValidator : AbstractValidator<ActivateCrmMotivoCommand>
{
    public ActivateCrmMotivoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmInteresCommandValidator : AbstractValidator<CreateCrmInteresCommand>
{
    public CreateCrmInteresCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateCrmInteresCommandValidator : AbstractValidator<UpdateCrmInteresCommand>
{
    public UpdateCrmInteresCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeactivateCrmInteresCommandValidator : AbstractValidator<DeactivateCrmInteresCommand>
{
    public DeactivateCrmInteresCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class ActivateCrmInteresCommandValidator : AbstractValidator<ActivateCrmInteresCommand>
{
    public ActivateCrmInteresCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmCampanaCommandValidator : AbstractValidator<CreateCrmCampanaCommand>
{
    public CreateCrmCampanaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
    }
}

public class UpdateCrmCampanaCommandValidator : AbstractValidator<UpdateCrmCampanaCommand>
{
    public UpdateCrmCampanaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
    }
}

public class CloseCrmCampanaCommandValidator : AbstractValidator<CloseCrmCampanaCommand>
{
    public CloseCrmCampanaCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmComunicadoCommandValidator : AbstractValidator<CreateCrmComunicadoCommand>
{
    public CreateCrmComunicadoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Asunto).NotEmpty();
    }
}

public class UpdateCrmComunicadoCommandValidator : AbstractValidator<UpdateCrmComunicadoCommand>
{
    public UpdateCrmComunicadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Asunto).NotEmpty();
    }
}

public class DeleteCrmComunicadoCommandValidator : AbstractValidator<DeleteCrmComunicadoCommand>
{
    public DeleteCrmComunicadoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmSeguimientoCommandValidator : AbstractValidator<CreateCrmSeguimientoCommand>
{
    public CreateCrmSeguimientoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateCrmSeguimientoCommandValidator : AbstractValidator<UpdateCrmSeguimientoCommand>
{
    public UpdateCrmSeguimientoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteCrmSeguimientoCommandValidator : AbstractValidator<DeleteCrmSeguimientoCommand>
{
    public DeleteCrmSeguimientoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
