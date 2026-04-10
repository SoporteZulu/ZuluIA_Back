using MediatR;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Queries;

public class GetRequisicionesCompraPagedQueryHandler(IRequisicionCompraRepository repo, IApplicationDbContext db)
    : IRequestHandler<GetRequisicionesCompraPagedQuery, PagedResult<RequisicionCompraListDto>>
{
    public async Task<PagedResult<RequisicionCompraListDto>> Handle(
        GetRequisicionesCompraPagedQuery request, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.SolicitanteId, request.Estado, ct);

        var solicitanteIds = paged.Items.Select(x => x.SolicitanteId).Distinct().ToList();
        var solicitantes = await db.Usuarios
            .AsNoTrackingSafe()
            .Where(x => solicitanteIds.Contains(x.Id))
            .Select(x => new { x.Id, Nombre = x.NombreCompleto ?? x.UserName })
            .ToDictionarySafeAsync(x => x.Id, x => x.Nombre, ct);

        var items = paged.Items.Select(r => new RequisicionCompraListDto
        {
            Id             = r.Id,
            SucursalId     = r.SucursalId,
            SolicitanteId  = r.SolicitanteId,
            SolicitanteNombre = solicitantes.GetValueOrDefault(r.SolicitanteId, string.Empty),
            Fecha          = r.Fecha,
            Descripcion    = r.Descripcion,
            Estado         = r.Estado.ToString(),
            EstadoLegacy   = RequisicionCompraQueryMappings.MapEstadoRequisicionLegacy(r.Estado),
            CantidadItems  = r.Items.Count,
            CreatedAt      = r.CreatedAt
        }).ToList();

        return new PagedResult<RequisicionCompraListDto>(items, request.Page, request.PageSize, paged.TotalCount);
    }
}

public class GetRequisicionCompraDetalleQueryHandler(IRequisicionCompraRepository repo, IApplicationDbContext db)
    : IRequestHandler<GetRequisicionCompraDetalleQuery, RequisicionCompraDto?>
{
    public async Task<RequisicionCompraDto?> Handle(
        GetRequisicionCompraDetalleQuery request, CancellationToken ct)
    {
        var req = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (req is null) return null;

        var solicitanteNombre = await db.Usuarios
            .AsNoTrackingSafe()
            .Where(x => x.Id == req.SolicitanteId)
            .Select(x => x.NombreCompleto ?? x.UserName)
            .FirstOrDefaultSafeAsync(ct)
            ?? string.Empty;

        var itemIds = req.Items.Where(x => x.ItemId.HasValue).Select(x => x.ItemId!.Value).Distinct().ToList();
        var codigos = await db.Items
            .AsNoTrackingSafe()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo })
            .ToDictionarySafeAsync(x => x.Id, x => x.Codigo, ct);

        return new RequisicionCompraDto
        {
            Id            = req.Id,
            SucursalId    = req.SucursalId,
            SolicitanteId = req.SolicitanteId,
            SolicitanteNombre = solicitanteNombre,
            Fecha         = req.Fecha,
            Descripcion   = req.Descripcion,
            Estado        = req.Estado.ToString(),
            EstadoLegacy  = RequisicionCompraQueryMappings.MapEstadoRequisicionLegacy(req.Estado),
            Observacion   = req.Observacion,
            CantidadItems = req.Items.Count,
            CreatedAt     = req.CreatedAt,
            Items         = req.Items.Select(i => new RequisicionCompraItemDto
            {
                Id           = i.Id,
                ItemId       = i.ItemId,
                Codigo       = i.ItemId.HasValue ? codigos.GetValueOrDefault(i.ItemId.Value, string.Empty) : string.Empty,
                Descripcion  = i.Descripcion,
                Cantidad     = i.Cantidad,
                UnidadMedida = i.UnidadMedida,
                Observacion  = i.Observacion
            }).ToList()
        };
    }
}

internal static class RequisicionCompraQueryMappings
{
    public static string MapEstadoRequisicionLegacy(Domain.Enums.EstadoRequisicion estado) => estado switch
    {
        Domain.Enums.EstadoRequisicion.Borrador => "ABIERTA",
        Domain.Enums.EstadoRequisicion.Enviada => "EN_PROCESO",
        Domain.Enums.EstadoRequisicion.Aprobada => "EN_PROCESO",
        Domain.Enums.EstadoRequisicion.Rechazada => "CANCELADA",
        Domain.Enums.EstadoRequisicion.Cancelada => "CANCELADA",
        Domain.Enums.EstadoRequisicion.Procesada => "COTIZADA",
        _ => estado.ToString().ToUpperInvariant()
    };
}
