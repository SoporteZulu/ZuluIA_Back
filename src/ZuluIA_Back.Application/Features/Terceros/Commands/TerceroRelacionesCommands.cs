using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record AddTerceroDomicilioCommand(
    long TerceroId,
    long? TipoDomicilioId,
    long? ProvinciaId,
    long? LocalidadId,
    string? Calle,
    string? Barrio,
    string? CodigoPostal,
    string? Observacion,
    int Orden,
    bool EsDefecto) : IRequest<Result<long>>;

public record UpdateTerceroDomicilioCommand(
    long TerceroId,
    long DomicilioId,
    long? TipoDomicilioId,
    long? ProvinciaId,
    long? LocalidadId,
    string? Calle,
    string? Barrio,
    string? CodigoPostal,
    string? Observacion,
    int Orden,
    bool EsDefecto) : IRequest<Result>;

public record DeleteTerceroDomicilioCommand(long TerceroId, long DomicilioId) : IRequest<Result>;

public class AddTerceroDomicilioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddTerceroDomicilioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddTerceroDomicilioCommand request, CancellationToken ct)
    {
        PersonaDomicilio domicilio;
        try
        {
            domicilio = PersonaDomicilio.Crear(
                request.TerceroId,
                request.TipoDomicilioId,
                request.ProvinciaId,
                request.LocalidadId,
                request.Calle,
                request.Barrio,
                request.CodigoPostal,
                request.Observacion,
                request.Orden,
                request.EsDefecto);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.Domicilios.Add(domicilio);
        await db.SaveChangesAsync(ct);

        return Result.Success(domicilio.Id);
    }
}

public class UpdateTerceroDomicilioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTerceroDomicilioCommand, Result>
{
    public async Task<Result> Handle(UpdateTerceroDomicilioCommand request, CancellationToken ct)
    {
        var domicilio = await db.Domicilios.FirstOrDefaultAsync(
            d => d.Id == request.DomicilioId && d.PersonaId == request.TerceroId,
            ct);

        if (domicilio is null)
            return Result.Failure("Domicilio no encontrado.");

        domicilio.Actualizar(
            request.TipoDomicilioId,
            request.ProvinciaId,
            request.LocalidadId,
            request.Calle,
            request.Barrio,
            request.CodigoPostal,
            request.Observacion,
            request.Orden,
            request.EsDefecto);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteTerceroDomicilioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTerceroDomicilioCommand, Result>
{
    public async Task<Result> Handle(DeleteTerceroDomicilioCommand request, CancellationToken ct)
    {
        var domicilio = await db.Domicilios.FirstOrDefaultAsync(
            d => d.Id == request.DomicilioId && d.PersonaId == request.TerceroId,
            ct);

        if (domicilio is null)
            return Result.Failure("Domicilio no encontrado.");

        db.Domicilios.Remove(domicilio);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddTerceroMedioContactoCommand(
    long TerceroId,
    string Valor,
    long? TipoMedioContactoId,
    int Orden,
    bool EsDefecto,
    string? Observacion) : IRequest<Result<long>>;

public record UpdateTerceroMedioContactoCommand(
    long TerceroId,
    long MedioContactoId,
    string Valor,
    long? TipoMedioContactoId,
    int Orden,
    bool EsDefecto,
    string? Observacion) : IRequest<Result>;

public record DeleteTerceroMedioContactoCommand(long TerceroId, long MedioContactoId) : IRequest<Result>;

public class AddTerceroMedioContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddTerceroMedioContactoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddTerceroMedioContactoCommand request, CancellationToken ct)
    {
        MedioContacto medio;
        try
        {
            medio = MedioContacto.Crear(
                request.TerceroId,
                request.Valor,
                request.TipoMedioContactoId,
                request.Orden,
                request.EsDefecto,
                request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.MediosContacto.Add(medio);
        await db.SaveChangesAsync(ct);

        return Result.Success(medio.Id);
    }
}

public class UpdateTerceroMedioContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTerceroMedioContactoCommand, Result>
{
    public async Task<Result> Handle(UpdateTerceroMedioContactoCommand request, CancellationToken ct)
    {
        var medio = await db.MediosContacto.FirstOrDefaultAsync(
            m => m.Id == request.MedioContactoId && m.PersonaId == request.TerceroId,
            ct);

        if (medio is null)
            return Result.Failure("Medio de contacto no encontrado.");

        try
        {
            medio.Actualizar(
                request.Valor,
                request.TipoMedioContactoId,
                request.Orden,
                request.EsDefecto,
                request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteTerceroMedioContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTerceroMedioContactoCommand, Result>
{
    public async Task<Result> Handle(DeleteTerceroMedioContactoCommand request, CancellationToken ct)
    {
        var medio = await db.MediosContacto.FirstOrDefaultAsync(
            m => m.Id == request.MedioContactoId && m.PersonaId == request.TerceroId,
            ct);

        if (medio is null)
            return Result.Failure("Medio de contacto no encontrado.");

        db.MediosContacto.Remove(medio);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddTerceroTipoPersonaCommand(long TerceroId, long TipoPersonaId, string? Legajo, int? LegajoOrden)
    : IRequest<Result<long>>;

public record DeleteTerceroTipoPersonaCommand(long TerceroId, long PersonaTipoPersonaId) : IRequest<Result>;

public class AddTerceroTipoPersonaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddTerceroTipoPersonaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddTerceroTipoPersonaCommand request, CancellationToken ct)
    {
        var existe = await db.PersonasXTipoPersona.AnyAsync(
            p => p.PersonaId == request.TerceroId && p.TipoPersonaId == request.TipoPersonaId,
            ct);

        if (existe)
            return Result.Failure<long>("Este tipo de persona ya está asignado al tercero.");

        PersonaXTipoPersona asignacion;
        try
        {
            asignacion = PersonaXTipoPersona.Crear(
                request.TerceroId,
                request.TipoPersonaId,
                request.Legajo,
                request.LegajoOrden);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.PersonasXTipoPersona.Add(asignacion);
        await db.SaveChangesAsync(ct);

        return Result.Success(asignacion.Id);
    }
}

public class DeleteTerceroTipoPersonaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTerceroTipoPersonaCommand, Result>
{
    public async Task<Result> Handle(DeleteTerceroTipoPersonaCommand request, CancellationToken ct)
    {
        var asignacion = await db.PersonasXTipoPersona.FirstOrDefaultAsync(
            p => p.Id == request.PersonaTipoPersonaId && p.PersonaId == request.TerceroId,
            ct);

        if (asignacion is null)
            return Result.Failure("Tipo de persona no encontrado.");

        db.PersonasXTipoPersona.Remove(asignacion);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddTerceroVinculacionCommand(
    long TerceroId,
    long ClienteId,
    long ProveedorId,
    bool EsPredeterminado,
    long? TipoRelacionId) : IRequest<Result<long>>;

public record DeleteTerceroVinculacionCommand(long TerceroId, long VinculacionId) : IRequest<Result>;

public class AddTerceroVinculacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddTerceroVinculacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddTerceroVinculacionCommand request, CancellationToken ct)
    {
        VinculacionPersona vinculacion;
        try
        {
            vinculacion = VinculacionPersona.Crear(
                request.ClienteId,
                request.ProveedorId,
                request.EsPredeterminado,
                request.TipoRelacionId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.VinculacionesPersona.Add(vinculacion);
        await db.SaveChangesAsync(ct);

        return Result.Success(vinculacion.Id);
    }
}

public class DeleteTerceroVinculacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTerceroVinculacionCommand, Result>
{
    public async Task<Result> Handle(DeleteTerceroVinculacionCommand request, CancellationToken ct)
    {
        var vinculacion = await db.VinculacionesPersona.FirstOrDefaultAsync(
            v => v.Id == request.VinculacionId && (v.ClienteId == request.TerceroId || v.ProveedorId == request.TerceroId),
            ct);

        if (vinculacion is null)
            return Result.Failure("Vinculación no encontrada.");

        db.VinculacionesPersona.Remove(vinculacion);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddTerceroContactoCommand(long TerceroId, long PersonaContactoId, long? TipoRelacionId)
    : IRequest<Result<long>>;

public record DeleteTerceroContactoCommand(long TerceroId, long ContactoId) : IRequest<Result>;

public class AddTerceroContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddTerceroContactoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddTerceroContactoCommand request, CancellationToken ct)
    {
        Contacto contacto;
        try
        {
            contacto = Contacto.Crear(request.TerceroId, request.PersonaContactoId, request.TipoRelacionId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.Contactos.Add(contacto);
        await db.SaveChangesAsync(ct);

        return Result.Success(contacto.Id);
    }
}

public class DeleteTerceroContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTerceroContactoCommand, Result>
{
    public async Task<Result> Handle(DeleteTerceroContactoCommand request, CancellationToken ct)
    {
        var contacto = await db.Contactos.FirstOrDefaultAsync(
            c => c.Id == request.ContactoId && c.PersonaId == request.TerceroId,
            ct);

        if (contacto is null)
            return Result.Failure("Contacto no encontrado.");

        db.Contactos.Remove(contacto);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AddTerceroDomicilioCommandValidator : AbstractValidator<AddTerceroDomicilioCommand>
{
    public AddTerceroDomicilioCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
    }
}

public class UpdateTerceroDomicilioCommandValidator : AbstractValidator<UpdateTerceroDomicilioCommand>
{
    public UpdateTerceroDomicilioCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.DomicilioId).GreaterThan(0);
    }
}

public class DeleteTerceroDomicilioCommandValidator : AbstractValidator<DeleteTerceroDomicilioCommand>
{
    public DeleteTerceroDomicilioCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.DomicilioId).GreaterThan(0);
    }
}

public class AddTerceroMedioContactoCommandValidator : AbstractValidator<AddTerceroMedioContactoCommand>
{
    public AddTerceroMedioContactoCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class UpdateTerceroMedioContactoCommandValidator : AbstractValidator<UpdateTerceroMedioContactoCommand>
{
    public UpdateTerceroMedioContactoCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MedioContactoId).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class DeleteTerceroMedioContactoCommandValidator : AbstractValidator<DeleteTerceroMedioContactoCommand>
{
    public DeleteTerceroMedioContactoCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MedioContactoId).GreaterThan(0);
    }
}

public class AddTerceroTipoPersonaCommandValidator : AbstractValidator<AddTerceroTipoPersonaCommand>
{
    public AddTerceroTipoPersonaCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.TipoPersonaId).GreaterThan(0);
    }
}

public class DeleteTerceroTipoPersonaCommandValidator : AbstractValidator<DeleteTerceroTipoPersonaCommand>
{
    public DeleteTerceroTipoPersonaCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.PersonaTipoPersonaId).GreaterThan(0);
    }
}

public class AddTerceroVinculacionCommandValidator : AbstractValidator<AddTerceroVinculacionCommand>
{
    public AddTerceroVinculacionCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.ProveedorId).GreaterThan(0);
    }
}

public class DeleteTerceroVinculacionCommandValidator : AbstractValidator<DeleteTerceroVinculacionCommand>
{
    public DeleteTerceroVinculacionCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.VinculacionId).GreaterThan(0);
    }
}

public class AddTerceroContactoCommandValidator : AbstractValidator<AddTerceroContactoCommand>
{
    public AddTerceroContactoCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.PersonaContactoId).GreaterThan(0);
    }
}

public class DeleteTerceroContactoCommandValidator : AbstractValidator<DeleteTerceroContactoCommand>
{
    public DeleteTerceroContactoCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.ContactoId).GreaterThan(0);
    }
}
