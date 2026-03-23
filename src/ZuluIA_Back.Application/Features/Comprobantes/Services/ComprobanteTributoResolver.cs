using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Application.Features.Comprobantes.Services;

internal static class ComprobanteTributoResolver
{
    public static async Task<IReadOnlyCollection<ComprobanteTributo>> ResolveAsync(
        IApplicationDbContext db,
        Comprobante comprobante,
        CancellationToken ct)
    {
        if (comprobante.Percepciones <= 0)
            return [];

        var baseImponible = Math.Round(comprobante.NetoGravado + comprobante.NetoNoGravado, 2);

        var tributoConfig = await db.ImpuestosPorTipoComprobante
            .AsNoTracking()
            .Where(x => x.TipoComprobanteId == comprobante.TipoComprobanteId)
            .Join(
                db.Impuestos.AsNoTracking().Where(x => x.Activo && x.Tipo == "percepcion"),
                assignment => assignment.ImpuestoId,
                tax => tax.Id,
                (assignment, tax) => new { assignment.Orden, Tax = tax })
            .OrderBy(x => x.Orden)
            .Select(x => new
            {
                x.Orden,
                x.Tax.Id,
                x.Tax.Codigo,
                x.Tax.Descripcion,
                x.Tax.Alicuota
            })
            .FirstOrDefaultAsync(ct);

        return
        [
            ComprobanteTributo.Crear(
                comprobante.Id,
                tributoConfig?.Id,
                tributoConfig?.Codigo ?? "99",
                string.IsNullOrWhiteSpace(tributoConfig?.Descripcion) ? "Percepciones" : tributoConfig.Descripcion,
                baseImponible,
                Math.Round(tributoConfig?.Alicuota ?? 0m, 2),
                Math.Round(comprobante.Percepciones, 2),
                tributoConfig?.Orden ?? 0)
        ];
    }
}