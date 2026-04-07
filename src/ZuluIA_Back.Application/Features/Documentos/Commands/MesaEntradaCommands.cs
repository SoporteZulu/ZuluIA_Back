using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Documentos.Commands;

public record CreateMesaEntradaCommand(
    long SucursalId,
    long TipoId,
    long? TerceroId,
    string NroDocumento,
    string Asunto,
    DateOnly FechaIngreso,
    DateOnly? FechaVencimiento,
    string? Observacion) : IRequest<Result<long>>;

public record AssignMesaEntradaCommand(long Id, long UsuarioId) : IRequest<Result<MesaEntradaAsignacionResult>>;

public record ChangeMesaEntradaEstadoCommand(
    long Id,
    long EstadoId,
    string EstadoFlow,
    string? Observacion) : IRequest<Result<MesaEntradaEstadoResult>>;

public record ArchiveMesaEntradaCommand(long Id) : IRequest<Result>;

public record CancelMesaEntradaCommand(long Id) : IRequest<Result>;

public record CreateMesaEntradaTipoCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;

public record UpdateMesaEntradaTipoCommand(long Id, string Descripcion) : IRequest<Result>;

public record DeactivateMesaEntradaTipoCommand(long Id) : IRequest<Result>;

public record ActivateMesaEntradaTipoCommand(long Id) : IRequest<Result>;

public record CreateMesaEntradaEstadoCommand(string Codigo, string Descripcion, bool EsFinal) : IRequest<Result<long>>;

public record UpdateMesaEntradaEstadoCommand(long Id, string Descripcion, bool EsFinal) : IRequest<Result>;

public record DeactivateMesaEntradaEstadoCommand(long Id) : IRequest<Result>;

public record ActivateMesaEntradaEstadoCommand(long Id) : IRequest<Result>;

public record MesaEntradaAsignacionResult(long Id, long? AsignadoA);

public record MesaEntradaEstadoResult(long Id, string EstadoFlow, long? EstadoId);

public class CreateMesaEntradaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateMesaEntradaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateMesaEntradaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = MesaEntrada.Crear(
                request.SucursalId,
                request.TipoId,
                request.TerceroId,
                request.NroDocumento,
                request.Asunto,
                request.FechaIngreso,
                request.FechaVencimiento,
                request.Observacion,
                userId: null);

            db.MesasEntrada.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class AssignMesaEntradaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AssignMesaEntradaCommand, Result<MesaEntradaAsignacionResult>>
{
    public async Task<Result<MesaEntradaAsignacionResult>> Handle(AssignMesaEntradaCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntrada.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<MesaEntradaAsignacionResult>($"Registro {request.Id} no encontrado.");

        entity.AsignarResponsable(request.UsuarioId, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success(new MesaEntradaAsignacionResult(entity.Id, entity.AsignadoA));
    }
}

public class ChangeMesaEntradaEstadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ChangeMesaEntradaEstadoCommand, Result<MesaEntradaEstadoResult>>
{
    public async Task<Result<MesaEntradaEstadoResult>> Handle(ChangeMesaEntradaEstadoCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntrada.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<MesaEntradaEstadoResult>($"Registro {request.Id} no encontrado.");

        if (!Enum.TryParse<EstadoMesaEntrada>(request.EstadoFlow, true, out var flow))
            return Result.Failure<MesaEntradaEstadoResult>("EstadoFlow inválido.");

        entity.CambiarEstado(request.EstadoId, flow, request.Observacion, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success(new MesaEntradaEstadoResult(entity.Id, entity.EstadoFlow.ToString(), entity.EstadoId));
    }
}

public class ArchiveMesaEntradaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ArchiveMesaEntradaCommand, Result>
{
    public async Task<Result> Handle(ArchiveMesaEntradaCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntrada.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Registro {request.Id} no encontrado.");

        entity.Archivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CancelMesaEntradaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CancelMesaEntradaCommand, Result>
{
    public async Task<Result> Handle(CancelMesaEntradaCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntrada.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Registro {request.Id} no encontrado.");

        entity.Anular(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateMesaEntradaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateMesaEntradaTipoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateMesaEntradaTipoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.MesasEntradaTipos.AnyAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un tipo con ese codigo.");

        var entity = MesaEntradaTipo.Crear(codigo, request.Descripcion, userId: null);
        db.MesasEntradaTipos.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateMesaEntradaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateMesaEntradaTipoCommand, Result>
{
    public async Task<Result> Handle(UpdateMesaEntradaTipoCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntradaTipos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo {request.Id} no encontrado.");

        entity.Actualizar(request.Descripcion, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeactivateMesaEntradaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateMesaEntradaTipoCommand, Result>
{
    public async Task<Result> Handle(DeactivateMesaEntradaTipoCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntradaTipos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateMesaEntradaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateMesaEntradaTipoCommand, Result>
{
    public async Task<Result> Handle(ActivateMesaEntradaTipoCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntradaTipos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo {request.Id} no encontrado.");

        entity.Activar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateMesaEntradaEstadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateMesaEntradaEstadoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateMesaEntradaEstadoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.MesasEntradaEstados.AnyAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un estado con ese codigo.");

        var entity = MesaEntradaEstado.Crear(codigo, request.Descripcion, request.EsFinal, userId: null);
        db.MesasEntradaEstados.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateMesaEntradaEstadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateMesaEntradaEstadoCommand, Result>
{
    public async Task<Result> Handle(UpdateMesaEntradaEstadoCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntradaEstados.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado {request.Id} no encontrado.");

        entity.Actualizar(request.Descripcion, request.EsFinal, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeactivateMesaEntradaEstadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateMesaEntradaEstadoCommand, Result>
{
    public async Task<Result> Handle(DeactivateMesaEntradaEstadoCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntradaEstados.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateMesaEntradaEstadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateMesaEntradaEstadoCommand, Result>
{
    public async Task<Result> Handle(ActivateMesaEntradaEstadoCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntradaEstados.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado {request.Id} no encontrado.");

        entity.Activar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateMesaEntradaCommandValidator : AbstractValidator<CreateMesaEntradaCommand>
{
    public CreateMesaEntradaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoId).GreaterThan(0);
        RuleFor(x => x.NroDocumento).NotEmpty();
        RuleFor(x => x.Asunto).NotEmpty();
    }
}

public class AssignMesaEntradaCommandValidator : AbstractValidator<AssignMesaEntradaCommand>
{
    public AssignMesaEntradaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UsuarioId).GreaterThan(0);
    }
}

public class ChangeMesaEntradaEstadoCommandValidator : AbstractValidator<ChangeMesaEntradaEstadoCommand>
{
    public ChangeMesaEntradaEstadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.EstadoId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EstadoFlow).NotEmpty();
    }
}

public class ArchiveMesaEntradaCommandValidator : AbstractValidator<ArchiveMesaEntradaCommand>
{
    public ArchiveMesaEntradaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CancelMesaEntradaCommandValidator : AbstractValidator<CancelMesaEntradaCommand>
{
    public CancelMesaEntradaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateMesaEntradaTipoCommandValidator : AbstractValidator<CreateMesaEntradaTipoCommand>
{
    public CreateMesaEntradaTipoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateMesaEntradaTipoCommandValidator : AbstractValidator<UpdateMesaEntradaTipoCommand>
{
    public UpdateMesaEntradaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeactivateMesaEntradaTipoCommandValidator : AbstractValidator<DeactivateMesaEntradaTipoCommand>
{
    public DeactivateMesaEntradaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateMesaEntradaTipoCommandValidator : AbstractValidator<ActivateMesaEntradaTipoCommand>
{
    public ActivateMesaEntradaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateMesaEntradaEstadoCommandValidator : AbstractValidator<CreateMesaEntradaEstadoCommand>
{
    public CreateMesaEntradaEstadoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateMesaEntradaEstadoCommandValidator : AbstractValidator<UpdateMesaEntradaEstadoCommand>
{
    public UpdateMesaEntradaEstadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeactivateMesaEntradaEstadoCommandValidator : AbstractValidator<DeactivateMesaEntradaEstadoCommand>
{
    public DeactivateMesaEntradaEstadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateMesaEntradaEstadoCommandValidator : AbstractValidator<ActivateMesaEntradaEstadoCommand>
{
    public ActivateMesaEntradaEstadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
