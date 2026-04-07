using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record CreateAutorizacionComprobanteCommand(
    long ComprobanteId,
    long SucursalId,
    string TipoOperacion) : IRequest<Result<long>>;

public record AutorizarComprobanteCommand(
    long Id,
    long ResponsableId,
    string? Motivo) : IRequest<Result>;

public record RechazarAutorizacionComprobanteCommand(
    long Id,
    long ResponsableId,
    string? Motivo) : IRequest<Result>;

public class CreateAutorizacionComprobanteCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateAutorizacionComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateAutorizacionComprobanteCommand request, CancellationToken ct)
    {
        AutorizacionComprobante entity;
        try
        {
            entity = AutorizacionComprobante.Crear(
                request.ComprobanteId,
                request.SucursalId,
                request.TipoOperacion,
                currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.AutorizacionesComprobantes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result.Success(entity.Id);
    }
}

public class AutorizarComprobanteCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<AutorizarComprobanteCommand, Result>
{
    public async Task<Result> Handle(AutorizarComprobanteCommand request, CancellationToken ct)
    {
        var entity = await db.AutorizacionesComprobantes.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Autorización {request.Id} no encontrada.");

        try
        {
            entity.Autorizar(request.ResponsableId, request.Motivo, currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class RechazarAutorizacionComprobanteCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<RechazarAutorizacionComprobanteCommand, Result>
{
    public async Task<Result> Handle(RechazarAutorizacionComprobanteCommand request, CancellationToken ct)
    {
        var entity = await db.AutorizacionesComprobantes.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Autorización {request.Id} no encontrada.");

        try
        {
            entity.Rechazar(request.ResponsableId, request.Motivo, currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateAutorizacionComprobanteCommandValidator : AbstractValidator<CreateAutorizacionComprobanteCommand>
{
    public CreateAutorizacionComprobanteCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoOperacion).NotEmpty();
    }
}

public class AutorizarComprobanteCommandValidator : AbstractValidator<AutorizarComprobanteCommand>
{
    public AutorizarComprobanteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ResponsableId).GreaterThan(0);
    }
}

public class RechazarAutorizacionComprobanteCommandValidator : AbstractValidator<RechazarAutorizacionComprobanteCommand>
{
    public RechazarAutorizacionComprobanteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ResponsableId).GreaterThan(0);
    }
}
