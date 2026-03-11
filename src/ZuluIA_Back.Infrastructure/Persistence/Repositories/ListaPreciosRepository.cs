using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ListaPreciosRepository : BaseRepository<ListaPrecios>, IListaPreciosRepository
{
    public ListaPreciosRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ListaPrecios>> GetActivasAsync(
        DateOnly? fecha = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.Activa);

        if (fecha.HasValue)
        {
            var f = fecha.Value;
            query = query.Where(x =>
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= f) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= f));
        }

        return await query
            .OrderBy(x => x.Descripcion)
            .ToListAsync(ct);
    }

    public async Task<ListaPrecios?> GetByIdConItemsAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<ListaPreciosItem?> GetPrecioItemAsync(
        long listaId,
        long itemId,
        CancellationToken ct = default) =>
        await Context.ListaPreciosItems
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ListaId == listaId && x.ItemId == itemId,
                ct);

    public async Task<ListaPreciosItem?> ResolverPrecioItemAsync(
        long itemId,
        long monedaId,
        DateOnly fecha,
        CancellationToken ct = default) =>
        await Context.ListaPreciosItems
            .AsNoTracking()
            .Include(x => x.Lista)
            .Where(x =>
                x.ItemId == itemId &&
                x.Lista != null &&
                x.Lista.Activa &&
                x.Lista.MonedaId == monedaId &&
                (!x.Lista.VigenciaDesde.HasValue || x.Lista.VigenciaDesde.Value <= fecha) &&
                (!x.Lista.VigenciaHasta.HasValue || x.Lista.VigenciaHasta.Value >= fecha))
            .OrderByDescending(x => x.Lista!.VigenciaDesde)
            .FirstOrDefaultAsync(ct);

    public override async Task<ListaPrecios?> GetByIdAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}
