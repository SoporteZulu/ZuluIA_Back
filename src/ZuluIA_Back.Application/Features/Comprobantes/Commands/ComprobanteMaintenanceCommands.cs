using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record AsignarCaeComprobanteCommand(
    long ComprobanteId,
    string Cae,
    DateOnly FechaVto,
    string? QrData) : IRequest<Result>;

public class AsignarCaeComprobanteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AsignarCaeComprobanteCommand, Result>
{
    public async Task<Result> Handle(AsignarCaeComprobanteCommand request, CancellationToken ct)
    {
        var comprobante = await db.Comprobantes.FirstOrDefaultAsync(x => x.Id == request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure($"No se encontro el comprobante ID {request.ComprobanteId}.");

        try
        {
            comprobante.AsignarCae(request.Cae, request.FechaVto, request.QrData, null);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record SolicitarCaeAfipComprobanteCommand(long ComprobanteId) : IRequest<Result>;

public class SolicitarCaeAfipComprobanteCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IAfipCaeComprobanteService afipCaeComprobanteService,
    IUnitOfWork uow)
    : IRequestHandler<SolicitarCaeAfipComprobanteCommand, Result>
{
    public async Task<Result> Handle(SolicitarCaeAfipComprobanteCommand request, CancellationToken ct)
    {
        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure($"No se encontro el comprobante ID {request.ComprobanteId}.");
        var result = await afipCaeComprobanteService.SolicitarYAsignarAsync(comprobante, ct);
        if (result.IsFailure)
            return result;

        comprobanteRepo.Update(comprobante);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record SolicitarSifenParaguayComprobanteCommand(long ComprobanteId)
    : IRequest<Result<ResultadoEnvioSifenParaguayDto>>;

public record ReintentarSifenParaguayComprobanteCommand(long ComprobanteId)
    : IRequest<Result<ResultadoEnvioSifenParaguayDto>>;

public record ConciliarSifenParaguayComprobanteCommand(long ComprobanteId)
    : IRequest<Result<ResultadoEnvioSifenParaguayDto>>;

public class SolicitarSifenParaguayComprobanteCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IParaguaySifenComprobanteService paraguaySifenComprobanteService,
    IUnitOfWork uow)
    : IRequestHandler<SolicitarSifenParaguayComprobanteCommand, Result<ResultadoEnvioSifenParaguayDto>>
{
    public async Task<Result<ResultadoEnvioSifenParaguayDto>> Handle(
        SolicitarSifenParaguayComprobanteCommand request,
        CancellationToken ct)
    {
        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>($"No se encontro el comprobante ID {request.ComprobanteId}.");

        if (comprobante.EstadoSifen == Domain.Enums.EstadoSifenParaguay.Aceptado)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>("El comprobante ya fue aceptado por SIFEN/SET y no debe reenviarse.");

        var result = await paraguaySifenComprobanteService.EnviarAsync(comprobante, ct);
        if (result.IsFailure)
            return result;

        comprobanteRepo.Update(comprobante);
        await uow.SaveChangesAsync(ct);
        return result;
    }
}

public class ReintentarSifenParaguayComprobanteCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IParaguaySifenComprobanteService paraguaySifenComprobanteService,
    IUnitOfWork uow)
    : IRequestHandler<ReintentarSifenParaguayComprobanteCommand, Result<ResultadoEnvioSifenParaguayDto>>
{
    public async Task<Result<ResultadoEnvioSifenParaguayDto>> Handle(
        ReintentarSifenParaguayComprobanteCommand request,
        CancellationToken ct)
    {
        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>($"No se encontro el comprobante ID {request.ComprobanteId}.");

        if (comprobante.EstadoSifen == Domain.Enums.EstadoSifenParaguay.Aceptado)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>("El comprobante ya fue aceptado por SIFEN/SET y no debe reenviarse.");

        if (comprobante.EstadoSifen != Domain.Enums.EstadoSifenParaguay.Rechazado
            && comprobante.EstadoSifen != Domain.Enums.EstadoSifenParaguay.Error)
        {
            return Result.Failure<ResultadoEnvioSifenParaguayDto>("Solo se puede reintentar un comprobante con ultimo estado rechazado o error.");
        }

        var result = await paraguaySifenComprobanteService.EnviarAsync(comprobante, ct);
        if (result.IsFailure)
            return result;

        comprobanteRepo.Update(comprobante);
        await uow.SaveChangesAsync(ct);
        return result;
    }
}

public class ConciliarSifenParaguayComprobanteCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IParaguaySifenComprobanteService paraguaySifenComprobanteService,
    IUnitOfWork uow)
    : IRequestHandler<ConciliarSifenParaguayComprobanteCommand, Result<ResultadoEnvioSifenParaguayDto>>
{
    public async Task<Result<ResultadoEnvioSifenParaguayDto>> Handle(
        ConciliarSifenParaguayComprobanteCommand request,
        CancellationToken ct)
    {
        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>($"No se encontro el comprobante ID {request.ComprobanteId}.");

        var result = await paraguaySifenComprobanteService.ConciliarEstadoAsync(comprobante, ct);
        if (result.IsFailure)
            return result;

        comprobanteRepo.Update(comprobante);
        await uow.SaveChangesAsync(ct);
        return result;
    }
}

public record CreateTipoEntregaCommand(string Codigo, string Descripcion, long? TipoComprobanteId, string? Prefijo)
    : IRequest<Result<long>>;

public record UpdateTipoEntregaCommand(long Id, string Descripcion, long? TipoComprobanteId, string? Prefijo)
    : IRequest<Result>;

public class CreateTipoEntregaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateTipoEntregaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTipoEntregaCommand request, CancellationToken ct)
    {
        var exists = await db.TiposEntrega.AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct);
        if (exists)
            return Result.Failure<long>("Ya existe un tipo de entrega con ese codigo.");

        TipoEntrega tipo;
        try
        {
            tipo = TipoEntrega.Crear(request.Codigo, request.Descripcion, request.TipoComprobanteId, request.Prefijo);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.TiposEntrega.Add(tipo);
        await db.SaveChangesAsync(ct);

        return Result.Success(tipo.Id);
    }
}

public class UpdateTipoEntregaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTipoEntregaCommand, Result>
{
    public async Task<Result> Handle(UpdateTipoEntregaCommand request, CancellationToken ct)
    {
        var tipo = await db.TiposEntrega.FindAsync([request.Id], ct);
        if (tipo is null)
            return Result.Failure("Tipo de entrega no encontrado.");

        try
        {
            tipo.Actualizar(request.Descripcion, request.TipoComprobanteId, request.Prefijo);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record CreateComprobanteEntregaCommand(
    long ComprobanteId,
    DateOnly Fecha,
    string? RazonSocial,
    string? Domicilio,
    long? LocalidadId,
    long? ProvinciaId,
    long? PaisId,
    string? CodigoPostal,
    string? Telefono1,
    string? Telefono2,
    string? Celular,
    string? Email,
    string? Observacion,
    long? TipoEntregaId,
    long? TransportistaId,
    long? ZonaId) : IRequest<Result<long>>;

public record UpdateComprobanteEntregaCommand(
    long ComprobanteId,
    string? RazonSocial,
    string? Domicilio,
    long? LocalidadId,
    long? ProvinciaId,
    long? PaisId,
    string? CodigoPostal,
    string? Telefono1,
    string? Telefono2,
    string? Celular,
    string? Email,
    string? Observacion,
    long? TipoEntregaId,
    long? TransportistaId,
    long? ZonaId) : IRequest<Result>;

public class CreateComprobanteEntregaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateComprobanteEntregaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateComprobanteEntregaCommand request, CancellationToken ct)
    {
        var exists = await db.Comprobantes.AnyAsync(x => x.Id == request.ComprobanteId, ct);
        if (!exists)
            return Result.Failure<long>("Comprobante no encontrado.");

        var dup = await db.ComprobantesEntregas.AnyAsync(x => x.ComprobanteId == request.ComprobanteId, ct);
        if (dup)
            return Result.Failure<long>("El comprobante ya tiene datos de entrega. Use PUT para actualizar.");

        ComprobanteEntrega entrega;
        try
        {
            entrega = ComprobanteEntrega.Crear(
                request.ComprobanteId,
                request.Fecha,
                request.RazonSocial,
                request.Domicilio,
                request.LocalidadId,
                request.ProvinciaId,
                request.PaisId,
                request.CodigoPostal,
                request.Telefono1,
                request.Telefono2,
                request.Celular,
                request.Email,
                request.Observacion,
                request.TipoEntregaId,
                request.TransportistaId,
                request.ZonaId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.ComprobantesEntregas.Add(entrega);
        await db.SaveChangesAsync(ct);

        return Result.Success(entrega.Id);
    }
}

public class UpdateComprobanteEntregaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateComprobanteEntregaCommand, Result>
{
    public async Task<Result> Handle(UpdateComprobanteEntregaCommand request, CancellationToken ct)
    {
        var entrega = await db.ComprobantesEntregas.FirstOrDefaultAsync(x => x.ComprobanteId == request.ComprobanteId, ct);
        if (entrega is null)
            return Result.Failure("Entrega no encontrada.");

        entrega.Actualizar(
            request.RazonSocial,
            request.Domicilio,
            request.LocalidadId,
            request.ProvinciaId,
            request.PaisId,
            request.CodigoPostal,
            request.Telefono1,
            request.Telefono2,
            request.Celular,
            request.Email,
            request.Observacion,
            request.TipoEntregaId,
            request.TransportistaId,
            request.ZonaId);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record AddComprobanteDetalleCostoCommand(long ComprobanteId, long ComprobanteItemId, long CentroCostoId)
    : IRequest<Result<long>>;

public record ProcesarComprobanteDetalleCostoCommand(long ComprobanteId, long DetalleCostoId, bool Procesar)
    : IRequest<Result<bool>>;

public class AddComprobanteDetalleCostoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddComprobanteDetalleCostoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddComprobanteDetalleCostoCommand request, CancellationToken ct)
    {
        var itemExists = await db.ComprobantesItems
            .AnyAsync(x => x.Id == request.ComprobanteItemId && x.ComprobanteId == request.ComprobanteId, ct);

        if (!itemExists)
            return Result.Failure<long>("El item no pertenece al comprobante indicado.");

        ComprobanteDetalleCosto detalle;
        try
        {
            detalle = ComprobanteDetalleCosto.Crear(request.ComprobanteItemId, request.CentroCostoId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.ComprobantesDetallesCostos.Add(detalle);
        await db.SaveChangesAsync(ct);

        return Result.Success(detalle.Id);
    }
}

public class ProcesarComprobanteDetalleCostoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ProcesarComprobanteDetalleCostoCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ProcesarComprobanteDetalleCostoCommand request, CancellationToken ct)
    {
        var detalle = await db.ComprobantesDetallesCostos
            .FirstOrDefaultAsync(
                x => x.Id == request.DetalleCostoId
                     && db.ComprobantesItems
                         .Where(ci => ci.ComprobanteId == request.ComprobanteId)
                         .Select(ci => ci.Id)
                         .Contains(x.ComprobanteItemId),
                ct);

        if (detalle is null)
            return Result.Failure<bool>("Detalle de costo no encontrado.");

        if (request.Procesar) detalle.MarcarProcesado();
        else detalle.DesmarcarProcesado();

        await db.SaveChangesAsync(ct);
        return Result.Success(detalle.Procesado);
    }
}

public record AddComprobanteFormaPagoCommand(
    long ComprobanteId,
    long FormaPagoId,
    DateOnly Fecha,
    decimal Importe,
    string? Descripcion,
    string? Observacion,
    long? MonedaId,
    decimal Cotizacion) : IRequest<Result<long>>;

public record AnularComprobanteFormaPagoCommand(long ComprobanteId, long ComprobanteFormaPagoId)
    : IRequest<Result>;

public class AddComprobanteFormaPagoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddComprobanteFormaPagoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddComprobanteFormaPagoCommand request, CancellationToken ct)
    {
        ComprobanteFormaPago forma;
        try
        {
            forma = ComprobanteFormaPago.Crear(
                request.ComprobanteId,
                request.FormaPagoId,
                request.Fecha,
                request.Importe,
                request.Descripcion,
                request.Observacion,
                request.MonedaId,
                request.Cotizacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.ComprobantesFormasPago.Add(forma);
        await db.SaveChangesAsync(ct);

        return Result.Success(forma.Id);
    }
}

public class AnularComprobanteFormaPagoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AnularComprobanteFormaPagoCommand, Result>
{
    public async Task<Result> Handle(AnularComprobanteFormaPagoCommand request, CancellationToken ct)
    {
        var forma = await db.ComprobantesFormasPago
            .FirstOrDefaultAsync(x => x.Id == request.ComprobanteFormaPagoId && x.ComprobanteId == request.ComprobanteId, ct);

        if (forma is null)
            return Result.Failure($"Forma de pago {request.ComprobanteFormaPagoId} no encontrada en comprobante {request.ComprobanteId}.");

        forma.Anular();
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AsignarCaeComprobanteCommandValidator : AbstractValidator<AsignarCaeComprobanteCommand>
{
    public AsignarCaeComprobanteCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.Cae).NotEmpty();
    }
}

public class CreateTipoEntregaCommandValidator : AbstractValidator<CreateTipoEntregaCommand>
{
    public CreateTipoEntregaCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateTipoEntregaCommandValidator : AbstractValidator<UpdateTipoEntregaCommand>
{
    public UpdateTipoEntregaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class CreateComprobanteEntregaCommandValidator : AbstractValidator<CreateComprobanteEntregaCommand>
{
    public CreateComprobanteEntregaCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}

public class UpdateComprobanteEntregaCommandValidator : AbstractValidator<UpdateComprobanteEntregaCommand>
{
    public UpdateComprobanteEntregaCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}

public class AddComprobanteDetalleCostoCommandValidator : AbstractValidator<AddComprobanteDetalleCostoCommand>
{
    public AddComprobanteDetalleCostoCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.ComprobanteItemId).GreaterThan(0);
        RuleFor(x => x.CentroCostoId).GreaterThan(0);
    }
}

public class ProcesarComprobanteDetalleCostoCommandValidator : AbstractValidator<ProcesarComprobanteDetalleCostoCommand>
{
    public ProcesarComprobanteDetalleCostoCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.DetalleCostoId).GreaterThan(0);
    }
}

public class AddComprobanteFormaPagoCommandValidator : AbstractValidator<AddComprobanteFormaPagoCommand>
{
    public AddComprobanteFormaPagoCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.FormaPagoId).GreaterThan(0);
        RuleFor(x => x.Importe).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
    }
}

public class AnularComprobanteFormaPagoCommandValidator : AbstractValidator<AnularComprobanteFormaPagoCommand>
{
    public AnularComprobanteFormaPagoCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.ComprobanteFormaPagoId).GreaterThan(0);
    }
}
