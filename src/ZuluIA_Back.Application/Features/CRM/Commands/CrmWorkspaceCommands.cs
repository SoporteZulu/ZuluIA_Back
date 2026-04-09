using FluentValidation;
using MediatR;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.CRM;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.CRM.Commands;

public record CreateCrmClienteCommand(
    string Nombre,
    string TipoCliente,
    string Segmento,
    string? Industria,
    string? Cuit,
    string Pais,
    string? Provincia,
    string? Ciudad,
    string? Direccion,
    string? TelefonoPrincipal,
    string? EmailPrincipal,
    string? SitioWeb,
    string OrigenCliente,
    string EstadoRelacion,
    long? ResponsableId,
    DateOnly? FechaAlta,
    string? NotasGenerales) : IRequest<Result<long>>;

public record UpdateCrmClienteCommand(
    long Id,
    string Nombre,
    string TipoCliente,
    string Segmento,
    string? Industria,
    string? Cuit,
    string Pais,
    string? Provincia,
    string? Ciudad,
    string? Direccion,
    string? TelefonoPrincipal,
    string? EmailPrincipal,
    string? SitioWeb,
    string OrigenCliente,
    string EstadoRelacion,
    long? ResponsableId,
    DateOnly? FechaAlta,
    string? NotasGenerales) : IRequest<Result>;

public record DeleteCrmClienteCommand(long Id) : IRequest<Result>;

public record CreateCrmContactoCommand(
    long ClienteId,
    string Nombre,
    string Apellido,
    string? Cargo,
    string? Email,
    string? Telefono,
    string CanalPreferido,
    string EstadoContacto,
    string? Notas) : IRequest<Result<long>>;

public record UpdateCrmContactoCommand(
    long Id,
    long ClienteId,
    string Nombre,
    string Apellido,
    string? Cargo,
    string? Email,
    string? Telefono,
    string CanalPreferido,
    string EstadoContacto,
    string? Notas) : IRequest<Result>;

public record DeleteCrmContactoCommand(long Id) : IRequest<Result>;

public record CreateCrmOportunidadCommand(
    long ClienteId,
    long? ContactoPrincipalId,
    string Titulo,
    string Etapa,
    int Probabilidad,
    decimal MontoEstimado,
    string Moneda,
    DateOnly FechaApertura,
    DateOnly? FechaEstimadaCierre,
    long? ResponsableId,
    string Origen,
    string? MotivoPerdida,
    string? Notas) : IRequest<Result<long>>;

public record UpdateCrmOportunidadCommand(
    long Id,
    long ClienteId,
    long? ContactoPrincipalId,
    string Titulo,
    string Etapa,
    int Probabilidad,
    decimal MontoEstimado,
    string Moneda,
    DateOnly FechaApertura,
    DateOnly? FechaEstimadaCierre,
    long? ResponsableId,
    string Origen,
    string? MotivoPerdida,
    string? Notas) : IRequest<Result>;

public record DeleteCrmOportunidadCommand(long Id) : IRequest<Result>;
public record CloseCrmOportunidadGanadaCommand(long Id) : IRequest<Result>;
public record CloseCrmOportunidadPerdidaCommand(long Id, string MotivoPerdida) : IRequest<Result>;
public record ReassignCrmOportunidadCommand(long Id, long ResponsableId) : IRequest<Result>;

public record CreateCrmInteraccionCommand(
    long ClienteId,
    long? ContactoId,
    long? OportunidadId,
    string TipoInteraccion,
    string Canal,
    DateTimeOffset FechaHora,
    long UsuarioResponsableId,
    string Resultado,
    string? Descripcion,
    IReadOnlyList<string>? Adjuntos) : IRequest<Result<long>>;

public record UpdateCrmInteraccionCommand(
    long Id,
    long ClienteId,
    long? ContactoId,
    long? OportunidadId,
    string TipoInteraccion,
    string Canal,
    DateTimeOffset FechaHora,
    long UsuarioResponsableId,
    string Resultado,
    string? Descripcion,
    IReadOnlyList<string>? Adjuntos) : IRequest<Result>;

public record DeleteCrmInteraccionCommand(long Id) : IRequest<Result>;

public record CreateCrmTareaCommand(
    long? ClienteId,
    long? OportunidadId,
    long AsignadoAId,
    string Titulo,
    string? Descripcion,
    string TipoTarea,
    DateOnly FechaVencimiento,
    string Prioridad,
    string Estado,
    DateOnly? FechaCompletado) : IRequest<Result<long>>;

public record UpdateCrmTareaCommand(
    long Id,
    long? ClienteId,
    long? OportunidadId,
    long AsignadoAId,
    string Titulo,
    string? Descripcion,
    string TipoTarea,
    DateOnly FechaVencimiento,
    string Prioridad,
    string Estado,
    DateOnly? FechaCompletado) : IRequest<Result>;

public record DeleteCrmTareaCommand(long Id) : IRequest<Result>;
public record CompleteCrmTareaCommand(long Id, DateOnly? FechaCompletado) : IRequest<Result>;
public record ReopenCrmTareaCommand(long Id) : IRequest<Result>;

public record CreateCrmSegmentoCommand(
    string Nombre,
    string? Descripcion,
    string CriteriosJson,
    string TipoSegmento) : IRequest<Result<long>>;

public record UpdateCrmSegmentoCommand(
    long Id,
    string Nombre,
    string? Descripcion,
    string CriteriosJson,
    string TipoSegmento) : IRequest<Result>;

public record DeleteCrmSegmentoCommand(long Id) : IRequest<Result>;
public record AddCrmSegmentoClienteCommand(long SegmentoId, long ClienteId) : IRequest<Result>;
public record RemoveCrmSegmentoClienteCommand(long SegmentoId, long ClienteId) : IRequest<Result>;

public record CreateCrmUsuarioCommand(
    string Nombre,
    string Apellido,
    string Email,
    string Rol,
    string Estado,
    string? Avatar) : IRequest<Result<long>>;

public record UpdateCrmUsuarioCommand(
    long Id,
    string Nombre,
    string Apellido,
    string Email,
    string Rol,
    string Estado,
    string? Avatar) : IRequest<Result>;

public record DeleteCrmUsuarioCommand(long Id) : IRequest<Result>;

internal static class CrmCommandPersistenceHelper
{
    public static async Task<long> ResolveTipoDocumentoIdAsync(IApplicationDbContext db, string? cuit, CancellationToken ct)
    {
        var query = db.TiposDocumento.AsNoTracking();
        var preferred = !string.IsNullOrWhiteSpace(cuit)
            ? await query
                .Where(x => x.Descripcion.ToLower().Contains("cuit") || x.Descripcion.ToLower().Contains("ruc"))
                .Select(x => (long?)x.Id)
                .FirstOrDefaultAsync(ct)
            : await query
                .Where(x => x.Descripcion.ToLower().Contains("dni") || x.Descripcion.ToLower().Contains("cedula"))
                .Select(x => (long?)x.Id)
                .FirstOrDefaultAsync(ct);

        if (preferred.HasValue)
            return preferred.Value;

        var fallback = await query.OrderBy(x => x.Id).Select(x => (long?)x.Id).FirstOrDefaultAsync(ct);
        return fallback ?? throw new InvalidOperationException("No existe un tipo de documento configurado para CRM.");
    }

    public static async Task<long> ResolveCondicionIvaIdAsync(IApplicationDbContext db, CancellationToken ct)
    {
        var query = db.CondicionesIva.AsNoTracking();
        var preferred = await query
            .Where(x => x.Descripcion.ToLower().Contains("consumidor") || x.Descripcion.ToLower().Contains("final"))
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(ct);

        if (preferred.HasValue)
            return preferred.Value;

        var fallback = await query.OrderBy(x => x.Id).Select(x => (long?)x.Id).FirstOrDefaultAsync(ct);
        return fallback ?? throw new InvalidOperationException("No existe una condición IVA configurada para CRM.");
    }

    public static string GenerateLegajo() => $"CRM{DateTime.UtcNow:yyMMddHHmmssfff}";

    public static string BuildTemporaryDocument(string? cuit)
        => string.IsNullOrWhiteSpace(cuit)
            ? $"CRM-{DateTime.UtcNow.Ticks}"
            : cuit.Trim();

    public static Domicilio BuildDomicilio(string? direccion)
        => string.IsNullOrWhiteSpace(direccion)
            ? Domicilio.Vacio()
            : Domicilio.Crear(direccion.Trim(), null, null, null, null, null, null);

    public static string SerializeList(IReadOnlyList<string>? values)
        => System.Text.Json.JsonSerializer.Serialize(values ?? Array.Empty<string>());

    public static string BuildNombreCompleto(string nombre, string apellido)
        => string.Join(' ', new[] { nombre?.Trim(), apellido?.Trim() }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public static string BuildUserName(string email, string nombre, string apellido)
    {
        var baseValue = !string.IsNullOrWhiteSpace(email)
            ? email.Split('@')[0]
            : BuildNombreCompleto(nombre, apellido).Replace(' ', '.');

        var sanitized = new string(baseValue
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-' ? ch : '.')
            .ToArray())
            .Trim('.');

        return string.IsNullOrWhiteSpace(sanitized) ? $"crm.user.{DateTime.UtcNow.Ticks}" : sanitized;
    }

    public static bool IsActivo(string estado)
        => string.Equals(estado?.Trim(), "activo", StringComparison.OrdinalIgnoreCase);
}

public class CreateCrmClienteCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCrmClienteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmClienteCommand request, CancellationToken ct)
    {
        try
        {
            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.ResponsableId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure<long>(responsableValidation);

            var tercero = await FindExistingTerceroAsync(request, ct);
            if (tercero is null)
            {
                tercero = await CreateTerceroAsync(request, ct);
                db.Terceros.Add(tercero);
                await db.SaveChangesAsync(ct);
            }

            var exists = await db.CrmClientes.AnyAsync(x => x.TerceroId == tercero.Id, ct);
            if (exists)
                return Result.Failure<long>("El tercero ya está registrado como cliente CRM.");

            UpdateTerceroData(tercero, request);

            var perfil = CrmCliente.Crear(
                tercero.Id,
                request.TipoCliente,
                request.Segmento,
                request.Industria,
                request.Pais,
                request.Provincia,
                request.Ciudad,
                request.Direccion,
                request.OrigenCliente,
                request.EstadoRelacion,
                request.ResponsableId,
                request.NotasGenerales,
                userId: null);

            db.CrmClientes.Add(perfil);
            await db.SaveChangesAsync(ct);
            return Result.Success(tercero.Id);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }
        catch (DbUpdateException ex) when (CrmLocalSchemaCompatibility.TryTranslate(ex, out var schemaError))
        {
            return Result.Failure<long>(schemaError);
        }
    }

    private async Task<Tercero?> FindExistingTerceroAsync(CreateCrmClienteCommand request, CancellationToken ct)
    {
        var cuit = request.Cuit?.Trim();
        if (!string.IsNullOrWhiteSpace(cuit))
        {
            return await db.Terceros.FirstOrDefaultAsync(x => x.NroDocumento == cuit, ct);
        }

        return await db.Terceros.FirstOrDefaultAsync(x => x.RazonSocial == request.Nombre.Trim(), ct);
    }

    private async Task<Tercero> CreateTerceroAsync(CreateCrmClienteCommand request, CancellationToken ct)
    {
        var tipoDocumentoId = await CrmCommandPersistenceHelper.ResolveTipoDocumentoIdAsync(db, request.Cuit, ct);
        var condicionIvaId = await CrmCommandPersistenceHelper.ResolveCondicionIvaIdAsync(db, ct);

        var tercero = Tercero.Crear(
            CrmCommandPersistenceHelper.GenerateLegajo(),
            request.Nombre,
            tipoDocumentoId,
            CrmCommandPersistenceHelper.BuildTemporaryDocument(request.Cuit),
            condicionIvaId,
            esCliente: true,
            esProveedor: false,
            esEmpleado: false,
            sucursalId: null,
            userId: null);

        UpdateTerceroData(tercero, request);
        return tercero;
    }

    private static void UpdateTerceroData(Tercero tercero, CreateCrmClienteCommand request)
    {
        tercero.Actualizar(
            request.Nombre,
            request.Nombre,
            tercero.CondicionIvaId,
            request.TelefonoPrincipal,
            null,
            request.EmailPrincipal,
            request.SitioWeb,
            CrmCommandPersistenceHelper.BuildDomicilio(request.Direccion),
            null,
            null,
            null,
            null,
            null,
            null,
            facturable: true,
            cobradorId: null,
            aplicaComisionCobrador: false,
            pctComisionCobrador: 0m,
            vendedorId: request.ResponsableId,
            aplicaComisionVendedor: request.ResponsableId.HasValue,
            pctComisionVendedor: 0m,
            observacion: request.NotasGenerales,
            userId: null);
    }
}

public class UpdateCrmClienteCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrmClienteCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmClienteCommand request, CancellationToken ct)
    {
        try
        {
            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.ResponsableId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure(responsableValidation);

            var tercero = await db.Terceros.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            var perfil = await db.CrmClientes.FirstOrDefaultAsync(x => x.TerceroId == request.Id, ct);
            if (tercero is null || perfil is null)
                return Result.Failure($"Cliente CRM {request.Id} no encontrado.");

            tercero.Actualizar(
                request.Nombre,
                request.Nombre,
                tercero.CondicionIvaId,
                request.TelefonoPrincipal,
                null,
                request.EmailPrincipal,
                request.SitioWeb,
                CrmCommandPersistenceHelper.BuildDomicilio(request.Direccion),
                null,
                null,
                null,
                null,
                null,
                null,
                facturable: true,
                cobradorId: null,
                aplicaComisionCobrador: false,
                pctComisionCobrador: 0m,
                vendedorId: request.ResponsableId,
                aplicaComisionVendedor: request.ResponsableId.HasValue,
                pctComisionVendedor: 0m,
                observacion: request.NotasGenerales,
                userId: null);

            perfil.Actualizar(
                request.TipoCliente,
                request.Segmento,
                request.Industria,
                request.Pais,
                request.Provincia,
                request.Ciudad,
                request.Direccion,
                request.OrigenCliente,
                request.EstadoRelacion,
                request.ResponsableId,
                request.NotasGenerales,
                userId: null);

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
        catch (DbUpdateException ex) when (CrmLocalSchemaCompatibility.TryTranslate(ex, out var schemaError))
        {
            return Result.Failure(schemaError);
        }
    }
}

public class DeleteCrmClienteCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCrmClienteCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmClienteCommand request, CancellationToken ct)
    {
        var perfil = await db.CrmClientes.FirstOrDefaultAsync(x => x.TerceroId == request.Id, ct);
        if (perfil is null)
            return Result.Failure($"Cliente CRM {request.Id} no encontrado.");

        var contactos = await db.CrmContactos.Where(x => x.ClienteId == request.Id && x.Activo).ToListAsync(ct);
        var oportunidades = await db.CrmOportunidades.Where(x => x.ClienteId == request.Id && x.Activa).ToListAsync(ct);
        var oportunidadIds = oportunidades.Select(x => x.Id).ToHashSet();
        var tareas = await db.CrmTareas
            .Where(x => x.Activa && (x.ClienteId == request.Id || (x.OportunidadId.HasValue && oportunidadIds.Contains(x.OportunidadId.Value))))
            .ToListAsync(ct);
        var interacciones = await db.CrmInteracciones.Where(x => x.ClienteId == request.Id).ToListAsync(ct);

        foreach (var contacto in contactos)
            contacto.Desactivar(userId: null);

        foreach (var oportunidad in oportunidades)
            oportunidad.MarcarEliminada(userId: null);

        foreach (var tarea in tareas)
            tarea.MarcarEliminada(userId: null);

        foreach (var interaccion in interacciones)
            db.CrmInteracciones.Remove(interaccion);

        perfil.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CloseCrmOportunidadGanadaCommandHandler(IApplicationDbContext db) : IRequestHandler<CloseCrmOportunidadGanadaCommand, Result>
{
    public async Task<Result> Handle(CloseCrmOportunidadGanadaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = await db.CrmOportunidades.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Oportunidad CRM {request.Id} no encontrada.");

            entity.CerrarGanada(userId: null);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class CloseCrmOportunidadPerdidaCommandHandler(IApplicationDbContext db) : IRequestHandler<CloseCrmOportunidadPerdidaCommand, Result>
{
    public async Task<Result> Handle(CloseCrmOportunidadPerdidaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = await db.CrmOportunidades.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Oportunidad CRM {request.Id} no encontrada.");

            entity.CerrarPerdida(request.MotivoPerdida, userId: null);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class ReassignCrmOportunidadCommandHandler(IApplicationDbContext db) : IRequestHandler<ReassignCrmOportunidadCommand, Result>
{
    public async Task<Result> Handle(ReassignCrmOportunidadCommand request, CancellationToken ct)
    {
        try
        {
            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.ResponsableId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure(responsableValidation);

            var entity = await db.CrmOportunidades.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Oportunidad CRM {request.Id} no encontrada.");

            entity.ReasignarResponsable(request.ResponsableId, userId: null);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class CreateCrmContactoCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCrmContactoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmContactoCommand request, CancellationToken ct)
    {
        try
        {
            var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId, ct);
            if (clienteValidation is not null)
                return Result.Failure<long>(clienteValidation);

            var entity = CrmContacto.Crear(
                request.ClienteId,
                request.Nombre,
                request.Apellido,
                request.Cargo,
                request.Email,
                request.Telefono,
                request.CanalPreferido,
                request.EstadoContacto,
                request.Notas,
                userId: null);

            db.CrmContactos.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateCrmContactoCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrmContactoCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmContactoCommand request, CancellationToken ct)
    {
        try
        {
            var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId, ct);
            if (clienteValidation is not null)
                return Result.Failure(clienteValidation);

            var entity = await db.CrmContactos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Contacto CRM {request.Id} no encontrado.");

            entity.Actualizar(
                request.ClienteId,
                request.Nombre,
                request.Apellido,
                request.Cargo,
                request.Email,
                request.Telefono,
                request.CanalPreferido,
                request.EstadoContacto,
                request.Notas,
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

public class DeleteCrmContactoCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCrmContactoCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmContactoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmContactos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Contacto CRM {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CompleteCrmTareaCommandHandler(IApplicationDbContext db) : IRequestHandler<CompleteCrmTareaCommand, Result>
{
    public async Task<Result> Handle(CompleteCrmTareaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = await db.CrmTareas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Tarea CRM {request.Id} no encontrada.");

            entity.Completar(request.FechaCompletado, userId: null);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class ReopenCrmTareaCommandHandler(IApplicationDbContext db) : IRequestHandler<ReopenCrmTareaCommand, Result>
{
    public async Task<Result> Handle(ReopenCrmTareaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = await db.CrmTareas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Tarea CRM {request.Id} no encontrada.");

            entity.Reabrir(userId: null);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class CreateCrmOportunidadCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCrmOportunidadCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmOportunidadCommand request, CancellationToken ct)
    {
        try
        {
            var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId, ct);
            if (clienteValidation is not null)
                return Result.Failure<long>(clienteValidation);

            var contactoValidation = await CrmWorkspaceReferenceValidation.ValidateContactoAsync(db, request.ClienteId, request.ContactoPrincipalId, ct);
            if (contactoValidation is not null)
                return Result.Failure<long>(contactoValidation);

            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.ResponsableId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure<long>(responsableValidation);

            var entity = CrmOportunidad.Crear(
                request.ClienteId,
                request.ContactoPrincipalId,
                request.Titulo,
                request.Etapa,
                request.Probabilidad,
                request.MontoEstimado,
                request.Moneda,
                request.FechaApertura,
                request.FechaEstimadaCierre,
                request.ResponsableId,
                request.Origen,
                request.MotivoPerdida,
                request.Notas,
                userId: null);

            db.CrmOportunidades.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateCrmOportunidadCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrmOportunidadCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmOportunidadCommand request, CancellationToken ct)
    {
        try
        {
            var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId, ct);
            if (clienteValidation is not null)
                return Result.Failure(clienteValidation);

            var contactoValidation = await CrmWorkspaceReferenceValidation.ValidateContactoAsync(db, request.ClienteId, request.ContactoPrincipalId, ct);
            if (contactoValidation is not null)
                return Result.Failure(contactoValidation);

            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.ResponsableId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure(responsableValidation);

            var entity = await db.CrmOportunidades.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Oportunidad CRM {request.Id} no encontrada.");

            entity.Actualizar(
                request.ClienteId,
                request.ContactoPrincipalId,
                request.Titulo,
                request.Etapa,
                request.Probabilidad,
                request.MontoEstimado,
                request.Moneda,
                request.FechaApertura,
                request.FechaEstimadaCierre,
                request.ResponsableId,
                request.Origen,
                request.MotivoPerdida,
                request.Notas,
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

public class DeleteCrmOportunidadCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCrmOportunidadCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmOportunidadCommand request, CancellationToken ct)
    {
        var entity = await db.CrmOportunidades.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Oportunidad CRM {request.Id} no encontrada.");

        entity.MarcarEliminada(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmInteraccionCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCrmInteraccionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmInteraccionCommand request, CancellationToken ct)
    {
        try
        {
            var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId, ct);
            if (clienteValidation is not null)
                return Result.Failure<long>(clienteValidation);

            var contactoValidation = await CrmWorkspaceReferenceValidation.ValidateContactoAsync(db, request.ClienteId, request.ContactoId, ct);
            if (contactoValidation is not null)
                return Result.Failure<long>(contactoValidation);

            var oportunidadValidation = await CrmWorkspaceReferenceValidation.ValidateOportunidadAsync(db, request.ClienteId, request.OportunidadId, ct);
            if (oportunidadValidation is not null)
                return Result.Failure<long>(oportunidadValidation);

            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.UsuarioResponsableId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure<long>(responsableValidation);

            var entity = CrmInteraccion.Crear(
                request.ClienteId,
                request.ContactoId,
                request.OportunidadId,
                request.TipoInteraccion,
                request.Canal,
                request.FechaHora,
                request.UsuarioResponsableId,
                request.Resultado,
                request.Descripcion,
                CrmCommandPersistenceHelper.SerializeList(request.Adjuntos),
                userId: null);

            db.CrmInteracciones.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateCrmInteraccionCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrmInteraccionCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmInteraccionCommand request, CancellationToken ct)
    {
        try
        {
            var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId, ct);
            if (clienteValidation is not null)
                return Result.Failure(clienteValidation);

            var contactoValidation = await CrmWorkspaceReferenceValidation.ValidateContactoAsync(db, request.ClienteId, request.ContactoId, ct);
            if (contactoValidation is not null)
                return Result.Failure(contactoValidation);

            var oportunidadValidation = await CrmWorkspaceReferenceValidation.ValidateOportunidadAsync(db, request.ClienteId, request.OportunidadId, ct);
            if (oportunidadValidation is not null)
                return Result.Failure(oportunidadValidation);

            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.UsuarioResponsableId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure(responsableValidation);

            var entity = await db.CrmInteracciones.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Interacción CRM {request.Id} no encontrada.");

            entity.Actualizar(
                request.ClienteId,
                request.ContactoId,
                request.OportunidadId,
                request.TipoInteraccion,
                request.Canal,
                request.FechaHora,
                request.UsuarioResponsableId,
                request.Resultado,
                request.Descripcion,
                CrmCommandPersistenceHelper.SerializeList(request.Adjuntos),
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

public class DeleteCrmInteraccionCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCrmInteraccionCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmInteraccionCommand request, CancellationToken ct)
    {
        var entity = await db.CrmInteracciones.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Interacción CRM {request.Id} no encontrada.");

        db.CrmInteracciones.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmTareaCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCrmTareaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmTareaCommand request, CancellationToken ct)
    {
        try
        {
            if (request.ClienteId.HasValue)
            {
                var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId.Value, ct);
                if (clienteValidation is not null)
                    return Result.Failure<long>(clienteValidation);
            }

            var oportunidadValidation = await CrmWorkspaceReferenceValidation.ValidateOportunidadAsync(db, request.ClienteId, request.OportunidadId, ct);
            if (oportunidadValidation is not null)
                return Result.Failure<long>(oportunidadValidation);

            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.AsignadoAId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure<long>(responsableValidation);

            var entity = CrmTarea.Crear(
                request.ClienteId,
                request.OportunidadId,
                request.AsignadoAId,
                request.Titulo,
                request.Descripcion,
                request.TipoTarea,
                request.FechaVencimiento,
                request.Prioridad,
                request.Estado,
                request.FechaCompletado,
                userId: null);

            db.CrmTareas.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateCrmTareaCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrmTareaCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmTareaCommand request, CancellationToken ct)
    {
        try
        {
            if (request.ClienteId.HasValue)
            {
                var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId.Value, ct);
                if (clienteValidation is not null)
                    return Result.Failure(clienteValidation);
            }

            var oportunidadValidation = await CrmWorkspaceReferenceValidation.ValidateOportunidadAsync(db, request.ClienteId, request.OportunidadId, ct);
            if (oportunidadValidation is not null)
                return Result.Failure(oportunidadValidation);

            var responsableValidation = await CrmWorkspaceReferenceValidation.ValidateUsuarioActivoAsync(db, request.AsignadoAId, "responsable CRM", ct);
            if (responsableValidation is not null)
                return Result.Failure(responsableValidation);

            var entity = await db.CrmTareas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                return Result.Failure($"Tarea CRM {request.Id} no encontrada.");

            entity.Actualizar(
                request.ClienteId,
                request.OportunidadId,
                request.AsignadoAId,
                request.Titulo,
                request.Descripcion,
                request.TipoTarea,
                request.FechaVencimiento,
                request.Prioridad,
                request.Estado,
                request.FechaCompletado,
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

public class DeleteCrmTareaCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCrmTareaCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmTareaCommand request, CancellationToken ct)
    {
        var entity = await db.CrmTareas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarea CRM {request.Id} no encontrada.");

        entity.MarcarEliminada(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmSegmentoCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCrmSegmentoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmSegmentoCommand request, CancellationToken ct)
    {
        if (!CrmDomainRules.TryValidateSegmentDefinition(request.TipoSegmento, request.CriteriosJson, out var criteriaError))
            return Result.Failure<long>(criteriaError!);

        var nombre = request.Nombre.Trim();
        var exists = await db.CrmSegmentos.AnyAsync(x => x.Nombre == nombre, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un segmento CRM con ese nombre.");

        var entity = CrmSegmento.Crear(nombre, request.Descripcion, request.CriteriosJson, request.TipoSegmento, userId: null);
        db.CrmSegmentos.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateCrmSegmentoCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrmSegmentoCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmSegmentoCommand request, CancellationToken ct)
    {
        if (!CrmDomainRules.TryValidateSegmentDefinition(request.TipoSegmento, request.CriteriosJson, out var criteriaError))
            return Result.Failure(criteriaError!);

        var entity = await db.CrmSegmentos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Segmento CRM {request.Id} no encontrado.");

        var exists = await db.CrmSegmentos.AnyAsync(x => x.Id != request.Id && x.Nombre == request.Nombre.Trim(), ct);
        if (exists)
            return Result.Failure("Ya existe un segmento CRM con ese nombre.");

        if (string.Equals(request.TipoSegmento, "dinamico", StringComparison.OrdinalIgnoreCase))
        {
            var miembros = await db.CrmSegmentosMiembros
                .Where(x => x.SegmentoId == request.Id && x.Activo)
                .ToListAsync(ct);

            foreach (var miembro in miembros)
                miembro.Desactivar(userId: null);
        }

        entity.Actualizar(request.Nombre, request.Descripcion, request.CriteriosJson, request.TipoSegmento, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteCrmSegmentoCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCrmSegmentoCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmSegmentoCommand request, CancellationToken ct)
    {
        var entity = await db.CrmSegmentos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Segmento CRM {request.Id} no encontrado.");

        var miembros = await db.CrmSegmentosMiembros
            .Where(x => x.SegmentoId == request.Id && x.Activo)
            .ToListAsync(ct);

        foreach (var miembro in miembros)
            miembro.Desactivar(userId: null);

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AddCrmSegmentoClienteCommandHandler(IApplicationDbContext db) : IRequestHandler<AddCrmSegmentoClienteCommand, Result>
{
    public async Task<Result> Handle(AddCrmSegmentoClienteCommand request, CancellationToken ct)
    {
        var segmento = await db.CrmSegmentos.FirstOrDefaultAsync(x => x.Id == request.SegmentoId && x.Activo, ct);
        if (segmento is null)
            return Result.Failure($"Segmento CRM {request.SegmentoId} no encontrado.");

        if (!segmento.EsEstatico())
            return Result.Failure("Solo los segmentos CRM estáticos admiten membresía manual.");

        var clienteValidation = await CrmWorkspaceReferenceValidation.ValidateClienteAsync(db, request.ClienteId, ct);
        if (clienteValidation is not null)
            return Result.Failure(clienteValidation);

        var miembro = await db.CrmSegmentosMiembros.FirstOrDefaultAsync(
            x => x.SegmentoId == request.SegmentoId && x.ClienteId == request.ClienteId,
            ct);

        if (miembro is null)
        {
            db.CrmSegmentosMiembros.Add(CrmSegmentoMiembro.Crear(request.SegmentoId, request.ClienteId, userId: null));
        }
        else if (!miembro.Activo)
        {
            miembro.Reactivar(userId: null);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class RemoveCrmSegmentoClienteCommandHandler(IApplicationDbContext db) : IRequestHandler<RemoveCrmSegmentoClienteCommand, Result>
{
    public async Task<Result> Handle(RemoveCrmSegmentoClienteCommand request, CancellationToken ct)
    {
        var segmento = await db.CrmSegmentos.FirstOrDefaultAsync(x => x.Id == request.SegmentoId && x.Activo, ct);
        if (segmento is null)
            return Result.Failure($"Segmento CRM {request.SegmentoId} no encontrado.");

        if (!segmento.EsEstatico())
            return Result.Failure("Solo los segmentos CRM estáticos admiten membresía manual.");

        var miembro = await db.CrmSegmentosMiembros.FirstOrDefaultAsync(
            x => x.SegmentoId == request.SegmentoId && x.ClienteId == request.ClienteId && x.Activo,
            ct);
        if (miembro is null)
            return Result.Failure($"El cliente CRM {request.ClienteId} no pertenece manualmente al segmento {request.SegmentoId}.");

        miembro.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmUsuarioCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCrmUsuarioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCrmUsuarioCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await db.Usuarios.AnyAsync(x => x.Email == email, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un usuario con ese email.");

        var userName = CrmCommandPersistenceHelper.BuildUserName(email, request.Nombre, request.Apellido);
        var duplicatedUserName = await db.Usuarios.AnyAsync(x => x.UserName == userName, ct);
        if (duplicatedUserName)
            userName = $"{userName}.{DateTime.UtcNow:HHmmss}";

        var usuario = Usuario.Crear(
            userName,
            CrmCommandPersistenceHelper.BuildNombreCompleto(request.Nombre, request.Apellido),
            email,
            supabaseUserId: null,
            userId: null);

        if (!CrmCommandPersistenceHelper.IsActivo(request.Estado))
            usuario.Desactivar(userId: null);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        var perfil = CrmUsuarioPerfil.Crear(usuario.Id, request.Rol, request.Avatar, userId: null);
        if (!CrmCommandPersistenceHelper.IsActivo(request.Estado))
            perfil.Actualizar(request.Rol, request.Avatar, activo: false, userId: null);

        db.CrmUsuariosPerfiles.Add(perfil);
        await db.SaveChangesAsync(ct);
        return Result.Success(usuario.Id);
    }
}

public class UpdateCrmUsuarioCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrmUsuarioCommand, Result>
{
    public async Task<Result> Handle(UpdateCrmUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (usuario is null)
            return Result.Failure($"Usuario CRM {request.Id} no encontrado.");

        var email = request.Email.Trim().ToLowerInvariant();
        var duplicatedEmail = await db.Usuarios.AnyAsync(x => x.Id != request.Id && x.Email == email, ct);
        if (duplicatedEmail)
            return Result.Failure("Ya existe un usuario con ese email.");

        usuario.Actualizar(CrmCommandPersistenceHelper.BuildNombreCompleto(request.Nombre, request.Apellido), email, userId: null);
        if (CrmCommandPersistenceHelper.IsActivo(request.Estado))
            usuario.Activar(userId: null);
        else
            usuario.Desactivar(userId: null);

        var perfil = await db.CrmUsuariosPerfiles.FirstOrDefaultAsync(x => x.UsuarioId == request.Id, ct);
        if (perfil is null)
        {
            perfil = CrmUsuarioPerfil.Crear(request.Id, request.Rol, request.Avatar, userId: null);
            db.CrmUsuariosPerfiles.Add(perfil);
        }
        else
        {
            perfil.Actualizar(request.Rol, request.Avatar, CrmCommandPersistenceHelper.IsActivo(request.Estado), userId: null);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteCrmUsuarioCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCrmUsuarioCommand, Result>
{
    public async Task<Result> Handle(DeleteCrmUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (usuario is null)
            return Result.Failure($"Usuario CRM {request.Id} no encontrado.");

        usuario.Desactivar(userId: null);

        var perfil = await db.CrmUsuariosPerfiles.FirstOrDefaultAsync(x => x.UsuarioId == request.Id, ct);
        perfil?.Actualizar(perfil.Rol, perfil.Avatar, activo: false, userId: null);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateCrmClienteCommandValidator : AbstractValidator<CreateCrmClienteCommand>
{
    public CreateCrmClienteCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.TipoCliente).Must(CrmDomainRules.IsValidTipoCliente).WithMessage("El tipo de cliente CRM no es válido.");
        RuleFor(x => x.Segmento).Must(CrmDomainRules.IsValidSegmentoCliente).WithMessage("El segmento del cliente CRM no es válido.");
        RuleFor(x => x.Pais).NotEmpty();
        RuleFor(x => x.OrigenCliente).Must(CrmDomainRules.IsValidOrigenCliente).WithMessage("El origen del cliente CRM no es válido.");
        RuleFor(x => x.EstadoRelacion).Must(CrmDomainRules.IsValidEstadoRelacion).WithMessage("El estado de relación CRM no es válido.");
    }
}

public class UpdateCrmClienteCommandValidator : AbstractValidator<UpdateCrmClienteCommand>
{
    public UpdateCrmClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.TipoCliente).Must(CrmDomainRules.IsValidTipoCliente).WithMessage("El tipo de cliente CRM no es válido.");
        RuleFor(x => x.Segmento).Must(CrmDomainRules.IsValidSegmentoCliente).WithMessage("El segmento del cliente CRM no es válido.");
        RuleFor(x => x.Pais).NotEmpty();
        RuleFor(x => x.OrigenCliente).Must(CrmDomainRules.IsValidOrigenCliente).WithMessage("El origen del cliente CRM no es válido.");
        RuleFor(x => x.EstadoRelacion).Must(CrmDomainRules.IsValidEstadoRelacion).WithMessage("El estado de relación CRM no es válido.");
    }
}

public class DeleteCrmClienteCommandValidator : AbstractValidator<DeleteCrmClienteCommand>
{
    public DeleteCrmClienteCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmContactoCommandValidator : AbstractValidator<CreateCrmContactoCommand>
{
    public CreateCrmContactoCommandValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Apellido).NotEmpty();
        RuleFor(x => x.CanalPreferido).Must(CrmDomainRules.IsValidCanalContacto).WithMessage("El canal preferido del contacto CRM no es válido.");
        RuleFor(x => x.EstadoContacto).Must(CrmDomainRules.IsValidEstadoContacto).WithMessage("El estado del contacto CRM no es válido.");
    }
}

public class UpdateCrmContactoCommandValidator : AbstractValidator<UpdateCrmContactoCommand>
{
    public UpdateCrmContactoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Apellido).NotEmpty();
        RuleFor(x => x.CanalPreferido).Must(CrmDomainRules.IsValidCanalContacto).WithMessage("El canal preferido del contacto CRM no es válido.");
        RuleFor(x => x.EstadoContacto).Must(CrmDomainRules.IsValidEstadoContacto).WithMessage("El estado del contacto CRM no es válido.");
    }
}

public class DeleteCrmContactoCommandValidator : AbstractValidator<DeleteCrmContactoCommand>
{
    public DeleteCrmContactoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmOportunidadCommandValidator : AbstractValidator<CreateCrmOportunidadCommand>
{
    public CreateCrmOportunidadCommandValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.Titulo).NotEmpty();
        RuleFor(x => x.Etapa).Must(CrmDomainRules.IsValidEtapaOportunidad).WithMessage("La etapa de la oportunidad CRM no es válida.");
        RuleFor(x => x.Probabilidad).InclusiveBetween(0, 100);
        RuleFor(x => x.Moneda).Must(CrmDomainRules.IsValidMoneda).WithMessage("La moneda de la oportunidad CRM no es válida.");
        RuleFor(x => x.Origen).Must(CrmDomainRules.IsValidOrigenOportunidad).WithMessage("El origen de la oportunidad CRM no es válido.");
    }
}

public class UpdateCrmOportunidadCommandValidator : AbstractValidator<UpdateCrmOportunidadCommand>
{
    public UpdateCrmOportunidadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.Titulo).NotEmpty();
        RuleFor(x => x.Etapa).Must(CrmDomainRules.IsValidEtapaOportunidad).WithMessage("La etapa de la oportunidad CRM no es válida.");
        RuleFor(x => x.Probabilidad).InclusiveBetween(0, 100);
        RuleFor(x => x.Moneda).Must(CrmDomainRules.IsValidMoneda).WithMessage("La moneda de la oportunidad CRM no es válida.");
        RuleFor(x => x.Origen).Must(CrmDomainRules.IsValidOrigenOportunidad).WithMessage("El origen de la oportunidad CRM no es válido.");
    }
}

public class DeleteCrmOportunidadCommandValidator : AbstractValidator<DeleteCrmOportunidadCommand>
{
    public DeleteCrmOportunidadCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CloseCrmOportunidadGanadaCommandValidator : AbstractValidator<CloseCrmOportunidadGanadaCommand>
{
    public CloseCrmOportunidadGanadaCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CloseCrmOportunidadPerdidaCommandValidator : AbstractValidator<CloseCrmOportunidadPerdidaCommand>
{
    public CloseCrmOportunidadPerdidaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.MotivoPerdida).NotEmpty();
    }
}

public class ReassignCrmOportunidadCommandValidator : AbstractValidator<ReassignCrmOportunidadCommand>
{
    public ReassignCrmOportunidadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ResponsableId).GreaterThan(0);
    }
}

public class CreateCrmInteraccionCommandValidator : AbstractValidator<CreateCrmInteraccionCommand>
{
    public CreateCrmInteraccionCommandValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.TipoInteraccion).Must(CrmDomainRules.IsValidTipoInteraccion).WithMessage("El tipo de interacción CRM no es válido.");
        RuleFor(x => x.Canal).Must(CrmDomainRules.IsValidCanalInteraccion).WithMessage("El canal de interacción CRM no es válido.");
        RuleFor(x => x.UsuarioResponsableId).GreaterThan(0);
        RuleFor(x => x.Resultado).Must(CrmDomainRules.IsValidResultadoInteraccion).WithMessage("El resultado de la interacción CRM no es válido.");
    }
}

public class UpdateCrmInteraccionCommandValidator : AbstractValidator<UpdateCrmInteraccionCommand>
{
    public UpdateCrmInteraccionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.TipoInteraccion).Must(CrmDomainRules.IsValidTipoInteraccion).WithMessage("El tipo de interacción CRM no es válido.");
        RuleFor(x => x.Canal).Must(CrmDomainRules.IsValidCanalInteraccion).WithMessage("El canal de interacción CRM no es válido.");
        RuleFor(x => x.UsuarioResponsableId).GreaterThan(0);
        RuleFor(x => x.Resultado).Must(CrmDomainRules.IsValidResultadoInteraccion).WithMessage("El resultado de la interacción CRM no es válido.");
    }
}

public class DeleteCrmInteraccionCommandValidator : AbstractValidator<DeleteCrmInteraccionCommand>
{
    public DeleteCrmInteraccionCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmTareaCommandValidator : AbstractValidator<CreateCrmTareaCommand>
{
    public CreateCrmTareaCommandValidator()
    {
        RuleFor(x => x.AsignadoAId).GreaterThan(0);
        RuleFor(x => x.Titulo).NotEmpty();
        RuleFor(x => x.TipoTarea).Must(CrmDomainRules.IsValidTipoTarea).WithMessage("El tipo de tarea CRM no es válido.");
        RuleFor(x => x.Prioridad).Must(CrmDomainRules.IsValidPrioridadTarea).WithMessage("La prioridad de la tarea CRM no es válida.");
        RuleFor(x => x.Estado).Must(CrmDomainRules.IsValidEstadoTarea).WithMessage("El estado de la tarea CRM no es válido.");
    }
}

public class UpdateCrmTareaCommandValidator : AbstractValidator<UpdateCrmTareaCommand>
{
    public UpdateCrmTareaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.AsignadoAId).GreaterThan(0);
        RuleFor(x => x.Titulo).NotEmpty();
        RuleFor(x => x.TipoTarea).Must(CrmDomainRules.IsValidTipoTarea).WithMessage("El tipo de tarea CRM no es válido.");
        RuleFor(x => x.Prioridad).Must(CrmDomainRules.IsValidPrioridadTarea).WithMessage("La prioridad de la tarea CRM no es válida.");
        RuleFor(x => x.Estado).Must(CrmDomainRules.IsValidEstadoTarea).WithMessage("El estado de la tarea CRM no es válido.");
    }
}

public class DeleteCrmTareaCommandValidator : AbstractValidator<DeleteCrmTareaCommand>
{
    public DeleteCrmTareaCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CompleteCrmTareaCommandValidator : AbstractValidator<CompleteCrmTareaCommand>
{
    public CompleteCrmTareaCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class ReopenCrmTareaCommandValidator : AbstractValidator<ReopenCrmTareaCommand>
{
    public ReopenCrmTareaCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class CreateCrmSegmentoCommandValidator : AbstractValidator<CreateCrmSegmentoCommand>
{
    public CreateCrmSegmentoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.TipoSegmento).Must(CrmDomainRules.IsValidTipoSegmento).WithMessage("El tipo de segmento CRM no es válido.");
        RuleFor(x => x.CriteriosJson)
            .Must((command, criteriosJson) => CrmDomainRules.HasValidSegmentDefinition(command.TipoSegmento, criteriosJson))
            .WithMessage("La definición del segmento CRM no es válida.");
    }
}

public class UpdateCrmSegmentoCommandValidator : AbstractValidator<UpdateCrmSegmentoCommand>
{
    public UpdateCrmSegmentoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.TipoSegmento).Must(CrmDomainRules.IsValidTipoSegmento).WithMessage("El tipo de segmento CRM no es válido.");
        RuleFor(x => x.CriteriosJson)
            .Must((command, criteriosJson) => CrmDomainRules.HasValidSegmentDefinition(command.TipoSegmento, criteriosJson))
            .WithMessage("La definición del segmento CRM no es válida.");
    }
}

public class DeleteCrmSegmentoCommandValidator : AbstractValidator<DeleteCrmSegmentoCommand>
{
    public DeleteCrmSegmentoCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class AddCrmSegmentoClienteCommandValidator : AbstractValidator<AddCrmSegmentoClienteCommand>
{
    public AddCrmSegmentoClienteCommandValidator()
    {
        RuleFor(x => x.SegmentoId).GreaterThan(0);
        RuleFor(x => x.ClienteId).GreaterThan(0);
    }
}

public class RemoveCrmSegmentoClienteCommandValidator : AbstractValidator<RemoveCrmSegmentoClienteCommand>
{
    public RemoveCrmSegmentoClienteCommandValidator()
    {
        RuleFor(x => x.SegmentoId).GreaterThan(0);
        RuleFor(x => x.ClienteId).GreaterThan(0);
    }
}

public class CreateCrmUsuarioCommandValidator : AbstractValidator<CreateCrmUsuarioCommand>
{
    public CreateCrmUsuarioCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Apellido).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Rol).Must(CrmDomainRules.IsValidRolUsuario).WithMessage("El rol de usuario CRM no es válido.");
        RuleFor(x => x.Estado).Must(CrmDomainRules.IsValidEstadoUsuario).WithMessage("El estado del usuario CRM no es válido.");
    }
}

public class UpdateCrmUsuarioCommandValidator : AbstractValidator<UpdateCrmUsuarioCommand>
{
    public UpdateCrmUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Apellido).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Rol).Must(CrmDomainRules.IsValidRolUsuario).WithMessage("El rol de usuario CRM no es válido.");
        RuleFor(x => x.Estado).Must(CrmDomainRules.IsValidEstadoUsuario).WithMessage("El estado del usuario CRM no es válido.");
    }
}

public class DeleteCrmUsuarioCommandValidator : AbstractValidator<DeleteCrmUsuarioCommand>
{
    public DeleteCrmUsuarioCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
