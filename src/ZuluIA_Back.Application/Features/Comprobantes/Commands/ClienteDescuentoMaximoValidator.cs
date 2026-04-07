using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

internal static class ClienteDescuentoMaximoValidator
{
    public static async Task<string?> ValidateAsync(
        IApplicationDbContext db,
        long terceroId,
        IEnumerable<decimal> descuentosPct,
        CancellationToken ct)
    {
        var porcentajeMaximo = await db.Terceros
            .AsNoTracking()
            .Where(x => x.Id == terceroId && x.DeletedAt == null)
            .Select(x => x.PorcentajeMaximoDescuento)
            .FirstOrDefaultAsync(ct);

        if (!porcentajeMaximo.HasValue)
            return null;

        var descuentoSolicitado = descuentosPct.DefaultIfEmpty(0m).Max();
        if (descuentoSolicitado <= porcentajeMaximo.Value)
            return null;

        return $"El descuento solicitado ({descuentoSolicitado:0.##}%) supera el porcentaje máximo permitido para el cliente ({porcentajeMaximo.Value:0.##}%).";
    }
}
