using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Services;

public sealed record PrecioListaResolutionResult(
    decimal Precio,
    decimal DescuentoPct,
    decimal PrecioFinal,
    long? ListaPreciosId,
    string Origen,
    long? PromocionId = null);

public class PrecioListaResolutionService(IApplicationDbContext db)
{
    /// <summary>
    /// Resuelve el precio final aplicable para un ítem considerando precios especiales,
    /// lista explícita, lista asignada al cliente, lista por defecto, herencia y promociones.
    /// </summary>
    public async Task<PrecioListaResolutionResult?> ResolveAsync(
        long itemId,
        long monedaId,
        DateOnly fecha,
        long? terceroId = null,
        long? listaPreciosId = null,
        long? canalVentaId = null,
        long? vendedorId = null,
        long? categoriaItemId = null,
        decimal? montoCompra = null,
        int? cantidad = null,
        CancellationToken ct = default)
    {
        var precioEspecialCliente = await ResolvePrecioEspecialClienteAsync(
            itemId,
            terceroId,
            monedaId,
            fecha,
            ct);
        if (precioEspecialCliente is not null)
        {
            return precioEspecialCliente;
        }

        var precioEspecialCanal = await ResolvePrecioEspecialCanalAsync(
            itemId,
            canalVentaId,
            monedaId,
            fecha,
            ct);
        if (precioEspecialCanal is not null)
        {
            return precioEspecialCanal;
        }

        var precioEspecialVendedor = await ResolvePrecioEspecialVendedorAsync(
            itemId,
            vendedorId,
            monedaId,
            fecha,
            ct);
        if (precioEspecialVendedor is not null)
        {
            return precioEspecialVendedor;
        }

        var listaIdResuelta = listaPreciosId
            ?? await ResolveListaAsignadaAsync(terceroId, monedaId, fecha, ct)
            ?? await ResolveListaPorDefectoAsync(monedaId, fecha, ct);

        if (!listaIdResuelta.HasValue)
        {
            return null;
        }

        var precioLista = await ResolvePrecioEnListaConHerenciaAsync(
            listaIdResuelta.Value,
            itemId,
            fecha,
            categoriaItemId,
            montoCompra,
            cantidad,
            ct);

        return precioLista;
    }

    private async Task<PrecioListaResolutionResult?> ResolvePrecioEspecialClienteAsync(
        long itemId,
        long? terceroId,
        long monedaId,
        DateOnly fecha,
        CancellationToken ct)
    {
        if (!terceroId.HasValue)
        {
            return null;
        }

        var precio = await db.PreciosEspecialesClientes
            .AsNoTracking()
            .Where(x =>
                x.ItemId == itemId &&
                x.ClienteId == terceroId.Value &&
                x.MonedaId == monedaId &&
                x.Activo &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= fecha) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= fecha))
            .OrderByDescending(x => x.VigenciaDesde)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return precio is null
            ? null
            : new PrecioListaResolutionResult(
                precio.Precio,
                precio.DescuentoPct,
                precio.PrecioFinal,
                null,
                "PRECIO_ESPECIAL_CLIENTE");
    }

    private async Task<PrecioListaResolutionResult?> ResolvePrecioEspecialCanalAsync(
        long itemId,
        long? canalVentaId,
        long monedaId,
        DateOnly fecha,
        CancellationToken ct)
    {
        if (!canalVentaId.HasValue)
        {
            return null;
        }

        var precio = await db.PreciosEspecialesCanales
            .AsNoTracking()
            .Where(x =>
                x.ItemId == itemId &&
                x.CanalId == canalVentaId.Value &&
                x.MonedaId == monedaId &&
                x.Activo &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= fecha) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= fecha))
            .OrderByDescending(x => x.VigenciaDesde)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return precio is null
            ? null
            : new PrecioListaResolutionResult(
                precio.Precio,
                precio.DescuentoPct,
                precio.PrecioFinal,
                null,
                "PRECIO_ESPECIAL_CANAL");
    }

    private async Task<PrecioListaResolutionResult?> ResolvePrecioEspecialVendedorAsync(
        long itemId,
        long? vendedorId,
        long monedaId,
        DateOnly fecha,
        CancellationToken ct)
    {
        if (!vendedorId.HasValue)
        {
            return null;
        }

        var precio = await db.PreciosEspecialesVendedores
            .AsNoTracking()
            .Where(x =>
                x.ItemId == itemId &&
                x.VendedorId == vendedorId.Value &&
                x.MonedaId == monedaId &&
                x.Activo &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= fecha) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= fecha))
            .OrderByDescending(x => x.VigenciaDesde)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return precio is null
            ? null
            : new PrecioListaResolutionResult(
                precio.Precio,
                precio.DescuentoPct,
                precio.PrecioFinal,
                null,
                "PRECIO_ESPECIAL_VENDEDOR");
    }

    private async Task<long?> ResolveListaAsignadaAsync(
        long? terceroId,
        long monedaId,
        DateOnly fecha,
        CancellationToken ct)
    {
        if (!terceroId.HasValue)
        {
            return null;
        }

        return await db.ListasPreciosPersonas
            .AsNoTracking()
            .Where(x => x.PersonaId == terceroId.Value)
            .Join(
                db.ListasPrecios.AsNoTracking(),
                asignacion => asignacion.ListaPreciosId,
                lista => lista.Id,
                (asignacion, lista) => lista)
            .Where(x =>
                x.Activa &&
                x.MonedaId == monedaId &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= fecha) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= fecha))
            .OrderByDescending(x => x.Prioridad)
            .ThenByDescending(x => x.EsPorDefecto)
            .ThenBy(x => x.Descripcion)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<long?> ResolveListaPorDefectoAsync(
        long monedaId,
        DateOnly fecha,
        CancellationToken ct)
    {
        return await db.ListasPrecios
            .AsNoTracking()
            .Where(x =>
                x.Activa &&
                x.EsPorDefecto &&
                x.MonedaId == monedaId &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= fecha) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= fecha))
            .OrderByDescending(x => x.Prioridad)
            .ThenBy(x => x.Descripcion)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<PrecioListaResolutionResult?> ResolvePrecioEnListaConHerenciaAsync(
        long listaId,
        long itemId,
        DateOnly fecha,
        long? categoriaItemId,
        decimal? montoCompra,
        int? cantidad,
        CancellationToken ct)
    {
        var visitadas = new HashSet<long>();
        long? listaActualId = listaId;

        while (listaActualId.HasValue && visitadas.Add(listaActualId.Value))
        {
            var lista = await db.ListasPrecios
                .AsNoTracking()
                .Where(x => x.Id == listaActualId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.ListaPadreId,
                    x.Activa,
                    x.VigenciaDesde,
                    x.VigenciaHasta
                })
                .FirstOrDefaultAsync(ct);

            if (lista is null)
            {
                return null;
            }

            if (lista.Activa &&
                (!lista.VigenciaDesde.HasValue || lista.VigenciaDesde.Value <= fecha) &&
                (!lista.VigenciaHasta.HasValue || lista.VigenciaHasta.Value >= fecha))
            {
                var item = await db.ListaPreciosItems
                    .AsNoTracking()
                    .Where(x => x.ListaId == lista.Id && x.ItemId == itemId)
                    .FirstOrDefaultAsync(ct);

                if (item is not null)
                {
                    var promocion = await db.ListasPreciosPromociones
                        .AsNoTracking()
                        .Where(x =>
                            x.ListaId == lista.Id &&
                            x.Activa &&
                            x.VigenciaDesde <= fecha &&
                            x.VigenciaHasta >= fecha)
                        .OrderByDescending(x => x.DescuentoPct)
                        .ThenByDescending(x => x.UpdatedAt)
                        .FirstOrDefaultAsync(ct);

                    if (promocion is not null &&
                        promocion.AplicaA(itemId, categoriaItemId, montoCompra, cantidad))
                    {
                        var descuentoTotal = Math.Min(100m, item.DescuentoPct + promocion.DescuentoPct);
                        var precioFinalPromocion = Math.Round(item.Precio * (1 - descuentoTotal / 100m), 4);

                        return new PrecioListaResolutionResult(
                            item.Precio,
                            descuentoTotal,
                            precioFinalPromocion,
                            lista.Id,
                            "LISTA_PROMOCION",
                            promocion.Id);
                    }

                    return new PrecioListaResolutionResult(
                        item.Precio,
                        item.DescuentoPct,
                        item.PrecioFinal,
                        lista.Id,
                        "LISTA_PRECIO");
                }
            }

            listaActualId = lista.ListaPadreId;
        }

        return null;
    }
}
