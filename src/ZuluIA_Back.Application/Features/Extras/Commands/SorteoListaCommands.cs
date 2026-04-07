using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateSorteoListaCommand(
    long SucursalId,
    long TipoId,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin) : IRequest<Result<long>>;

public record UpdateSorteoListaCommand(
    long Id,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin) : IRequest<Result>;

public record CloseSorteoListaCommand(long Id) : IRequest<Result>;

public record AddParticipanteSorteoCommand(long SorteoId, long TerceroId, int NroTicket) : IRequest<Result<long>>;

public record MarkSorteoParticipanteGanadorCommand(long SorteoId, long ParticipanteId) : IRequest<Result<SorteoParticipanteGanadorResult>>;

public record CreateSorteoListaTipoCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;

public record UpdateSorteoListaTipoCommand(long Id, string Descripcion) : IRequest<Result>;

public record DeactivateSorteoListaTipoCommand(long Id) : IRequest<Result>;

public record ActivateSorteoListaTipoCommand(long Id) : IRequest<Result>;

public record SorteoParticipanteGanadorResult(long Id, bool Ganador);

public class CreateSorteoListaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateSorteoListaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateSorteoListaCommand request, CancellationToken ct)
    {
        try
        {
            var entity = SorteoLista.Crear(
                request.SucursalId,
                request.TipoId,
                request.Descripcion,
                request.FechaInicio,
                request.FechaFin,
                userId: null);

            db.SorteosLista.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateSorteoListaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSorteoListaCommand, Result>
{
    public async Task<Result> Handle(UpdateSorteoListaCommand request, CancellationToken ct)
    {
        var entity = await db.SorteosLista.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Sorteo {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(request.Descripcion, request.FechaInicio, request.FechaFin, userId: null);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class CloseSorteoListaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CloseSorteoListaCommand, Result>
{
    public async Task<Result> Handle(CloseSorteoListaCommand request, CancellationToken ct)
    {
        var entity = await db.SorteosLista.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Sorteo {request.Id} no encontrado.");

        entity.Cerrar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AddParticipanteSorteoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddParticipanteSorteoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddParticipanteSorteoCommand request, CancellationToken ct)
    {
        var sorteoExists = await db.SorteosLista.AnyAsync(x => x.Id == request.SorteoId, ct);
        if (!sorteoExists)
            return Result.Failure<long>($"Sorteo {request.SorteoId} no encontrado.");

        var ticketExists = await db.SorteosListaXCliente
            .AnyAsync(x => x.SorteoListaId == request.SorteoId && x.NroTicket == request.NroTicket, ct);
        if (ticketExists)
            return Result.Failure<long>("Ya existe ese numero de ticket en el sorteo.");

        try
        {
            var entity = SorteoListaXCliente.Inscribir(request.SorteoId, request.TerceroId, request.NroTicket);
            db.SorteosListaXCliente.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class MarkSorteoParticipanteGanadorCommandHandler(IApplicationDbContext db)
    : IRequestHandler<MarkSorteoParticipanteGanadorCommand, Result<SorteoParticipanteGanadorResult>>
{
    public async Task<Result<SorteoParticipanteGanadorResult>> Handle(MarkSorteoParticipanteGanadorCommand request, CancellationToken ct)
    {
        var entity = await db.SorteosListaXCliente
            .FirstOrDefaultAsync(x => x.Id == request.ParticipanteId && x.SorteoListaId == request.SorteoId, ct);

        if (entity is null)
            return Result.Failure<SorteoParticipanteGanadorResult>("Participante no encontrado.");

        entity.MarcarGanador();
        await db.SaveChangesAsync(ct);
        return Result.Success(new SorteoParticipanteGanadorResult(entity.Id, entity.Ganador));
    }
}

public class CreateSorteoListaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateSorteoListaTipoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateSorteoListaTipoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await db.SorteosListaTipos.AnyAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un tipo con ese codigo.");

        try
        {
            var entity = SorteoListaTipo.Crear(codigo, request.Descripcion, userId: null);
            db.SorteosListaTipos.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateSorteoListaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSorteoListaTipoCommand, Result>
{
    public async Task<Result> Handle(UpdateSorteoListaTipoCommand request, CancellationToken ct)
    {
        var entity = await db.SorteosListaTipos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo {request.Id} no encontrado.");

        entity.Actualizar(request.Descripcion, userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeactivateSorteoListaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateSorteoListaTipoCommand, Result>
{
    public async Task<Result> Handle(DeactivateSorteoListaTipoCommand request, CancellationToken ct)
    {
        var entity = await db.SorteosListaTipos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateSorteoListaTipoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateSorteoListaTipoCommand, Result>
{
    public async Task<Result> Handle(ActivateSorteoListaTipoCommand request, CancellationToken ct)
    {
        var entity = await db.SorteosListaTipos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tipo {request.Id} no encontrado.");

        entity.Activar(userId: null);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateSorteoListaCommandValidator : AbstractValidator<CreateSorteoListaCommand>
{
    public CreateSorteoListaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
    }
}

public class UpdateSorteoListaCommandValidator : AbstractValidator<UpdateSorteoListaCommand>
{
    public UpdateSorteoListaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
    }
}

public class CloseSorteoListaCommandValidator : AbstractValidator<CloseSorteoListaCommand>
{
    public CloseSorteoListaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class AddParticipanteSorteoCommandValidator : AbstractValidator<AddParticipanteSorteoCommand>
{
    public AddParticipanteSorteoCommandValidator()
    {
        RuleFor(x => x.SorteoId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.NroTicket).GreaterThan(0);
    }
}

public class MarkSorteoParticipanteGanadorCommandValidator : AbstractValidator<MarkSorteoParticipanteGanadorCommand>
{
    public MarkSorteoParticipanteGanadorCommandValidator()
    {
        RuleFor(x => x.SorteoId).GreaterThan(0);
        RuleFor(x => x.ParticipanteId).GreaterThan(0);
    }
}

public class CreateSorteoListaTipoCommandValidator : AbstractValidator<CreateSorteoListaTipoCommand>
{
    public CreateSorteoListaTipoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class UpdateSorteoListaTipoCommandValidator : AbstractValidator<UpdateSorteoListaTipoCommand>
{
    public UpdateSorteoListaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}

public class DeactivateSorteoListaTipoCommandValidator : AbstractValidator<DeactivateSorteoListaTipoCommand>
{
    public DeactivateSorteoListaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateSorteoListaTipoCommandValidator : AbstractValidator<ActivateSorteoListaTipoCommand>
{
    public ActivateSorteoListaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}