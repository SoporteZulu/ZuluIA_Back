using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public class GetAsientoDetalleQueryHandler(
    IAsientoRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetAsientoDetalleQuery, AsientoDto?>
{
    public async Task<AsientoDto?> Handle(
        GetAsientoDetalleQuery request,
        CancellationToken ct)
    {
        var asiento = await repo.GetByIdConLineasAsync(request.Id, ct);
        if (asiento is null) return null;

        var ejercicio = await db.Ejercicios.AsNoTracking()
            .Where(x => x.Id == asiento.EjercicioId)
            .Select(x => new { x.Descripcion })
            .FirstOrDefaultAsync(ct);

        var cuentaIds = asiento.Lineas.Select(x => x.CuentaId).Distinct().ToList();
        var ccIds = asiento.Lineas
            .Where(x => x.CentroCostoId.HasValue)
            .Select(x => x.CentroCostoId!.Value).Distinct().ToList();

        var cuentas = await db.PlanCuentas.AsNoTracking()
            .Where(x => cuentaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CodigoCuenta, x.Denominacion })
            .ToDictionaryAsync(x => x.Id, ct);

        var centros = await db.CentrosCosto.AsNoTracking()
            .Where(x => ccIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        return new AsientoDto
        {
            Id                  = asiento.Id,
            EjercicioId         = asiento.EjercicioId,
            EjercicioDescripcion = ejercicio?.Descripcion ?? "—",
            SucursalId          = asiento.SucursalId,
            Fecha               = asiento.Fecha,
            Numero              = asiento.Numero,
            Descripcion         = asiento.Descripcion,
            OrigenTabla         = asiento.OrigenTabla,
            OrigenId            = asiento.OrigenId,
            Estado              = asiento.Estado.ToString().ToUpperInvariant(),
            TotalDebe           = asiento.TotalDebe,
            TotalHaber          = asiento.TotalHaber,
            Cuadra              = asiento.Cuadra,
            CreatedAt           = asiento.CreatedAt,
            Lineas = asiento.Lineas
                .OrderBy(x => x.Orden)
                .Select(l => new AsientoLineaDto
                {
                    Id                      = l.Id,
                    CuentaId                = l.CuentaId,
                    CuentaCodigo            = cuentas.GetValueOrDefault(l.CuentaId)?.CodigoCuenta  ?? "—",
                    CuentaDenominacion      = cuentas.GetValueOrDefault(l.CuentaId)?.Denominacion  ?? "—",
                    Debe                    = l.Debe,
                    Haber                   = l.Haber,
                    Descripcion             = l.Descripcion,
                    Orden                   = l.Orden,
                    CentroCostoId           = l.CentroCostoId,
                    CentroCostoDescripcion  = l.CentroCostoId.HasValue
                        ? centros.GetValueOrDefault(l.CentroCostoId.Value)?.Descripcion
                        : null
                }).ToList().AsReadOnly()
        };
    }
}