using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateTransportistaCommand(
    long TerceroId,
    string? NroCuitTransportista,
    string? DomicilioPartida,
    string? Patente,
    string? MarcaVehiculo) : IRequest<Result<long>>;

public record UpdateTransportistaCommand(
    long Id,
    string? DomicilioPartida,
    string? Patente,
    string? MarcaVehiculo) : IRequest<Result>;

public record DeactivateTransportistaCommand(long Id) : IRequest<Result>;

public record ActivateTransportistaCommand(long Id) : IRequest<Result>;

public class CreateTransportistaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateTransportistaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTransportistaCommand request, CancellationToken ct)
    {
        var existe = await db.Transportistas
            .AnyAsync(x => x.TerceroId == request.TerceroId, ct);

        if (existe)
            return Result.Failure<long>($"Ya existe un transportista para el tercero ID {request.TerceroId}.");

        var entity = Transportista.Crear(
            request.TerceroId,
            request.NroCuitTransportista,
            request.DomicilioPartida,
            request.Patente,
            request.MarcaVehiculo);

        await db.Transportistas.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class UpdateTransportistaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTransportistaCommand, Result>
{
    public async Task<Result> Handle(UpdateTransportistaCommand request, CancellationToken ct)
    {
        var entity = await db.Transportistas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"No se encontró el transportista con ID {request.Id}.");

        entity.Actualizar(request.DomicilioPartida, request.Patente, request.MarcaVehiculo);
        db.Transportistas.Update(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeactivateTransportistaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateTransportistaCommand, Result>
{
    public async Task<Result> Handle(DeactivateTransportistaCommand request, CancellationToken ct)
    {
        var entity = await db.Transportistas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"No se encontró el transportista con ID {request.Id}.");

        entity.Desactivar();
        db.Transportistas.Update(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ActivateTransportistaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateTransportistaCommand, Result>
{
    public async Task<Result> Handle(ActivateTransportistaCommand request, CancellationToken ct)
    {
        var entity = await db.Transportistas.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"No se encontró el transportista con ID {request.Id}.");

        entity.Activar();
        db.Transportistas.Update(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateTransportistaCommandValidator : AbstractValidator<CreateTransportistaCommand>
{
    public CreateTransportistaCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
    }
}

public class UpdateTransportistaCommandValidator : AbstractValidator<UpdateTransportistaCommand>
{
    public UpdateTransportistaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateTransportistaCommandValidator : AbstractValidator<DeactivateTransportistaCommand>
{
    public DeactivateTransportistaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateTransportistaCommandValidator : AbstractValidator<ActivateTransportistaCommand>
{
    public ActivateTransportistaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
