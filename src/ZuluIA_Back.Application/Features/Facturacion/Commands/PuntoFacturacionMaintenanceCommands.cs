using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record AddConfiguracionFiscalCommand(
    long PuntoFacturacionId,
    long TipoComprobanteId,
    long? TecnologiaId,
    long? InterfazFiscalId,
    int? Marco,
    string? Puerto,
    int CopiasDefault,
    string? ClaveActivacion,
    string? DirectorioLocal,
    string? DirectorioLocalBackup,
    int? TimerInicial,
    int? TimerSincronizacion,
    bool Online) : IRequest<Result<long>>;

public record UpdateConfiguracionFiscalCommand(
    long PuntoFacturacionId,
    long ConfiguracionFiscalId,
    long? TecnologiaId,
    long? InterfazFiscalId,
    int? Marco,
    string? Puerto,
    int CopiasDefault,
    string? ClaveActivacion,
    string? DirectorioLocal,
    string? DirectorioLocalBackup,
    int? TimerInicial,
    int? TimerSincronizacion,
    bool Online) : IRequest<Result>;

public record DeleteConfiguracionFiscalCommand(long PuntoFacturacionId, long ConfiguracionFiscalId) : IRequest<Result>;

public class AddConfiguracionFiscalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddConfiguracionFiscalCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddConfiguracionFiscalCommand request, CancellationToken ct)
    {
        var puntoExiste = await db.PuntosFacturacion.AnyAsync(x => x.Id == request.PuntoFacturacionId, ct);
        if (!puntoExiste)
            return Result.Failure<long>("Punto de facturacion no encontrado.");

        var config = ConfiguracionFiscal.Crear(
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.TecnologiaId,
            request.InterfazFiscalId,
            request.Marco,
            request.Puerto,
            request.CopiasDefault,
            request.ClaveActivacion,
            request.DirectorioLocal,
            request.DirectorioLocalBackup,
            request.TimerInicial,
            request.TimerSincronizacion,
            request.Online);

        db.ConfiguracionesFiscales.Add(config);
        await db.SaveChangesAsync(ct);

        return Result.Success(config.Id);
    }
}

public class UpdateConfiguracionFiscalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateConfiguracionFiscalCommand, Result>
{
    public async Task<Result> Handle(UpdateConfiguracionFiscalCommand request, CancellationToken ct)
    {
        var config = await db.ConfiguracionesFiscales.FirstOrDefaultAsync(
            c => c.Id == request.ConfiguracionFiscalId && c.PuntoFacturacionId == request.PuntoFacturacionId,
            ct);

        if (config is null)
            return Result.Failure("Configuracion no encontrada.");

        config.Actualizar(
            request.TecnologiaId,
            request.InterfazFiscalId,
            request.Marco,
            request.Puerto,
            request.CopiasDefault,
            request.ClaveActivacion,
            request.DirectorioLocal,
            request.DirectorioLocalBackup,
            request.TimerInicial,
            request.TimerSincronizacion,
            request.Online);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteConfiguracionFiscalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteConfiguracionFiscalCommand, Result>
{
    public async Task<Result> Handle(DeleteConfiguracionFiscalCommand request, CancellationToken ct)
    {
        var config = await db.ConfiguracionesFiscales.FirstOrDefaultAsync(
            c => c.Id == request.ConfiguracionFiscalId && c.PuntoFacturacionId == request.PuntoFacturacionId,
            ct);

        if (config is null)
            return Result.Failure("Configuracion no encontrada.");

        db.ConfiguracionesFiscales.Remove(config);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddTipoComprobantePuntoFacturacionCommand(
    long PuntoFacturacionId,
    long TipoComprobanteId,
    long NumeroComprobanteProximo,
    bool Editable,
    int FilasCantidad,
    int FilasAnchoMaximo,
    long? ReporteId,
    int CantidadCopias,
    bool VistaPrevia,
    bool ImprimirControladorFiscal,
    bool PermitirSeleccionMoneda,
    int? VarianteNroUnico,
    string? MascaraMoneda) : IRequest<Result<long>>;

public record UpdateTipoComprobantePuntoFacturacionCommand(
    long PuntoFacturacionId,
    long TipoComprobantePuntoFacturacionId,
    long NumeroComprobanteProximo,
    bool Editable,
    int FilasCantidad,
    int FilasAnchoMaximo,
    long? ReporteId,
    int CantidadCopias,
    bool VistaPrevia,
    bool ImprimirControladorFiscal,
    bool PermitirSeleccionMoneda,
    int? VarianteNroUnico,
    string? MascaraMoneda) : IRequest<Result>;

public record DeleteTipoComprobantePuntoFacturacionCommand(long PuntoFacturacionId, long TipoComprobantePuntoFacturacionId)
    : IRequest<Result>;

public class AddTipoComprobantePuntoFacturacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddTipoComprobantePuntoFacturacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddTipoComprobantePuntoFacturacionCommand request, CancellationToken ct)
    {
        var puntoExiste = await db.PuntosFacturacion.AnyAsync(x => x.Id == request.PuntoFacturacionId, ct);
        if (!puntoExiste)
            return Result.Failure<long>("Punto de facturacion no encontrado.");

        var duplicado = await db.TiposComprobantesPuntoFacturacion.AnyAsync(
            x => x.PuntoFacturacionId == request.PuntoFacturacionId && x.TipoComprobanteId == request.TipoComprobanteId,
            ct);

        if (duplicado)
            return Result.Failure<long>("El tipo de comprobante ya esta asignado a este punto de facturacion.");

        TipoComprobantePuntoFacturacion entity;
        try
        {
            entity = TipoComprobantePuntoFacturacion.Crear(
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.NumeroComprobanteProximo,
                request.Editable,
                request.FilasCantidad,
                request.FilasAnchoMaximo,
                request.ReporteId,
                request.CantidadCopias,
                request.VistaPrevia,
                request.ImprimirControladorFiscal,
                request.PermitirSeleccionMoneda,
                request.VarianteNroUnico,
                request.MascaraMoneda);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.TiposComprobantesPuntoFacturacion.Add(entity);
        await db.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}

public class UpdateTipoComprobantePuntoFacturacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTipoComprobantePuntoFacturacionCommand, Result>
{
    public async Task<Result> Handle(UpdateTipoComprobantePuntoFacturacionCommand request, CancellationToken ct)
    {
        var entity = await db.TiposComprobantesPuntoFacturacion.FirstOrDefaultAsync(
            x => x.Id == request.TipoComprobantePuntoFacturacionId && x.PuntoFacturacionId == request.PuntoFacturacionId,
            ct);

        if (entity is null)
            return Result.Failure("Tipo de comprobante no encontrado.");

        entity.Actualizar(
            request.NumeroComprobanteProximo,
            request.Editable,
            request.FilasCantidad,
            request.FilasAnchoMaximo,
            request.ReporteId,
            request.CantidadCopias,
            request.VistaPrevia,
            request.ImprimirControladorFiscal,
            request.PermitirSeleccionMoneda,
            request.VarianteNroUnico,
            request.MascaraMoneda);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteTipoComprobantePuntoFacturacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTipoComprobantePuntoFacturacionCommand, Result>
{
    public async Task<Result> Handle(DeleteTipoComprobantePuntoFacturacionCommand request, CancellationToken ct)
    {
        var entity = await db.TiposComprobantesPuntoFacturacion.FirstOrDefaultAsync(
            x => x.Id == request.TipoComprobantePuntoFacturacionId && x.PuntoFacturacionId == request.PuntoFacturacionId,
            ct);

        if (entity is null)
            return Result.Failure("Tipo de comprobante no encontrado.");

        db.TiposComprobantesPuntoFacturacion.Remove(entity);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AddConfiguracionFiscalCommandValidator : AbstractValidator<AddConfiguracionFiscalCommand>
{
    public AddConfiguracionFiscalCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.CopiasDefault).GreaterThan(0);
    }
}

public class UpdateConfiguracionFiscalCommandValidator : AbstractValidator<UpdateConfiguracionFiscalCommand>
{
    public UpdateConfiguracionFiscalCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.ConfiguracionFiscalId).GreaterThan(0);
        RuleFor(x => x.CopiasDefault).GreaterThan(0);
    }
}

public class DeleteConfiguracionFiscalCommandValidator : AbstractValidator<DeleteConfiguracionFiscalCommand>
{
    public DeleteConfiguracionFiscalCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.ConfiguracionFiscalId).GreaterThan(0);
    }
}

public class AddTipoComprobantePuntoFacturacionCommandValidator : AbstractValidator<AddTipoComprobantePuntoFacturacionCommand>
{
    public AddTipoComprobantePuntoFacturacionCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.NumeroComprobanteProximo).GreaterThan(0);
    }
}

public class UpdateTipoComprobantePuntoFacturacionCommandValidator : AbstractValidator<UpdateTipoComprobantePuntoFacturacionCommand>
{
    public UpdateTipoComprobantePuntoFacturacionCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.TipoComprobantePuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.NumeroComprobanteProximo).GreaterThan(0);
    }
}

public class DeleteTipoComprobantePuntoFacturacionCommandValidator : AbstractValidator<DeleteTipoComprobantePuntoFacturacionCommand>
{
    public DeleteTipoComprobantePuntoFacturacionCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.TipoComprobantePuntoFacturacionId).GreaterThan(0);
    }
}
