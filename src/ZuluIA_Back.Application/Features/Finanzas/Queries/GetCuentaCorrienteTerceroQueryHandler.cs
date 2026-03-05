using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Queries;

public class GetCuentaCorrienteTerceroQueryHandler(
    ICuentaCorrienteRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetCuentaCorrienteTerceroQuery, IReadOnlyList<CuentaCorrienteDto>>
{
    public async Task<IReadOnlyList<CuentaCorrienteDto>> Handle(
        GetCuentaCorrienteTerceroQuery request,
        CancellationToken ct)
    {
        var cuentas = await repo.GetByTerceroAsync(request.TerceroId, ct);

        if (request.SucursalId.HasValue)
            cuentas = cuentas
                .Where(x => x.SucursalId == request.SucursalId.Value
                         || x.SucursalId == null)
                .ToList();

        var monedaIds = cuentas.Select(x => x.MonedaId).Distinct().ToList();
        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var tercero = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == request.TerceroId)
            .Select(x => new { x.RazonSocial })
            .FirstOrDefaultAsync(ct);

        return cuentas.Select(c => new CuentaCorrienteDto
        {
            TerceroId          = c.TerceroId,
            TerceroRazonSocial = tercero?.RazonSocial ?? "—",
            SucursalId         = c.SucursalId,
            MonedaId           = c.MonedaId,
            MonedaSimbolo      = monedas.GetValueOrDefault(c.MonedaId)?.Simbolo ?? "$",
            Saldo              = c.Saldo,
            UpdatedAt          = c.UpdatedAt
        }).ToList().AsReadOnly();
    }
}