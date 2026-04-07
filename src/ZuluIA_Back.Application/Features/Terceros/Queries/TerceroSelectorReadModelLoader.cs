using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

internal static class TerceroSelectorReadModelLoader
{
    public static async Task LoadUbicacionAsync(
        IApplicationDbContext db,
        IReadOnlyList<Tercero> terceros,
        IList<TerceroSelectorDto> dtos,
        CancellationToken ct)
    {
        if (terceros.Count == 0 || dtos.Count == 0)
            return;

        var provinciaIds = terceros
            .Where(x => x.Domicilio.ProvinciaId.HasValue)
            .Select(x => x.Domicilio.ProvinciaId!.Value)
            .Distinct()
            .ToList();

        var localidadIds = terceros
            .Where(x => x.Domicilio.LocalidadId.HasValue)
            .Select(x => x.Domicilio.LocalidadId!.Value)
            .Distinct()
            .ToList();

        var barrioIds = terceros
            .Where(x => x.Domicilio.BarrioId.HasValue)
            .Select(x => x.Domicilio.BarrioId!.Value)
            .Distinct()
            .ToList();

        var provincias = provinciaIds.Count > 0
            ? await db.Provincias
                .AsNoTracking()
                .Where(x => provinciaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var localidades = localidadIds.Count > 0
            ? await db.Localidades
                .AsNoTracking()
                .Where(x => localidadIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Descripcion, x.ProvinciaId }, ct)
            : [];

        var barrios = barrioIds.Count > 0
            ? await db.Barrios
                .AsNoTracking()
                .Where(x => barrioIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        for (var i = 0; i < dtos.Count && i < terceros.Count; i++)
        {
            var dto = dtos[i];
            var tercero = terceros[i];

            var provinciaDescripcion = tercero.Domicilio.ProvinciaId.HasValue && provincias.TryGetValue(tercero.Domicilio.ProvinciaId.Value, out var provincia)
                ? provincia
                : null;

            string? localidadDescripcion = null;
            if (tercero.Domicilio.LocalidadId.HasValue && localidades.TryGetValue(tercero.Domicilio.LocalidadId.Value, out var localidad))
            {
                localidadDescripcion = localidad.Descripcion;
                provinciaDescripcion ??= provincias.TryGetValue(localidad.ProvinciaId, out var provinciaDerivada)
                    ? provinciaDerivada
                    : null;
            }

            var barrioDescripcion = tercero.Domicilio.BarrioId.HasValue && barrios.TryGetValue(tercero.Domicilio.BarrioId.Value, out var barrio)
                ? barrio
                : null;

            dto.GeografiaCompleta = TerceroUbicacionFormatter.BuildGeografiaCompleta(
                provinciaDescripcion,
                localidadDescripcion,
                barrioDescripcion);

            dto.UbicacionCompleta = TerceroUbicacionFormatter.BuildUbicacionCompleta(
                tercero.Domicilio.Completo,
                dto.GeografiaCompleta,
                tercero.Domicilio.CodigoPostal);
        }
    }
}
