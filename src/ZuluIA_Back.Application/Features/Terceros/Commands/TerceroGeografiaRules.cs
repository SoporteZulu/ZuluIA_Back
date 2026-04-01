using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

internal static class TerceroGeografiaRules
{
    public static async Task<Result<long?>> ResolveProvinciaIdAsync(
        IApplicationDbContext db,
        long? provinciaId,
        long? localidadId,
        long? barrioId,
        CancellationToken ct)
    {
        if (barrioId.HasValue && !localidadId.HasValue)
            return Result.Failure<long?>("Debe informar la localidad cuando se indica un barrio.");

        if (provinciaId.HasValue)
        {
            var provinciaExiste = await db.Provincias
                .AsNoTracking()
                .AnyAsync(x => x.Id == provinciaId.Value && x.DeletedAt == null, ct);

            if (!provinciaExiste)
                return Result.Failure<long?>("La provincia indicada no existe o fue eliminada.");
        }

        if (localidadId.HasValue)
        {
            var localidad = await db.Localidades
                .AsNoTracking()
                .Where(x => x.Id == localidadId.Value && x.DeletedAt == null)
                .Select(x => new { x.Id, x.ProvinciaId })
                .FirstOrDefaultAsync(ct);

            if (localidad is null)
                return Result.Failure<long?>("La localidad indicada no existe o fue eliminada.");

            if (provinciaId.HasValue && localidad.ProvinciaId != provinciaId.Value)
                return Result.Failure<long?>("La localidad indicada no pertenece a la provincia seleccionada.");

            provinciaId = localidad.ProvinciaId;
        }

        if (barrioId.HasValue)
        {
            var barrio = await db.Barrios
                .AsNoTracking()
                .Where(x => x.Id == barrioId.Value && x.DeletedAt == null)
                .Select(x => new { x.Id, x.LocalidadId })
                .FirstOrDefaultAsync(ct);

            if (barrio is null)
                return Result.Failure<long?>("El barrio indicado no existe o fue eliminado.");

            if (barrio.LocalidadId != localidadId)
                return Result.Failure<long?>("El barrio indicado no pertenece a la localidad seleccionada.");
        }

        return Result.Success(provinciaId);
    }
}
