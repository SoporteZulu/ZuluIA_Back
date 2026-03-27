using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.TasasInteres.DTOs;

namespace ZuluIA_Back.Application.Features.TasasInteres.Queries;

public record GetTasasInteresQuery(bool? SoloActivas = null) : IRequest<List<TasaInteresDto>>;

public class GetTasasInteresQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTasasInteresQuery, List<TasaInteresDto>>
{
    public async Task<List<TasaInteresDto>> Handle(GetTasasInteresQuery request, CancellationToken ct)
    {
        var query = db.TasasInteres.AsNoTracking().Where(t => t.DeletedAt == null);
        if (request.SoloActivas.HasValue)
            query = query.Where(t => t.Activo == request.SoloActivas);

        return await query
            .OrderByDescending(t => t.FechaDesde)
            .Select(t => new TasaInteresDto(
                t.Id, t.Descripcion, t.TasaMensual,
                t.FechaDesde, t.FechaHasta, t.Activo))
            .ToListAsync(ct);
    }
}

// ── Cta. cte. con intereses moratorios ────────────────────────────────────

public record SaldoConInteresDto(
    long TerceroId,
    long MonedaId,
    long? SucursalId,
    decimal SaldoOriginal,
    decimal InteresAcumulado,
    decimal SaldoTotal,
    DateOnly FechaCalculo);

public record GetCuentaCorrienteConInteresQuery(
    long TerceroId,
    long? SucursalId,
    DateOnly? FechaCalculo = null)
    : IRequest<List<SaldoConInteresDto>>;

public class GetCuentaCorrienteConInteresQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCuentaCorrienteConInteresQuery, List<SaldoConInteresDto>>
{
    public async Task<List<SaldoConInteresDto>> Handle(
        GetCuentaCorrienteConInteresQuery request, CancellationToken ct)
    {
        var fechaCorte = request.FechaCalculo ?? DateOnly.FromDateTime(DateTime.Today);

        // Tasa activa vigente al fechaCorte
        var tasa = await db.TasasInteres.AsNoTracking()
            .Where(t => t.Activo && t.FechaDesde <= fechaCorte
                        && (t.FechaHasta == null || t.FechaHasta >= fechaCorte)
                        && t.DeletedAt == null)
            .OrderByDescending(t => t.FechaDesde)
            .FirstOrDefaultAsync(ct);

        var tasaDiaria = tasa is not null ? tasa.TasaMensual / 100m / 30m : 0m;

        var query = db.CuentaCorriente.AsNoTracking()
            .Where(cc => cc.TerceroId == request.TerceroId && cc.Saldo != 0);
        if (request.SucursalId.HasValue)
            query = query.Where(cc => cc.SucursalId == request.SucursalId);

        var saldos = await query.ToListAsync(ct);

        var result = new List<SaldoConInteresDto>();
        foreach (var cc in saldos)
        {
            // Días en mora: calculado desde la fecha del último movimiento (UpdatedAt)
            var fechaUltMov = DateOnly.FromDateTime(cc.UpdatedAt.UtcDateTime);
            var diasMora = Math.Max(0, fechaCorte.DayNumber - fechaUltMov.DayNumber);
            var interes = cc.Saldo > 0 ? cc.Saldo * tasaDiaria * diasMora : 0m;

            result.Add(new SaldoConInteresDto(
                cc.TerceroId, cc.MonedaId, cc.SucursalId,
                cc.Saldo, interes, cc.Saldo + interes, fechaCorte));
        }

        return result;
    }
}
