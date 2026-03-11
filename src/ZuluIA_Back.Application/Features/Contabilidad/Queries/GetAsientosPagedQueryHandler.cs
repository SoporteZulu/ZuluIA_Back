using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public class GetAsientosPagedQueryHandler(IAsientoRepository repo)
    : IRequestHandler<GetAsientosPagedQuery, PagedResult<AsientoListDto>>
{
    public async Task<PagedResult<AsientoListDto>> Handle(
        GetAsientosPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.EjercicioId, request.SucursalId,
            request.Estado, request.Desde, request.Hasta, ct);

        var dtos = result.Items.Select(a => new AsientoListDto
        {
            Id          = a.Id,
            EjercicioId = a.EjercicioId,
            SucursalId  = a.SucursalId,
            Fecha       = a.Fecha,
            Numero      = a.Numero,
            Descripcion = a.Descripcion,
            Estado      = a.Estado.ToString().ToUpperInvariant(),
            TotalDebe   = a.TotalDebe,
            TotalHaber  = a.TotalHaber,
            OrigenTabla = a.OrigenTabla,
            OrigenId    = a.OrigenId
        }).ToList();

        return new PagedResult<AsientoListDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}