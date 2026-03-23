using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Caea.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Caea.Commands;

public record SolicitarCaeaCommand(
    long PuntoFacturacionId,
    string NroCaea,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    string TipoComprobante,
    int CantidadAsignada)
    : IRequest<Result<long>>;

public record SolicitarCaeaAfipCommand(
    long PuntoFacturacionId,
    int Periodo,
    short Orden,
    string TipoComprobante,
    int CantidadAsignada)
    : IRequest<Result<long>>;

public record InformarCaeaCommand(long Id) : IRequest<Result>;
public record AnularCaeaCommand(long Id) : IRequest<Result>;

public class SolicitarCaeaCommandHandler(
    IRepository<Domain.Entities.Facturacion.Caea> caeaRepo,
    IRepository<AuditoriaCaea> auditoriaRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<SolicitarCaeaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(SolicitarCaeaCommand request, CancellationToken ct)
    {
        var caea = Domain.Entities.Facturacion.Caea.Crear(
            request.PuntoFacturacionId,
            request.NroCaea,
            request.FechaDesde,
            request.FechaHasta,
            null,
            null,
            request.TipoComprobante,
            request.CantidadAsignada,
            currentUser.UserId);

        await caeaRepo.AddAsync(caea, ct);
        await uow.SaveChangesAsync(ct);
        await auditoriaRepo.AddAsync(AuditoriaCaea.Registrar(
            caea.Id,
            currentUser.UserId,
            AccionAuditoria.Creado,
            "CAEA creado manualmente.",
            null), ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(caea.Id);
    }
}

public class SolicitarCaeaAfipCommandHandler(
    IAfipWsfeCaeaService afipWsfeCaeaService,
    IRepository<Domain.Entities.Facturacion.Caea> caeaRepo,
    IRepository<AuditoriaCaea> auditoriaRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<SolicitarCaeaAfipCommand, Result<long>>
{
    public async Task<Result<long>> Handle(SolicitarCaeaAfipCommand request, CancellationToken ct)
    {
        try
        {
            var afipResponse = await afipWsfeCaeaService.SolicitarCaeaAsync(
                new SolicitarCaeaAfipRequest(request.Periodo, request.Orden),
                ct);

            var (fechaDesde, fechaHasta) = ObtenerVentana(request.Periodo, request.Orden);

            var caea = Domain.Entities.Facturacion.Caea.Crear(
                request.PuntoFacturacionId,
                afipResponse.NroCaea,
                fechaDesde,
                fechaHasta,
                afipResponse.FechaProceso,
                afipResponse.FechaTopeInformar,
                request.TipoComprobante,
                request.CantidadAsignada,
                currentUser.UserId);

            await caeaRepo.AddAsync(caea, ct);
            await uow.SaveChangesAsync(ct);
            await auditoriaRepo.AddAsync(AuditoriaCaea.Registrar(
                caea.Id,
                currentUser.UserId,
                AccionAuditoria.AfipSolicitud,
                $"CAEA solicitado a AFIP. nroCaea={afipResponse.NroCaea}; fechaProceso={afipResponse.FechaProceso:yyyy-MM-dd}; fechaTopeInformar={afipResponse.FechaTopeInformar:yyyy-MM-dd}",
                null), ct);
            await uow.SaveChangesAsync(ct);

            return Result.Success(caea.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }

    private static (DateOnly Desde, DateOnly Hasta) ObtenerVentana(int periodo, short orden)
    {
        var anio = periodo / 100;
        var mes = periodo % 100;
        var ultimoDia = DateTime.DaysInMonth(anio, mes);

        return orden == 1
            ? (new DateOnly(anio, mes, 1), new DateOnly(anio, mes, 15))
            : (new DateOnly(anio, mes, 16), new DateOnly(anio, mes, ultimoDia));
    }
}

public class InformarCaeaCommandHandler(
    IRepository<Domain.Entities.Facturacion.Caea> caeaRepo,
    IRepository<AuditoriaCaea> auditoriaRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<InformarCaeaCommand, Result>
{
    public async Task<Result> Handle(InformarCaeaCommand request, CancellationToken ct)
    {
        var caea = await caeaRepo.GetByIdAsync(request.Id, ct);
        if (caea is null) return Result.Failure("CAEA no encontrado.");
        caea.MarcarInformado(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        await auditoriaRepo.AddAsync(AuditoriaCaea.Registrar(
            caea.Id,
            currentUser.UserId,
            AccionAuditoria.Aprobado,
            "CAEA marcado como informado.",
            null), ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AnularCaeaCommandHandler(
    IRepository<Domain.Entities.Facturacion.Caea> caeaRepo,
    IRepository<AuditoriaCaea> auditoriaRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularCaeaCommand, Result>
{
    public async Task<Result> Handle(AnularCaeaCommand request, CancellationToken ct)
    {
        var caea = await caeaRepo.GetByIdAsync(request.Id, ct);
        if (caea is null) return Result.Failure("CAEA no encontrado.");
        caea.Anular(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        await auditoriaRepo.AddAsync(AuditoriaCaea.Registrar(
            caea.Id,
            currentUser.UserId,
            AccionAuditoria.Anulado,
            "CAEA anulado.",
            null), ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
