using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contratos;

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

public record CreateContratoComprobanteCommand(
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

public record UpdateContratoComprobanteCommand(
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

public class CreateContratoComprobanteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateContratoComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateContratoComprobanteCommand request, CancellationToken ct)
    {
        var sucursalId = request.SucursalTerceroId ?? 0;
        if (sucursalId <= 0)
            return Result.Failure<long>("La sucursal del contrato es requerida.");

        var monedaId = request.MonedaId.HasValue ? request.MonedaId.Value : 0;
        if (monedaId <= 0)
            return Result.Failure<long>("La moneda del contrato es requerida.");

        try
        {
            var entity = Contrato.Crear(
                request.TerceroId,
                sucursalId,
                monedaId,
                $"CTR-CMP-{request.TerceroId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                request.Detalles.FirstOrDefault()?.Descripcion ?? request.Observacion ?? "Contrato recurrente",
                request.FechaDesde,
                request.FechaVencimiento,
                request.Total,
                request.Duracion > 1,
                request.Observacion,
                null);

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

public class UpdateContratoComprobanteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateContratoComprobanteCommand, Result>
{
    public async Task<Result> Handle(UpdateContratoComprobanteCommand request, CancellationToken ct)
    {
        var entity = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return Result.Failure($"Contrato {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(
                entity.Descripcion,
                entity.FechaInicio,
                request.FechaVencimiento,
                request.Total,
                entity.RenovacionAutomatica,
                request.Observacion,
                null);

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (ArgumentException ex)
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
            entity.Cancelar(request.Motivo, null);
            await db.SaveChangesAsync(ct);
            return Result.Success(new AnularContratoResult(entity.Id, entity.Estado.ToString().ToUpperInvariant()));
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

        return Result.Success(new RegistrarFacturacionContratoResult(entity.Id, 0, entity.Estado.ToString().ToUpperInvariant()));
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

public class CreateContratoComprobanteCommandValidator : AbstractValidator<CreateContratoComprobanteCommand>
{
    public CreateContratoComprobanteCommandValidator()
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

public class UpdateContratoComprobanteCommandValidator : AbstractValidator<UpdateContratoComprobanteCommand>
{
    public UpdateContratoComprobanteCommandValidator()
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