using MediatR;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Queries;

public class GetRequisicionesCompraPagedQueryHandler(IRequisicionCompraRepository repo)
    : IRequestHandler<GetRequisicionesCompraPagedQuery, PagedResult<RequisicionCompraListDto>>
{
    public async Task<PagedResult<RequisicionCompraListDto>> Handle(
        GetRequisicionesCompraPagedQuery request, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.SolicitanteId, request.Estado, ct);

        var items = paged.Items.Select(r => new RequisicionCompraListDto
        {
            Id             = r.Id,
            SucursalId     = r.SucursalId,
            SolicitanteId  = r.SolicitanteId,
            Fecha          = r.Fecha,
            Descripcion    = r.Descripcion,
            Estado         = r.Estado.ToString(),
            CantidadItems  = r.Items.Count,
            CreatedAt      = r.CreatedAt
        }).ToList();

        return new PagedResult<RequisicionCompraListDto>(items, paged.TotalCount, request.Page, request.PageSize);
    }
}

public class GetRequisicionCompraDetalleQueryHandler(IRequisicionCompraRepository repo)
    : IRequestHandler<GetRequisicionCompraDetalleQuery, RequisicionCompraDto?>
{
    public async Task<RequisicionCompraDto?> Handle(
        GetRequisicionCompraDetalleQuery request, CancellationToken ct)
    {
        var req = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (req is null) return null;

        return new RequisicionCompraDto
        {
            Id            = req.Id,
            SucursalId    = req.SucursalId,
            SolicitanteId = req.SolicitanteId,
            Fecha         = req.Fecha,
            Descripcion   = req.Descripcion,
            Estado        = req.Estado.ToString(),
            Observacion   = req.Observacion,
            CantidadItems = req.Items.Count,
            CreatedAt     = req.CreatedAt,
            Items         = req.Items.Select(i => new RequisicionCompraItemDto
            {
                Id           = i.Id,
                ItemId       = i.ItemId,
                Descripcion  = i.Descripcion,
                Cantidad     = i.Cantidad,
                UnidadMedida = i.UnidadMedida,
                Observacion  = i.Observacion
            }).ToList()
        };
    }
}
