using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Application.Features.Comisiones.DTOs;

namespace ZuluIA_Back.Application.Features.Comisiones.Queries;

public record GetComisionesVendedorQuery(
    long SucursalId,
    long? VendedorId,
    int? Periodo,
    string? Estado,
    int Page = 1,
    int PageSize = 20)
    : IRequest<PagedResult<ComisionVendedorListDto>>;

public class GetComisionesVendedorQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetComisionesVendedorQuery, PagedResult<ComisionVendedorListDto>>
{
    public async Task<PagedResult<ComisionVendedorListDto>> Handle(
        GetComisionesVendedorQuery request, CancellationToken ct)
    {
        var query = db.ComisionesVendedor.AsNoTracking()
            .Where(c => c.SucursalId == request.SucursalId && c.DeletedAt == null);

        if (request.VendedorId.HasValue)
            query = query.Where(c => c.VendedorId == request.VendedorId);

        if (request.Periodo.HasValue)
            query = query.Where(c => c.Periodo == request.Periodo);

        if (!string.IsNullOrWhiteSpace(request.Estado))
            query = query.Where(c => c.Estado.ToString() == request.Estado);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.Periodo)
            .ThenBy(c => c.VendedorId)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ComisionVendedorListDto(
                c.Id, c.SucursalId, c.VendedorId, c.Periodo,
                c.MontoBase, c.PorcentajeComision, c.MontoComision,
                c.Estado.ToString()))
            .ToListAsync(ct);

        return new PagedResult<ComisionVendedorListDto>(items, request.Page, request.PageSize, total);
    }
}

public record GetComisionDetalleQuery(long Id) : IRequest<ComisionVendedorDto?>;

public class GetComisionDetalleQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetComisionDetalleQuery, ComisionVendedorDto?>
{
    public async Task<ComisionVendedorDto?> Handle(GetComisionDetalleQuery request, CancellationToken ct)
    {
        return await db.ComisionesVendedor.AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new ComisionVendedorDto(
                c.Id, c.SucursalId, c.VendedorId, c.Periodo,
                c.MontoBase, c.PorcentajeComision, c.MontoComision,
                c.Estado.ToString(), c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }
}
