using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Logistica;

namespace ZuluIA_Back.Application.Features.Logistica.Commands;

public record CreateOrdenEmpaqueDetalleInput(
    long? ItemId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PorcentajeIva,
    string? Observacion);

public record CreateOrdenEmpaqueCommand(
    long TerceroId,
    long? SucursalTerceroId,
    long? VendedorId,
    long? DepositoId,
    long? TransportistaId,
    long? AgenteId,
    long? TipoComprobanteId,
    long? PuntoFacturacionId,
    int? MonedaId,
    decimal Cotizacion,
    DateOnly Fecha,
    DateOnly? FechaEmbarque,
    DateOnly? FechaVencimiento,
    string? OrigenObservacion,
    string? DestinoObservacion,
    decimal Total,
    string? Observacion,
    IReadOnlyList<CreateOrdenEmpaqueDetalleInput> Detalles) : IRequest<Result<long>>;

public record ConfirmOrdenEmpaqueCommand(long Id) : IRequest<Result<OrdenEmpaqueEstadoResult>>;

public record CancelOrdenEmpaqueCommand(long Id) : IRequest<Result<OrdenEmpaqueEstadoResult>>;

public record OrdenEmpaqueEstadoResult(long Id, string Estado);

public class CreateOrdenEmpaqueCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateOrdenEmpaqueCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateOrdenEmpaqueCommand request, CancellationToken ct)
    {
        try
        {
            var entity = OrdenEmpaque.Crear(
                request.TerceroId,
                request.SucursalTerceroId,
                request.VendedorId,
                request.DepositoId,
                request.TransportistaId,
                request.AgenteId,
                request.TipoComprobanteId,
                request.PuntoFacturacionId,
                request.MonedaId,
                request.Cotizacion,
                request.Fecha,
                request.FechaEmbarque,
                request.FechaVencimiento,
                request.OrigenObservacion,
                request.DestinoObservacion,
                request.Total,
                request.Observacion);

            foreach (var det in request.Detalles)
                entity.AgregarDetalle(
                    det.ItemId,
                    det.Descripcion,
                    det.Cantidad,
                    det.PrecioUnitario,
                    det.PorcentajeIva,
                    det.Observacion);

            db.OrdenesEmpaque.Add(entity);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class ConfirmOrdenEmpaqueCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ConfirmOrdenEmpaqueCommand, Result<OrdenEmpaqueEstadoResult>>
{
    public async Task<Result<OrdenEmpaqueEstadoResult>> Handle(ConfirmOrdenEmpaqueCommand request, CancellationToken ct)
    {
        var entity = await db.OrdenesEmpaque.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<OrdenEmpaqueEstadoResult>($"Orden de empaque {request.Id} no encontrada.");

        try
        {
            entity.Confirmar();
            await db.SaveChangesAsync(ct);
            return Result.Success(new OrdenEmpaqueEstadoResult(entity.Id, entity.Estado));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<OrdenEmpaqueEstadoResult>(ex.Message);
        }
    }
}

public class CancelOrdenEmpaqueCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CancelOrdenEmpaqueCommand, Result<OrdenEmpaqueEstadoResult>>
{
    public async Task<Result<OrdenEmpaqueEstadoResult>> Handle(CancelOrdenEmpaqueCommand request, CancellationToken ct)
    {
        var entity = await db.OrdenesEmpaque.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<OrdenEmpaqueEstadoResult>($"Orden de empaque {request.Id} no encontrada.");

        try
        {
            entity.Anular();
            await db.SaveChangesAsync(ct);
            return Result.Success(new OrdenEmpaqueEstadoResult(entity.Id, entity.Estado));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<OrdenEmpaqueEstadoResult>(ex.Message);
        }
    }
}

public class CreateOrdenEmpaqueCommandValidator : AbstractValidator<CreateOrdenEmpaqueCommand>
{
    public CreateOrdenEmpaqueCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Detalles).NotEmpty();
        RuleForEach(x => x.Detalles).ChildRules(d =>
        {
            d.RuleFor(x => x.Descripcion).NotEmpty();
            d.RuleFor(x => x.Cantidad).GreaterThan(0);
            d.RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
        });
    }
}

public class ConfirmOrdenEmpaqueCommandValidator : AbstractValidator<ConfirmOrdenEmpaqueCommand>
{
    public ConfirmOrdenEmpaqueCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CancelOrdenEmpaqueCommandValidator : AbstractValidator<CancelOrdenEmpaqueCommand>
{
    public CancelOrdenEmpaqueCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
