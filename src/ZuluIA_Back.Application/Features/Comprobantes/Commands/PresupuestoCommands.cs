using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record CreatePresupuestoItemInput(
    long ItemId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal DescuentoPct);

public record CreatePresupuestoCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    DateOnly? FechaVigencia,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    long? UserId,
    IReadOnlyCollection<CreatePresupuestoItemInput>? Items) : IRequest<Result<long>>;

public record AprobarPresupuestoCommand(long Id, long? UserId) : IRequest<Result>;

public record RechazarPresupuestoCommand(long Id, long? UserId) : IRequest<Result>;

public record DeletePresupuestoCommand(long Id) : IRequest<Result>;

public class CreatePresupuestoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreatePresupuestoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePresupuestoCommand request, CancellationToken ct)
    {
        var pres = Presupuesto.Crear(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.FechaVigencia,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            request.UserId);

        await db.Presupuestos.AddAsync(pres, ct);
        await db.SaveChangesAsync(ct);

        if (request.Items is { Count: > 0 })
        {
            foreach (var (item, idx) in request.Items.Select((x, i) => (x, i)))
            {
                var linea = PresupuestoItem.Crear(
                    pres.Id,
                    item.ItemId,
                    item.Descripcion,
                    item.Cantidad,
                    item.PrecioUnitario,
                    item.DescuentoPct,
                    (short)idx);

                await db.PresupuestosItems.AddAsync(linea, ct);
            }

            pres.ActualizarTotal(
                request.Items.Sum(i => i.Cantidad * i.PrecioUnitario * (1 - i.DescuentoPct / 100)),
                request.UserId);

            await db.SaveChangesAsync(ct);
        }

        return Result.Success(pres.Id);
    }
}

public class AprobarPresupuestoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AprobarPresupuestoCommand, Result>
{
    public async Task<Result> Handle(AprobarPresupuestoCommand request, CancellationToken ct)
    {
        var entity = await db.Presupuestos
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct);

        if (entity is null)
            return Result.Failure($"Presupuesto {request.Id} no encontrado.");

        entity.Aprobar(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class RechazarPresupuestoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RechazarPresupuestoCommand, Result>
{
    public async Task<Result> Handle(RechazarPresupuestoCommand request, CancellationToken ct)
    {
        var entity = await db.Presupuestos
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct);

        if (entity is null)
            return Result.Failure($"Presupuesto {request.Id} no encontrado.");

        entity.Rechazar(request.UserId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeletePresupuestoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeletePresupuestoCommand, Result>
{
    public async Task<Result> Handle(DeletePresupuestoCommand request, CancellationToken ct)
    {
        var entity = await db.Presupuestos
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct);

        if (entity is null)
            return Result.Failure($"Presupuesto {request.Id} no encontrado.");

        entity.Eliminar();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreatePresupuestoItemInputValidator : AbstractValidator<CreatePresupuestoItemInput>
{
    public CreatePresupuestoItemInputValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DescuentoPct).InclusiveBetween(0, 100);
    }
}

public class CreatePresupuestoCommandValidator : AbstractValidator<CreatePresupuestoCommand>
{
    public CreatePresupuestoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleForEach(x => x.Items).SetValidator(new CreatePresupuestoItemInputValidator());
    }
}

public class AprobarPresupuestoCommandValidator : AbstractValidator<AprobarPresupuestoCommand>
{
    public AprobarPresupuestoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class RechazarPresupuestoCommandValidator : AbstractValidator<RechazarPresupuestoCommand>
{
    public RechazarPresupuestoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeletePresupuestoCommandValidator : AbstractValidator<DeletePresupuestoCommand>
{
    public DeletePresupuestoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}