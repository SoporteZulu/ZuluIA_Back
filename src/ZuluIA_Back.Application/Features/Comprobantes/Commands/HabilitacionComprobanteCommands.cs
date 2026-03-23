using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record CreateHabilitacionComprobanteCommand(
    long ComprobanteId,
    long SucursalId,
    string TipoDocumento,
    string? MotivoBloqueo) : IRequest<Result<long>>;

public record HabilitarComprobanteCommand(
    long Id,
    long ResponsableId,
    string? Observacion) : IRequest<Result>;

public record RechazarHabilitacionComprobanteCommand(
    long Id,
    long ResponsableId,
    string? Observacion) : IRequest<Result>;

public class CreateHabilitacionComprobanteCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateHabilitacionComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateHabilitacionComprobanteCommand request, CancellationToken ct)
    {
        HabilitacionComprobante entity;
        try
        {
            entity = HabilitacionComprobante.Crear(
                request.ComprobanteId,
                request.SucursalId,
                request.TipoDocumento,
                request.MotivoBloqueo,
                currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.HabilitacionesComprobantes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class HabilitarComprobanteCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<HabilitarComprobanteCommand, Result>
{
    public async Task<Result> Handle(HabilitarComprobanteCommand request, CancellationToken ct)
    {
        var entity = await db.HabilitacionesComprobantes.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Habilitación {request.Id} no encontrada.");

        try
        {
            entity.Habilitar(request.ResponsableId, request.Observacion, currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class RechazarHabilitacionComprobanteCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<RechazarHabilitacionComprobanteCommand, Result>
{
    public async Task<Result> Handle(RechazarHabilitacionComprobanteCommand request, CancellationToken ct)
    {
        var entity = await db.HabilitacionesComprobantes.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Habilitación {request.Id} no encontrada.");

        try
        {
            entity.Rechazar(request.ResponsableId, request.Observacion, currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateHabilitacionComprobanteCommandValidator : AbstractValidator<CreateHabilitacionComprobanteCommand>
{
    public CreateHabilitacionComprobanteCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoDocumento).NotEmpty();
    }
}

public class HabilitarComprobanteCommandValidator : AbstractValidator<HabilitarComprobanteCommand>
{
    public HabilitarComprobanteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ResponsableId).GreaterThan(0);
    }
}

public class RechazarHabilitacionComprobanteCommandValidator : AbstractValidator<RechazarHabilitacionComprobanteCommand>
{
    public RechazarHabilitacionComprobanteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ResponsableId).GreaterThan(0);
    }
}
