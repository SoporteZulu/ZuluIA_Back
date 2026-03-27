using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Application.Features.Granos.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Granos.Queries;

public record GetLiquidacionesGranoPagedQuery(
    long? SucursalId,
    long? TerceroId,
    string? Estado,
    int Page = 1,
    int PageSize = 20)
    : IRequest<PagedResult<LiquidacionGranosListDto>>;

public class GetLiquidacionesGranoPagedQueryHandler(ILiquidacionGranosRepository repo)
    : IRequestHandler<GetLiquidacionesGranoPagedQuery, PagedResult<LiquidacionGranosListDto>>
{
    public async Task<PagedResult<LiquidacionGranosListDto>> Handle(
        GetLiquidacionesGranoPagedQuery request, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.TerceroId, request.Estado, ct);

        return new PagedResult<LiquidacionGranosListDto>(
            paged.Items.Select(l => new LiquidacionGranosListDto(
                l.Id, l.SucursalId, l.TerceroId, l.Producto, l.Fecha,
                l.Cantidad, l.PrecioBase, l.ValorNeto, l.Estado.ToString())).ToList(),
            paged.Page, paged.PageSize, paged.TotalCount);
    }
}

public record GetLiquidacionGranosDetalleQuery(long Id) : IRequest<LiquidacionGranosDto?>;

public class GetLiquidacionGranosDetalleQueryHandler(ILiquidacionGranosRepository repo)
    : IRequestHandler<GetLiquidacionGranosDetalleQuery, LiquidacionGranosDto?>
{
    public async Task<LiquidacionGranosDto?> Handle(GetLiquidacionGranosDetalleQuery request, CancellationToken ct)
    {
        var liq = await repo.GetByIdConConceptosAsync(request.Id, ct);
        if (liq is null) return null;

        return new LiquidacionGranosDto(
            liq.Id, liq.SucursalId, liq.TerceroId, liq.Producto, liq.Fecha,
            liq.Cantidad, liq.PrecioBase, liq.Deducciones, liq.ValorNeto,
            liq.Estado.ToString(), liq.ComprobanteId,
            liq.Conceptos.Select(c => new ConceptoDto(c.Id, c.Concepto, c.Importe, c.EsDeduccion)).ToList());
    }
}
