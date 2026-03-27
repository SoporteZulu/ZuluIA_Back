using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record ConciliarSifenParaguayPendientesCommand(
    int MaxItems = 20,
    long? SucursalId = null,
    string? EstadoSifen = null,
    string? CodigoRespuesta = null,
    bool? PuedeReintentar = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null)
    : IRequest<Result<ConciliacionSifenParaguayBatchResultDto>>;

public class ConciliacionSifenParaguayBatchResultDto
{
    public int MaxItems { get; set; }
    public long? SucursalId { get; set; }
    public int Encontrados { get; set; }
    public int TotalElegibles { get; set; }
    public bool HayMasResultados { get; set; }
    public int Procesados { get; set; }
    public int Exitosos { get; set; }
    public int Fallidos { get; set; }
    public List<ConciliacionSifenParaguayBatchItemDto> Items { get; set; } = [];
    public List<ConciliacionSifenParaguayBatchEstadoResumenDto> Estados { get; set; } = [];
    public List<ConciliacionSifenParaguayBatchErrorResumenDto> Errores { get; set; } = [];
}

public class ConciliacionSifenParaguayBatchEstadoResumenDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ConciliacionSifenParaguayBatchErrorResumenDto
{
    public string Error { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ConciliacionSifenParaguayBatchItemDto
{
    public long ComprobanteId { get; set; }
    public bool Exitoso { get; set; }
    public string? Estado { get; set; }
    public string? TrackingId { get; set; }
    public string? Cdc { get; set; }
    public string? NumeroLote { get; set; }
    public string? Error { get; set; }
}

public class ConciliarSifenParaguayPendientesCommandHandler(
    IApplicationDbContext db,
    IComprobanteRepository comprobanteRepo,
    IParaguaySifenComprobanteService paraguaySifenComprobanteService,
    IUnitOfWork uow)
    : IRequestHandler<ConciliarSifenParaguayPendientesCommand, Result<ConciliacionSifenParaguayBatchResultDto>>
{
    public async Task<Result<ConciliacionSifenParaguayBatchResultDto>> Handle(
        ConciliarSifenParaguayPendientesCommand request,
        CancellationToken ct)
    {
        if (request.MaxItems <= 0)
            return Result.Failure<ConciliacionSifenParaguayBatchResultDto>("MaxItems debe ser mayor a cero.");

        EstadoSifenParaguay? estadoSifen = null;
        if (!string.IsNullOrWhiteSpace(request.EstadoSifen)
            && Enum.TryParse<EstadoSifenParaguay>(request.EstadoSifen, true, out var parsedEstadoSifen))
        {
            estadoSifen = parsedEstadoSifen;
        }

        var codigoRespuesta = string.IsNullOrWhiteSpace(request.CodigoRespuesta)
            ? null
            : request.CodigoRespuesta.Trim();

        var query =
            from comprobante in db.Comprobantes.AsNoTracking()
            join sucursal in db.Sucursales.AsNoTracking() on comprobante.SucursalId equals sucursal.Id
            join pais in db.Paises.AsNoTracking() on sucursal.PaisId equals pais.Id
            where comprobante.Estado == EstadoComprobante.Emitido
                && (pais.Codigo == "PY" || pais.Codigo == "PRY")
                && comprobante.EstadoSifen != EstadoSifenParaguay.Aceptado
                && (comprobante.SifenTrackingId != null || comprobante.SifenCdc != null || comprobante.SifenNumeroLote != null)
                && (!request.SucursalId.HasValue || comprobante.SucursalId == request.SucursalId.Value)
                && (!estadoSifen.HasValue || comprobante.EstadoSifen == estadoSifen.Value)
                && (codigoRespuesta == null || comprobante.SifenCodigoRespuesta == codigoRespuesta)
                && (!request.FechaDesde.HasValue || comprobante.Fecha >= request.FechaDesde.Value)
                && (!request.FechaHasta.HasValue || comprobante.Fecha <= request.FechaHasta.Value)
                && (!request.PuedeReintentar.HasValue
                    || (request.PuedeReintentar.Value
                        ? comprobante.EstadoSifen == EstadoSifenParaguay.Rechazado || comprobante.EstadoSifen == EstadoSifenParaguay.Error
                        : comprobante.EstadoSifen == null || comprobante.EstadoSifen == EstadoSifenParaguay.Pendiente))
            orderby comprobante.SifenFechaRespuesta, comprobante.Id
            select comprobante.Id;

        var totalElegibles = await query.CountAsync(ct);

        var elegibles = await query
            .Take(request.MaxItems)
            .ToListAsync(ct);

        var response = new ConciliacionSifenParaguayBatchResultDto
        {
            MaxItems = request.MaxItems,
            SucursalId = request.SucursalId,
            Encontrados = elegibles.Count,
            TotalElegibles = totalElegibles,
            HayMasResultados = totalElegibles > elegibles.Count
        };

        foreach (var comprobanteId in elegibles)
        {
            var comprobante = await comprobanteRepo.GetByIdConItemsAsync(comprobanteId, ct);
            if (comprobante is null)
            {
                response.Items.Add(new ConciliacionSifenParaguayBatchItemDto
                {
                    ComprobanteId = comprobanteId,
                    Exitoso = false,
                    Error = $"No se encontro el comprobante ID {comprobanteId}."
                });
                response.Fallidos++;
                continue;
            }

            var result = await paraguaySifenComprobanteService.ConciliarEstadoAsync(comprobante, ct);
            response.Procesados++;

            if (result.IsFailure)
            {
                response.Items.Add(new ConciliacionSifenParaguayBatchItemDto
                {
                    ComprobanteId = comprobanteId,
                    Exitoso = false,
                    Error = result.Error,
                    TrackingId = comprobante.SifenTrackingId,
                    Cdc = comprobante.SifenCdc,
                    NumeroLote = comprobante.SifenNumeroLote
                });
                response.Fallidos++;
                continue;
            }

            comprobanteRepo.Update(comprobante);
            await uow.SaveChangesAsync(ct);

            response.Items.Add(new ConciliacionSifenParaguayBatchItemDto
            {
                ComprobanteId = comprobanteId,
                Exitoso = true,
                Estado = result.Value.Estado,
                TrackingId = result.Value.TrackingId,
                Cdc = result.Value.Cdc,
                NumeroLote = result.Value.NumeroLote
            });
            response.Exitosos++;
        }

        response.Fallidos = response.Items.Count(x => !x.Exitoso);
        response.Estados = response.Items
            .Where(x => x.Exitoso && !string.IsNullOrWhiteSpace(x.Estado))
            .GroupBy(x => x.Estado!)
            .Select(group => new ConciliacionSifenParaguayBatchEstadoResumenDto
            {
                Estado = group.Key,
                Cantidad = group.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ThenBy(x => x.Estado)
            .ToList();
        response.Errores = response.Items
            .Where(x => !x.Exitoso && !string.IsNullOrWhiteSpace(x.Error))
            .GroupBy(x => x.Error!)
            .Select(group => new ConciliacionSifenParaguayBatchErrorResumenDto
            {
                Error = group.Key,
                Cantidad = group.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ThenBy(x => x.Error)
            .ToList();
        return Result.Success(response);
    }
}