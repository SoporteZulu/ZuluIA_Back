using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

internal static class TerceroDomicilioPrincipalSync
{
    public static async Task<Domicilio> ResolvePrincipalAsync(
        IApplicationDbContext db,
        Domicilio fallback,
        IReadOnlyList<ReplaceTerceroDomicilioItem>? domicilios,
        CancellationToken ct)
    {
        var domicilioDefault = GetDefault(domicilios);
        if (domicilioDefault is null)
            return fallback;

        var provinciaId = domicilioDefault.ProvinciaId;

        if (!provinciaId.HasValue && domicilioDefault.LocalidadId.HasValue)
        {
            provinciaId = await db.Localidades
                .AsNoTracking()
                .Where(x => x.Id == domicilioDefault.LocalidadId.Value)
                .Select(x => (long?)x.ProvinciaId)
                .FirstOrDefaultAsync(ct);
        }

        var barrioId = await ResolveBarrioIdAsync(
            db,
            domicilioDefault.LocalidadId,
            domicilioDefault.Barrio,
            ct);

        return Domicilio.Crear(
            domicilioDefault.Calle,
            null,
            null,
            null,
            domicilioDefault.CodigoPostal,
            domicilioDefault.LocalidadId,
            barrioId,
            provinciaId);
    }

    public static async Task<ReplaceTerceroDomicilioItem?> BuildDefaultFromPrincipalAsync(
        IApplicationDbContext db,
        Domicilio domicilio,
        CancellationToken ct)
    {
        if (!HasData(domicilio))
            return null;

        var barrio = await ResolveBarrioDescripcionAsync(db, domicilio.BarrioId, ct);

        return new ReplaceTerceroDomicilioItem(
            Id: null,
            TipoDomicilioId: null,
            ProvinciaId: domicilio.ProvinciaId,
            LocalidadId: domicilio.LocalidadId,
            Calle: BuildLegacyStreet(domicilio),
            Barrio: barrio,
            CodigoPostal: domicilio.CodigoPostal,
            Observacion: null,
            EsDefecto: true,
            Orden: 0);
    }

    private static ReplaceTerceroDomicilioItem? GetDefault(IReadOnlyList<ReplaceTerceroDomicilioItem>? domicilios)
    {
        if (domicilios is null || domicilios.Count == 0)
            return null;

        var explicitDefault = domicilios.FirstOrDefault(x => x.EsDefecto);
        var candidate = explicitDefault ?? domicilios[0];
        return HasData(candidate) ? candidate : null;
    }

    private static bool HasData(Domicilio domicilio)
        => !string.IsNullOrWhiteSpace(domicilio.Calle)
           || !string.IsNullOrWhiteSpace(domicilio.Nro)
           || !string.IsNullOrWhiteSpace(domicilio.Piso)
           || !string.IsNullOrWhiteSpace(domicilio.Dpto)
           || !string.IsNullOrWhiteSpace(domicilio.CodigoPostal)
           || domicilio.ProvinciaId.HasValue
           || domicilio.LocalidadId.HasValue
           || domicilio.BarrioId.HasValue;

    private static bool HasData(ReplaceTerceroDomicilioItem domicilio)
        => !string.IsNullOrWhiteSpace(domicilio.Calle)
           || HasBarrioData(domicilio.Barrio)
           || !string.IsNullOrWhiteSpace(domicilio.CodigoPostal)
           || !string.IsNullOrWhiteSpace(domicilio.Observacion)
           || domicilio.ProvinciaId.HasValue
           || domicilio.LocalidadId.HasValue;

    private static bool HasBarrioData(string? barrio)
        => !string.IsNullOrWhiteSpace(barrio)
           && !string.Equals(barrio.Trim(), "sin especificar", StringComparison.OrdinalIgnoreCase);

    private static async Task<long?> ResolveBarrioIdAsync(
        IApplicationDbContext db,
        long? localidadId,
        string? barrio,
        CancellationToken ct)
    {
        if (!localidadId.HasValue || !HasBarrioData(barrio))
            return null;

        var normalized = barrio!.Trim();

        return await db.Barrios
            .AsNoTracking()
            .Where(x => x.LocalidadId == localidadId.Value && x.Descripcion == normalized)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    private static async Task<string?> ResolveBarrioDescripcionAsync(
        IApplicationDbContext db,
        long? barrioId,
        CancellationToken ct)
    {
        if (!barrioId.HasValue)
            return null;

        return await db.Barrios
            .AsNoTracking()
            .Where(x => x.Id == barrioId.Value)
            .Select(x => x.Descripcion)
            .FirstOrDefaultAsync(ct);
    }

    private static string? BuildLegacyStreet(Domicilio domicilio)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(domicilio.Calle))
            parts.Add(domicilio.Calle.Trim());

        if (!string.IsNullOrWhiteSpace(domicilio.Nro))
            parts.Add(domicilio.Nro.Trim());

        if (!string.IsNullOrWhiteSpace(domicilio.Piso))
            parts.Add($"P{domicilio.Piso.Trim()}");

        if (!string.IsNullOrWhiteSpace(domicilio.Dpto))
            parts.Add($"D{domicilio.Dpto.Trim()}");

        return parts.Count == 0 ? null : string.Join(" ", parts);
    }
}
