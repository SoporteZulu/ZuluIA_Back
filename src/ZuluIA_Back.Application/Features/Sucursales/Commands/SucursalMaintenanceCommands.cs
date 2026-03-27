using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Sucursales;

namespace ZuluIA_Back.Application.Features.Sucursales.Commands;

public record CreateAreaCommand(string Descripcion, string? Codigo, long? SucursalId) : IRequest<Result<long>>;
public record UpdateAreaCommand(long Id, string Descripcion, string? Codigo, long? SucursalId) : IRequest<Result>;
public record DeleteAreaCommand(long Id) : IRequest<Result>;

public class CreateAreaCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateAreaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateAreaCommand request, CancellationToken ct)
    {
        Area area;
        try
        {
            area = Area.Crear(request.Descripcion, request.Codigo, request.SucursalId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.Areas.Add(area);
        await db.SaveChangesAsync(ct);
        return Result.Success(area.Id);
    }
}

public class UpdateAreaCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateAreaCommand, Result>
{
    public async Task<Result> Handle(UpdateAreaCommand request, CancellationToken ct)
    {
        var area = await db.Areas.FindAsync([request.Id], ct);
        if (area is null)
            return Result.Failure($"Area {request.Id} no encontrada.");

        try
        {
            area.Actualizar(request.Descripcion, request.Codigo, request.SucursalId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteAreaCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteAreaCommand, Result>
{
    public async Task<Result> Handle(DeleteAreaCommand request, CancellationToken ct)
    {
        var area = await db.Areas.FindAsync([request.Id], ct);
        if (area is null)
            return Result.Failure($"Area {request.Id} no encontrada.");

        db.Areas.Remove(area);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record CreateTipoComprobanteSucursalCommand(
    long SucursalId,
    long TipoComprobanteId,
    long NumeroProximo,
    int FilasCantidad,
    int FilasAnchoMaximo,
    int CantidadCopias,
    bool ImprimirControladorFiscal,
    bool VarianteNroUnico,
    bool PermitirSeleccionMoneda,
    long? MonedaId,
    bool Editable,
    bool VistaPrevia,
    bool ControlIntervalo,
    long? NumeroDesde,
    long? NumeroHasta) : IRequest<Result<long>>;

public record UpdateTipoComprobanteSucursalCommand(
    long SucursalId,
    long ConfiguracionId,
    int FilasCantidad,
    int FilasAnchoMaximo,
    int CantidadCopias,
    bool ImprimirControladorFiscal,
    bool VarianteNroUnico,
    bool PermitirSeleccionMoneda,
    long? MonedaId,
    bool Editable,
    bool VistaPrevia,
    bool ControlIntervalo,
    long? NumeroDesde,
    long? NumeroHasta) : IRequest<Result>;

public class CreateTipoComprobanteSucursalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateTipoComprobanteSucursalCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTipoComprobanteSucursalCommand request, CancellationToken ct)
    {
        var existe = await db.TiposComprobantesSucursal.AnyAsync(
            t => t.TipoComprobanteId == request.TipoComprobanteId && t.SucursalId == request.SucursalId,
            ct);

        if (existe)
            return Result.Failure<long>("Ya existe configuracion para ese tipo de comprobante en esta sucursal.");

        TipoComprobanteSucursal config;
        try
        {
            config = TipoComprobanteSucursal.Crear(
                request.TipoComprobanteId,
                request.SucursalId,
                request.NumeroProximo,
                request.FilasCantidad,
                request.FilasAnchoMaximo,
                request.CantidadCopias,
                request.ImprimirControladorFiscal,
                request.VarianteNroUnico,
                request.PermitirSeleccionMoneda,
                request.MonedaId,
                request.Editable,
                request.VistaPrevia,
                request.ControlIntervalo,
                request.NumeroDesde,
                request.NumeroHasta);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.TiposComprobantesSucursal.Add(config);
        await db.SaveChangesAsync(ct);
        return Result.Success(config.Id);
    }
}

public class UpdateTipoComprobanteSucursalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTipoComprobanteSucursalCommand, Result>
{
    public async Task<Result> Handle(UpdateTipoComprobanteSucursalCommand request, CancellationToken ct)
    {
        var config = await db.TiposComprobantesSucursal.FirstOrDefaultAsync(
            t => t.Id == request.ConfiguracionId && t.SucursalId == request.SucursalId,
            ct);

        if (config is null)
            return Result.Failure($"Configuracion {request.ConfiguracionId} no encontrada.");

        config.ActualizarConfiguracion(
            request.FilasCantidad,
            request.FilasAnchoMaximo,
            request.CantidadCopias,
            request.ImprimirControladorFiscal,
            request.VarianteNroUnico,
            request.PermitirSeleccionMoneda,
            request.MonedaId,
            request.Editable,
            request.VistaPrevia,
            request.ControlIntervalo,
            request.NumeroDesde,
            request.NumeroHasta);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record AddSucursalDomicilioCommand(
    long SucursalId,
    long? TipoDomicilioId,
    long? ProvinciaId,
    long? LocalidadId,
    string? Calle,
    string? Barrio,
    string? CodigoPostal,
    string? Observacion,
    int Orden,
    bool EsDefecto) : IRequest<Result<long>>;

public record UpdateSucursalDomicilioCommand(
    long SucursalId,
    long DomicilioId,
    long? TipoDomicilioId,
    long? ProvinciaId,
    long? LocalidadId,
    string? Calle,
    string? Barrio,
    string? CodigoPostal,
    string? Observacion,
    int Orden,
    bool EsDefecto) : IRequest<Result>;

public record DeleteSucursalDomicilioCommand(long SucursalId, long DomicilioId) : IRequest<Result>;

public class AddSucursalDomicilioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddSucursalDomicilioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddSucursalDomicilioCommand request, CancellationToken ct)
    {
        var sucursalExiste = await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId, ct);
        if (!sucursalExiste)
            return Result.Failure<long>("Sucursal no encontrada.");

        SucursalDomicilio domicilio;
        try
        {
            domicilio = SucursalDomicilio.Crear(
                request.SucursalId,
                request.TipoDomicilioId,
                request.ProvinciaId,
                request.LocalidadId,
                request.Calle,
                request.Barrio,
                request.CodigoPostal,
                request.Observacion,
                request.Orden,
                request.EsDefecto);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.SucursalesDomicilio.Add(domicilio);
        await db.SaveChangesAsync(ct);
        return Result.Success(domicilio.Id);
    }
}

public class UpdateSucursalDomicilioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSucursalDomicilioCommand, Result>
{
    public async Task<Result> Handle(UpdateSucursalDomicilioCommand request, CancellationToken ct)
    {
        var domicilio = await db.SucursalesDomicilio.FirstOrDefaultAsync(
            d => d.Id == request.DomicilioId && d.SucursalId == request.SucursalId,
            ct);

        if (domicilio is null)
            return Result.Failure("Domicilio no encontrado.");

        domicilio.Actualizar(
            request.TipoDomicilioId,
            request.ProvinciaId,
            request.LocalidadId,
            request.Calle,
            request.Barrio,
            request.CodigoPostal,
            request.Observacion,
            request.Orden,
            request.EsDefecto);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteSucursalDomicilioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteSucursalDomicilioCommand, Result>
{
    public async Task<Result> Handle(DeleteSucursalDomicilioCommand request, CancellationToken ct)
    {
        var domicilio = await db.SucursalesDomicilio.FirstOrDefaultAsync(
            d => d.Id == request.DomicilioId && d.SucursalId == request.SucursalId,
            ct);

        if (domicilio is null)
            return Result.Failure("Domicilio no encontrado.");

        db.SucursalesDomicilio.Remove(domicilio);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record AddSucursalMedioContactoCommand(
    long SucursalId,
    string Valor,
    long? TipoMedioContactoId,
    int Orden,
    bool EsDefecto,
    string? Observacion) : IRequest<Result<long>>;

public record UpdateSucursalMedioContactoCommand(
    long SucursalId,
    long MedioContactoId,
    string Valor,
    long? TipoMedioContactoId,
    int Orden,
    bool EsDefecto,
    string? Observacion) : IRequest<Result>;

public record DeleteSucursalMedioContactoCommand(long SucursalId, long MedioContactoId) : IRequest<Result>;

public class AddSucursalMedioContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddSucursalMedioContactoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddSucursalMedioContactoCommand request, CancellationToken ct)
    {
        var sucursalExiste = await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId, ct);
        if (!sucursalExiste)
            return Result.Failure<long>("Sucursal no encontrada.");

        SucursalMedioContacto medio;
        try
        {
            medio = SucursalMedioContacto.Crear(
                request.SucursalId,
                request.Valor,
                request.TipoMedioContactoId,
                request.Orden,
                request.EsDefecto,
                request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.SucursalesMedioContacto.Add(medio);
        await db.SaveChangesAsync(ct);
        return Result.Success(medio.Id);
    }
}

public class UpdateSucursalMedioContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSucursalMedioContactoCommand, Result>
{
    public async Task<Result> Handle(UpdateSucursalMedioContactoCommand request, CancellationToken ct)
    {
        var medio = await db.SucursalesMedioContacto.FirstOrDefaultAsync(
            m => m.Id == request.MedioContactoId && m.SucursalId == request.SucursalId,
            ct);

        if (medio is null)
            return Result.Failure("Medio de contacto no encontrado.");

        try
        {
            medio.Actualizar(
                request.Valor,
                request.TipoMedioContactoId,
                request.Orden,
                request.EsDefecto,
                request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteSucursalMedioContactoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteSucursalMedioContactoCommand, Result>
{
    public async Task<Result> Handle(DeleteSucursalMedioContactoCommand request, CancellationToken ct)
    {
        var medio = await db.SucursalesMedioContacto.FirstOrDefaultAsync(
            m => m.Id == request.MedioContactoId && m.SucursalId == request.SucursalId,
            ct);

        if (medio is null)
            return Result.Failure("Medio de contacto no encontrado.");

        db.SucursalesMedioContacto.Remove(medio);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateAreaCommandValidator : AbstractValidator<CreateAreaCommand>
{
    public CreateAreaCommandValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateAreaCommandValidator : AbstractValidator<UpdateAreaCommand>
{
    public UpdateAreaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteAreaCommandValidator : AbstractValidator<DeleteAreaCommand>
{
    public DeleteAreaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateTipoComprobanteSucursalCommandValidator : AbstractValidator<CreateTipoComprobanteSucursalCommand>
{
    public CreateTipoComprobanteSucursalCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.NumeroProximo).GreaterThan(0);
    }
}

public class UpdateTipoComprobanteSucursalCommandValidator : AbstractValidator<UpdateTipoComprobanteSucursalCommand>
{
    public UpdateTipoComprobanteSucursalCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.ConfiguracionId).GreaterThan(0);
    }
}

public class AddSucursalDomicilioCommandValidator : AbstractValidator<AddSucursalDomicilioCommand>
{
    public AddSucursalDomicilioCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
    }
}

public class UpdateSucursalDomicilioCommandValidator : AbstractValidator<UpdateSucursalDomicilioCommand>
{
    public UpdateSucursalDomicilioCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.DomicilioId).GreaterThan(0);
    }
}

public class DeleteSucursalDomicilioCommandValidator : AbstractValidator<DeleteSucursalDomicilioCommand>
{
    public DeleteSucursalDomicilioCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.DomicilioId).GreaterThan(0);
    }
}

public class AddSucursalMedioContactoCommandValidator : AbstractValidator<AddSucursalMedioContactoCommand>
{
    public AddSucursalMedioContactoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class UpdateSucursalMedioContactoCommandValidator : AbstractValidator<UpdateSucursalMedioContactoCommand>
{
    public UpdateSucursalMedioContactoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.MedioContactoId).GreaterThan(0);
        RuleFor(x => x.Valor).NotEmpty();
    }
}

public class DeleteSucursalMedioContactoCommandValidator : AbstractValidator<DeleteSucursalMedioContactoCommand>
{
    public DeleteSucursalMedioContactoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.MedioContactoId).GreaterThan(0);
    }
}
