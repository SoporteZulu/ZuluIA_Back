using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

internal static class TerceroDomicilioReadModelLoader
{
    public static async Task LoadDescripcionesAsync(
        IApplicationDbContext db,
        IList<TerceroDomicilioDto> domicilios,
        CancellationToken ct)
    {
        if (domicilios.Count == 0)
            return;

        var provinciaIds = domicilios
            .Where(x => x.ProvinciaId.HasValue)
            .Select(x => x.ProvinciaId!.Value)
            .Distinct()
            .ToList();

        var tiposDomicilioIds = domicilios
            .Where(x => x.TipoDomicilioId.HasValue)
            .Select(x => x.TipoDomicilioId!.Value)
            .Distinct()
            .ToList();

        var localidadIds = domicilios
            .Where(x => x.LocalidadId.HasValue)
            .Select(x => x.LocalidadId!.Value)
            .Distinct()
            .ToList();

        var provincias = provinciaIds.Count > 0
            ? await db.Provincias
                .AsNoTrackingSafe()
                .Where(x => provinciaIds.Contains(x.Id))
                .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var localidades = localidadIds.Count > 0
            ? await db.Localidades
                .AsNoTrackingSafe()
                .Where(x => localidadIds.Contains(x.Id))
                .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var tiposDomicilio = tiposDomicilioIds.Count > 0
            ? await db.TiposDomicilio
                .AsNoTrackingSafe()
                .Where(x => tiposDomicilioIds.Contains(x.Id))
                .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        foreach (var domicilio in domicilios)
        {
            if (domicilio.TipoDomicilioId.HasValue && tiposDomicilio.TryGetValue(domicilio.TipoDomicilioId.Value, out var tipoDomicilioDescripcion))
                domicilio.TipoDomicilioDescripcion = tipoDomicilioDescripcion;

            if (domicilio.ProvinciaId.HasValue && provincias.TryGetValue(domicilio.ProvinciaId.Value, out var provinciaDescripcion))
                domicilio.ProvinciaDescripcion = provinciaDescripcion;

            if (domicilio.LocalidadId.HasValue && localidades.TryGetValue(domicilio.LocalidadId.Value, out var localidadDescripcion))
                domicilio.LocalidadDescripcion = localidadDescripcion;

            domicilio.GeografiaCompleta = TerceroUbicacionFormatter.BuildGeografiaCompleta(
                domicilio.ProvinciaDescripcion,
                domicilio.LocalidadDescripcion,
                domicilio.Barrio);

            domicilio.UbicacionCompleta = TerceroUbicacionFormatter.BuildUbicacionCompleta(
                domicilio.Calle,
                domicilio.GeografiaCompleta,
                domicilio.CodigoPostal);
        }
    }
}
