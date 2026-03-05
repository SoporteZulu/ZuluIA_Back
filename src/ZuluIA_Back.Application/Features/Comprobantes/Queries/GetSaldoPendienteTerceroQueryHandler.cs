using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetSaldoPendienteTerceroQueryHandler(
    IComprobanteRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetSaldoPendienteTerceroQuery, IReadOnlyList<SaldoPendienteDto>>
{
    public async Task<IReadOnlyList<SaldoPendienteDto>> Handle(
        GetSaldoPendienteTerceroQuery request,
        CancellationToken ct)
    {
        var comprobantes = await repo.GetSaldoPendienteByTerceroAsync(
            request.TerceroId, request.SucursalId, ct);

        var tipoIds = comprobantes.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        return comprobantes.Select(c => new SaldoPendienteDto
        {
            ComprobanteId    = c.Id,
            NumeroFormateado = c.Numero.Formateado,
            TipoComprobante  = tipos.GetValueOrDefault(c.TipoComprobanteId)?.Descripcion ?? "—",
            Fecha            = c.Fecha,
            FechaVencimiento = c.FechaVencimiento,
            Total            = c.Total,
            Saldo            = c.Saldo,
            Vencido          = c.FechaVencimiento.HasValue && c.FechaVencimiento.Value < hoy
        }).ToList().AsReadOnly();
    }
}