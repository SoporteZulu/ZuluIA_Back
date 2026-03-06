using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public class GetBalanceSumasYSaldosQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetBalanceSumasYSaldosQuery, BalanceSumasYSaldosDto>
{
    public async Task<BalanceSumasYSaldosDto> Handle(
        GetBalanceSumasYSaldosQuery request,
        CancellationToken ct)
    {
        var ejercicio = await db.Ejercicios.AsNoTracking()
            .Where(x => x.Id == request.EjercicioId)
            .Select(x => new { x.Descripcion })
            .FirstOrDefaultAsync(ct);

        // Obtener movimientos del período filtrado
        var asientosQuery = db.Asientos.AsNoTracking()
            .Where(x =>
                x.EjercicioId == request.EjercicioId &&
                x.Fecha       >= request.Desde        &&
                x.Fecha       <= request.Hasta         &&
                x.Estado      == Domain.Enums.EstadoAsiento.Confirmado);

        if (request.SucursalId.HasValue)
            asientosQuery = asientosQuery
                .Where(x => x.SucursalId == request.SucursalId.Value);

        var asientoIds = await asientosQuery
            .Select(x => x.Id)
            .ToListAsync(ct);

        // Agrupar líneas por cuenta
        var movimientos = await db.AsientosLineas.AsNoTracking()
            .Where(x => asientoIds.Contains(x.AsientoId))
            .GroupBy(x => x.CuentaId)
            .Select(g => new
            {
                CuentaId = g.Key,
                SumasDebe = g.Sum(x => x.Debe),
                SumasHaber = g.Sum(x => x.Haber)
            })
            .ToListAsync(ct);

        var cuentaIds = movimientos.Select(x => x.CuentaId).ToList();

        var cuentas = await db.PlanCuentas.AsNoTracking()
            .Where(x => cuentaIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.CodigoCuenta,
                x.Denominacion,
                x.Nivel,
                x.SaldoNormal
            })
            .ToDictionaryAsync(x => x.Id, ct);

        var lineas = movimientos.Select(m =>
        {
            var cuenta = cuentas.GetValueOrDefault(m.CuentaId);
            var saldoNeto = m.SumasDebe - m.SumasHaber;
            var esDeudora = cuenta?.SaldoNormal is 'D' or 'd';

            return new BalanceLineaDto
            {
                CuentaId       = m.CuentaId,
                CodigoCuenta   = cuenta?.CodigoCuenta ?? "—",
                Denominacion   = cuenta?.Denominacion ?? "—",
                Nivel          = cuenta?.Nivel ?? 0,
                SumasDebe      = m.SumasDebe,
                SumasHaber     = m.SumasHaber,
                SaldoDeudor    = saldoNeto > 0 ? saldoNeto : 0,
                SaldoAcreedor  = saldoNeto < 0 ? Math.Abs(saldoNeto) : 0
            };
        })
        .OrderBy(x => x.CodigoCuenta)
        .ToList();

        return new BalanceSumasYSaldosDto
        {
            EjercicioId          = request.EjercicioId,
            EjercicioDescripcion = ejercicio?.Descripcion ?? "—",
            Desde                = request.Desde,
            Hasta                = request.Hasta,
            Lineas               = lineas.AsReadOnly(),
            TotalDebe            = lineas.Sum(x => x.SumasDebe),
            TotalHaber           = lineas.Sum(x => x.SumasHaber),
            TotalSaldoDeudor     = lineas.Sum(x => x.SaldoDeudor),
            TotalSaldoAcreedor   = lineas.Sum(x => x.SaldoAcreedor)
        };
    }
}