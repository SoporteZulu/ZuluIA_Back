using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Geografia;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreatePaisCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;
public record UpdatePaisCommand(long Id, string Codigo, string Descripcion) : IRequest<Result>;
public record DeletePaisCommand(long Id) : IRequest<Result>;

public record CreateProvinciaCommand(long PaisId, string Codigo, string Descripcion) : IRequest<Result<long>>;
public record UpdateProvinciaCommand(long Id, long PaisId, string Codigo, string Descripcion) : IRequest<Result>;
public record DeleteProvinciaCommand(long Id) : IRequest<Result>;

public record CreateLocalidadCommand(long ProvinciaId, string Descripcion, string? CodigoPostal) : IRequest<Result<long>>;
public record UpdateLocalidadCommand(long Id, long ProvinciaId, string Descripcion, string? CodigoPostal) : IRequest<Result>;
public record DeleteLocalidadCommand(long Id) : IRequest<Result>;

public record CreateBarrioCommand(long LocalidadId, string Descripcion) : IRequest<Result<long>>;
public record UpdateBarrioCommand(long Id, long LocalidadId, string Descripcion) : IRequest<Result>;
public record DeleteBarrioCommand(long Id) : IRequest<Result>;

public class CreatePaisCommandHandler(IApplicationDbContext db) : IRequestHandler<CreatePaisCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePaisCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.Paises.AsNoTracking().AnyAsync(x => x.Codigo == codigo, ct))
            return Result.Failure<long>($"Ya existe un pais con codigo '{request.Codigo}'.");

        Pais entity;
        try { entity = Pais.Crear(codigo, request.Descripcion); }
        catch (ArgumentException ex) { return Result.Failure<long>(ex.Message); }

        db.Paises.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdatePaisCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdatePaisCommand, Result>
{
    public async Task<Result> Handle(UpdatePaisCommand request, CancellationToken ct)
    {
        var entity = await db.Paises.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Pais {request.Id} no encontrado.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.Paises.AsNoTracking().AnyAsync(x => x.Id != request.Id && x.Codigo == codigo, ct))
            return Result.Failure($"Ya existe un pais con codigo '{request.Codigo}'.");

        try { entity.Actualizar(codigo, request.Descripcion); }
        catch (ArgumentException ex) { return Result.Failure(ex.Message); }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeletePaisCommandHandler(IApplicationDbContext db) : IRequestHandler<DeletePaisCommand, Result>
{
    public async Task<Result> Handle(DeletePaisCommand request, CancellationToken ct)
    {
        var entity = await db.Paises.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Pais {request.Id} no encontrado.");
        if (await db.Provincias.AsNoTracking().AnyAsync(x => x.PaisId == request.Id, ct))
            return Result.Failure("No se puede eliminar un pais con provincias asociadas.");

        db.Paises.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateProvinciaCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateProvinciaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateProvinciaCommand request, CancellationToken ct)
    {
        if (!await db.Paises.AsNoTracking().AnyAsync(x => x.Id == request.PaisId, ct))
            return Result.Failure<long>($"Pais {request.PaisId} no encontrado.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.Provincias.AsNoTracking().AnyAsync(x => x.PaisId == request.PaisId && x.Codigo == codigo, ct))
            return Result.Failure<long>($"Ya existe una provincia con codigo '{request.Codigo}' para ese pais.");

        Provincia entity;
        try { entity = Provincia.Crear(request.PaisId, codigo, request.Descripcion); }
        catch (ArgumentException ex) { return Result.Failure<long>(ex.Message); }

        db.Provincias.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateProvinciaCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateProvinciaCommand, Result>
{
    public async Task<Result> Handle(UpdateProvinciaCommand request, CancellationToken ct)
    {
        var entity = await db.Provincias.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Provincia {request.Id} no encontrada.");
        if (!await db.Paises.AsNoTracking().AnyAsync(x => x.Id == request.PaisId, ct))
            return Result.Failure($"Pais {request.PaisId} no encontrado.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.Provincias.AsNoTracking().AnyAsync(x => x.Id != request.Id && x.PaisId == request.PaisId && x.Codigo == codigo, ct))
            return Result.Failure($"Ya existe una provincia con codigo '{request.Codigo}' para ese pais.");

        try { entity.Actualizar(request.PaisId, codigo, request.Descripcion); }
        catch (ArgumentException ex) { return Result.Failure(ex.Message); }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteProvinciaCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteProvinciaCommand, Result>
{
    public async Task<Result> Handle(DeleteProvinciaCommand request, CancellationToken ct)
    {
        var entity = await db.Provincias.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Provincia {request.Id} no encontrada.");
        if (await db.Localidades.AsNoTracking().AnyAsync(x => x.ProvinciaId == request.Id, ct))
            return Result.Failure("No se puede eliminar una provincia con localidades asociadas.");

        db.Provincias.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateLocalidadCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateLocalidadCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateLocalidadCommand request, CancellationToken ct)
    {
        if (!await db.Provincias.AsNoTracking().AnyAsync(x => x.Id == request.ProvinciaId, ct))
            return Result.Failure<long>($"Provincia {request.ProvinciaId} no encontrada.");

        if (await db.Localidades.AsNoTracking().AnyAsync(x => x.ProvinciaId == request.ProvinciaId && x.Descripcion == request.Descripcion.Trim(), ct))
            return Result.Failure<long>("Ya existe una localidad con esa descripcion para la provincia indicada.");

        Localidad entity;
        try { entity = Localidad.Crear(request.ProvinciaId, request.Descripcion, request.CodigoPostal); }
        catch (ArgumentException ex) { return Result.Failure<long>(ex.Message); }

        db.Localidades.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateLocalidadCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateLocalidadCommand, Result>
{
    public async Task<Result> Handle(UpdateLocalidadCommand request, CancellationToken ct)
    {
        var entity = await db.Localidades.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Localidad {request.Id} no encontrada.");
        if (!await db.Provincias.AsNoTracking().AnyAsync(x => x.Id == request.ProvinciaId, ct))
            return Result.Failure($"Provincia {request.ProvinciaId} no encontrada.");
        if (await db.Localidades.AsNoTracking().AnyAsync(x => x.Id != request.Id && x.ProvinciaId == request.ProvinciaId && x.Descripcion == request.Descripcion.Trim(), ct))
            return Result.Failure("Ya existe una localidad con esa descripcion para la provincia indicada.");

        try { entity.Actualizar(request.ProvinciaId, request.Descripcion, request.CodigoPostal); }
        catch (ArgumentException ex) { return Result.Failure(ex.Message); }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteLocalidadCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteLocalidadCommand, Result>
{
    public async Task<Result> Handle(DeleteLocalidadCommand request, CancellationToken ct)
    {
        var entity = await db.Localidades.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Localidad {request.Id} no encontrada.");
        if (await db.Barrios.AsNoTracking().AnyAsync(x => x.LocalidadId == request.Id, ct))
            return Result.Failure("No se puede eliminar una localidad con barrios asociados.");

        db.Localidades.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateBarrioCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateBarrioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateBarrioCommand request, CancellationToken ct)
    {
        if (!await db.Localidades.AsNoTracking().AnyAsync(x => x.Id == request.LocalidadId, ct))
            return Result.Failure<long>($"Localidad {request.LocalidadId} no encontrada.");
        if (await db.Barrios.AsNoTracking().AnyAsync(x => x.LocalidadId == request.LocalidadId && x.Descripcion == request.Descripcion.Trim(), ct))
            return Result.Failure<long>("Ya existe un barrio con esa descripcion para la localidad indicada.");

        Barrio entity;
        try { entity = Barrio.Crear(request.LocalidadId, request.Descripcion); }
        catch (ArgumentException ex) { return Result.Failure<long>(ex.Message); }

        db.Barrios.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateBarrioCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateBarrioCommand, Result>
{
    public async Task<Result> Handle(UpdateBarrioCommand request, CancellationToken ct)
    {
        var entity = await db.Barrios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Barrio {request.Id} no encontrado.");
        if (!await db.Localidades.AsNoTracking().AnyAsync(x => x.Id == request.LocalidadId, ct))
            return Result.Failure($"Localidad {request.LocalidadId} no encontrada.");
        if (await db.Barrios.AsNoTracking().AnyAsync(x => x.Id != request.Id && x.LocalidadId == request.LocalidadId && x.Descripcion == request.Descripcion.Trim(), ct))
            return Result.Failure("Ya existe un barrio con esa descripcion para la localidad indicada.");

        try { entity.Actualizar(request.LocalidadId, request.Descripcion); }
        catch (ArgumentException ex) { return Result.Failure(ex.Message); }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteBarrioCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteBarrioCommand, Result>
{
    public async Task<Result> Handle(DeleteBarrioCommand request, CancellationToken ct)
    {
        var entity = await db.Barrios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Barrio {request.Id} no encontrado.");

        db.Barrios.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreatePaisCommandValidator : AbstractValidator<CreatePaisCommand>
{
    public CreatePaisCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdatePaisCommandValidator : AbstractValidator<UpdatePaisCommand>
{
    public UpdatePaisCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeletePaisCommandValidator : AbstractValidator<DeletePaisCommand>
{
    public DeletePaisCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateProvinciaCommandValidator : AbstractValidator<CreateProvinciaCommand>
{
    public CreateProvinciaCommandValidator()
    {
        RuleFor(x => x.PaisId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateProvinciaCommandValidator : AbstractValidator<UpdateProvinciaCommand>
{
    public UpdateProvinciaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.PaisId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteProvinciaCommandValidator : AbstractValidator<DeleteProvinciaCommand>
{
    public DeleteProvinciaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateLocalidadCommandValidator : AbstractValidator<CreateLocalidadCommand>
{
    public CreateLocalidadCommandValidator()
    {
        RuleFor(x => x.ProvinciaId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateLocalidadCommandValidator : AbstractValidator<UpdateLocalidadCommand>
{
    public UpdateLocalidadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ProvinciaId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteLocalidadCommandValidator : AbstractValidator<DeleteLocalidadCommand>
{
    public DeleteLocalidadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateBarrioCommandValidator : AbstractValidator<CreateBarrioCommand>
{
    public CreateBarrioCommandValidator()
    {
        RuleFor(x => x.LocalidadId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateBarrioCommandValidator : AbstractValidator<UpdateBarrioCommand>
{
    public UpdateBarrioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.LocalidadId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteBarrioCommandValidator : AbstractValidator<DeleteBarrioCommand>
{
    public DeleteBarrioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}