using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record CreateContratoDetalleInput(
    long? ItemId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PorcentajeIva,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    DateOnly FechaPrimeraFactura,
    int FrecuenciaMeses,
    int Corte,
    string? Dominio);

public record CreateContratoCommand(
    long TerceroId,
    long? SucursalTerceroId,
    long? VendedorId,
    long? TipoComprobanteId,
    long? PuntoFacturacionId,
    int? CondicionVentaId,
    int? MonedaId,
    decimal Cotizacion,
    DateOnly FechaDesde,
    DateOnly FechaVencimiento,
    DateOnly FechaInicioFacturacion,
    int PeriodoMeses,
    int Duracion,
    decimal Total,
    string? Observacion,
    IReadOnlyCollection<CreateContratoDetalleInput> Detalles) : IRequest<Result<long>>;

public record UpdateContratoCommand(
    long Id,
    long? VendedorId,
    int? CondicionVentaId,
    DateOnly FechaVencimiento,
    int PeriodoMeses,
    int Duracion,
    decimal Total,
    string? Observacion) : IRequest<Result>;

public record AnularContratoCommand(long Id, string? Motivo) : IRequest<Result<AnularContratoResult>>;

public record RegistrarFacturacionContratoCommand(long Id) : IRequest<Result<RegistrarFacturacionContratoResult>>;

public record AnularContratoResult(long Id, string Estado);

public record RegistrarFacturacionContratoResult(long Id, int CuotasRestantes, string Estado);

public class CreateContratoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateContratoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateContratoCommand request, CancellationToken ct)
    {
        try
        {
            var entity = Contrato.Crear(
                request.TerceroId,
                request.SucursalTerceroId,
                request.VendedorId,
                request.TipoComprobanteId,
                request.PuntoFacturacionId,
                request.CondicionVentaId,
                request.MonedaId,
                request.Cotizacion,
                request.FechaDesde,
                request.FechaVencimiento,
                request.FechaInicioFacturacion,
                request.PeriodoMeses,
                request.Duracion,
                request.Total,
                request.Observacion);

            foreach (var detalle in request.Detalles)
            {
                entity.AgregarDetalle(
                    detalle.ItemId,
                    detalle.Descripcion,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.PorcentajeIva,
                    detalle.FechaDesde,
                    detalle.FechaHasta,
                    detalle.FechaPrimeraFactura,
                    detalle.FrecuenciaMeses,
                    detalle.Corte,
                    detalle.Dominio);
            }

            await db.Contratos.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Result.Success(entity.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class UpdateContratoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateContratoCommand, Result>
{
    public async Task<Result> Handle(UpdateContratoCommand request, CancellationToken ct)
    {
        var entity = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Contrato {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(
                request.VendedorId,
                request.CondicionVentaId,
                request.FechaVencimiento,
                request.PeriodoMeses,
                request.Duracion,
                request.Total,
                request.Observacion);

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class AnularContratoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AnularContratoCommand, Result<AnularContratoResult>>
{
    public async Task<Result<AnularContratoResult>> Handle(AnularContratoCommand request, CancellationToken ct)
    {
        var entity = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<AnularContratoResult>($"Contrato {request.Id} no encontrado.");

        try
        {
            entity.Anular(request.Motivo);
            await db.SaveChangesAsync(ct);
            return Result.Success(new AnularContratoResult(entity.Id, entity.Estado));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<AnularContratoResult>(ex.Message);
        }
    }
}

public class RegistrarFacturacionContratoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegistrarFacturacionContratoCommand, Result<RegistrarFacturacionContratoResult>>
{
    public async Task<Result<RegistrarFacturacionContratoResult>> Handle(RegistrarFacturacionContratoCommand request, CancellationToken ct)
    {
        var entity = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure<RegistrarFacturacionContratoResult>($"Contrato {request.Id} no encontrado.");

        if (entity.Anulado)
            return Result.Failure<RegistrarFacturacionContratoResult>("El contrato está anulado.");

        entity.RegistrarFacturacion();
        await db.SaveChangesAsync(ct);

        return Result.Success(new RegistrarFacturacionContratoResult(entity.Id, entity.CuotasRestantes, entity.Estado));
    }
}

public class CreateContratoDetalleInputValidator : AbstractValidator<CreateContratoDetalleInput>
{
    public CreateContratoDetalleInputValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FrecuenciaMeses).GreaterThan(0);
    }
}

public class CreateContratoCommandValidator : AbstractValidator<CreateContratoCommand>
{
    public CreateContratoCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.PeriodoMeses).GreaterThan(0);
        RuleFor(x => x.Duracion).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Detalles).NotNull().NotEmpty();
        RuleForEach(x => x.Detalles).SetValidator(new CreateContratoDetalleInputValidator());
    }
}

public class UpdateContratoCommandValidator : AbstractValidator<UpdateContratoCommand>
{
    public UpdateContratoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.PeriodoMeses).GreaterThan(0);
        RuleFor(x => x.Duracion).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0);
    }
}

public class AnularContratoCommandValidator : AbstractValidator<AnularContratoCommand>
{
    public AnularContratoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class RegistrarFacturacionContratoCommandValidator : AbstractValidator<RegistrarFacturacionContratoCommand>
{
    public RegistrarFacturacionContratoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}