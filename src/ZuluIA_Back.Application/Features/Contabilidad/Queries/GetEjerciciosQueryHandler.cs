using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public class GetEjerciciosQueryHandler(
    IEjercicioRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetEjerciciosQuery, IReadOnlyList<EjercicioDto>>
{
    public async Task<IReadOnlyList<EjercicioDto>> Handle(
        GetEjerciciosQuery request,
        CancellationToken ct)
    {
        var ejercicios = await repo.GetAllAsync(ct);

        var sucursalIds = ejercicios
            .SelectMany(e => e.Sucursales.Select(s => s.SucursalId))
            .Distinct().ToList();

        var sucursales = await db.Sucursales.AsNoTracking()
            .Where(x => sucursalIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        return ejercicios.Select(e => new EjercicioDto
        {
            Id                    = e.Id,
            Descripcion           = e.Descripcion,
            FechaInicio           = e.FechaInicio,
            FechaFin              = e.FechaFin,
            MascaraCuentaContable = e.MascaraCuentaContable,
            Cerrado               = e.Cerrado,
            CreatedAt             = e.CreatedAt,
            Sucursales            = e.Sucursales.Select(s => new EjercicioSucursalDto
            {
                Id                   = s.Id,
                SucursalId           = s.SucursalId,
                SucursalDescripcion  = sucursales.GetValueOrDefault(s.SucursalId)?.RazonSocial ?? "—",
                UsaContabilidad      = s.UsaContabilidad
            }).ToList().AsReadOnly()
        }).ToList().AsReadOnly();
    }
}