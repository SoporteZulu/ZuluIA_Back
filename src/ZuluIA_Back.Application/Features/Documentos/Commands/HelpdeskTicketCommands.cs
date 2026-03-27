using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Documentos.Commands;

public record CreateHelpdeskTicketCommand(
    long SucursalId,
    long TipoId,
    long? TerceroId,
    string NroDocumento,
    string Titulo,
    DateOnly FechaIngreso,
    DateOnly? FechaVencimiento,
    string? Observacion) : IRequest<Result<long>>;

public record AssignHelpdeskTicketCommand(long Id, long UsuarioId) : IRequest<Result<HelpdeskAsignacionResult>>;

public record ChangeHelpdeskTicketStateCommand(
    long Id,
    long EstadoId,
    string EstadoFlow,
    string? Observacion) : IRequest<Result<HelpdeskEstadoResult>>;

public record CloseHelpdeskTicketCommand(long Id) : IRequest<Result>;

public record HelpdeskAsignacionResult(long TicketId, long? AsignadoA);

public record HelpdeskEstadoResult(long TicketId, string EstadoFlow, long? EstadoId);

public class CreateHelpdeskTicketCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateHelpdeskTicketCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateHelpdeskTicketCommand request, CancellationToken ct)
    {
        try
        {
            var entity = MesaEntrada.Crear(
                request.SucursalId,
                request.TipoId,
                request.TerceroId,
                request.NroDocumento,
                request.Titulo,
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

public class AssignHelpdeskTicketCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AssignHelpdeskTicketCommand, Result<HelpdeskAsignacionResult>>
{
    public async Task<Result<HelpdeskAsignacionResult>> Handle(AssignHelpdeskTicketCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntrada.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<HelpdeskAsignacionResult>($"Ticket {request.Id} no encontrado.");

        entity.AsignarResponsable(request.UsuarioId, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success(new HelpdeskAsignacionResult(entity.Id, entity.AsignadoA));
    }
}

public class ChangeHelpdeskTicketStateCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ChangeHelpdeskTicketStateCommand, Result<HelpdeskEstadoResult>>
{
    public async Task<Result<HelpdeskEstadoResult>> Handle(ChangeHelpdeskTicketStateCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntrada.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<HelpdeskEstadoResult>($"Ticket {request.Id} no encontrado.");

        if (!Enum.TryParse<EstadoMesaEntrada>(request.EstadoFlow, true, out var flow))
            return Result.Failure<HelpdeskEstadoResult>("EstadoFlow inválido.");

        entity.CambiarEstado(request.EstadoId, flow, request.Observacion, userId: null);
        await db.SaveChangesAsync(ct);

        return Result.Success(new HelpdeskEstadoResult(entity.Id, entity.EstadoFlow.ToString(), entity.EstadoId));
    }
}

public class CloseHelpdeskTicketCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CloseHelpdeskTicketCommand, Result>
{
    public async Task<Result> Handle(CloseHelpdeskTicketCommand request, CancellationToken ct)
    {
        var entity = await db.MesasEntrada.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Ticket {request.Id} no encontrado.");

        entity.CambiarEstado(entity.EstadoId ?? 0, EstadoMesaEntrada.Resuelto, "Ticket cerrado", userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateHelpdeskTicketCommandValidator : AbstractValidator<CreateHelpdeskTicketCommand>
{
    public CreateHelpdeskTicketCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoId).GreaterThan(0);
        RuleFor(x => x.NroDocumento).NotEmpty();
        RuleFor(x => x.Titulo).NotEmpty();
    }
}

public class AssignHelpdeskTicketCommandValidator : AbstractValidator<AssignHelpdeskTicketCommand>
{
    public AssignHelpdeskTicketCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UsuarioId).GreaterThan(0);
    }
}

public class ChangeHelpdeskTicketStateCommandValidator : AbstractValidator<ChangeHelpdeskTicketStateCommand>
{
    public ChangeHelpdeskTicketStateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.EstadoId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EstadoFlow).NotEmpty();
    }
}

public class CloseHelpdeskTicketCommandValidator : AbstractValidator<CloseHelpdeskTicketCommand>
{
    public CloseHelpdeskTicketCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
