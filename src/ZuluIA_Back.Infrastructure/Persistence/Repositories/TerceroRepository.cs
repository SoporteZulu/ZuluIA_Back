using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class TerceroRepository(AppDbContext context)
    : BaseRepository<Tercero>(context), ITerceroRepository
{
    public async Task<Tercero?> GetByLegajoAsync(string legajo, CancellationToken ct = default) =>
        await DbSet
            .FirstOrDefaultAsync(x => x.Legajo == legajo.ToUpperInvariant(), ct);

    public async Task<Tercero?> GetByNroDocumentoAsync(string nroDocumento, CancellationToken ct = default) =>
        await DbSet
            .FirstOrDefaultAsync(x => x.NroDocumento == nroDocumento, ct);

    public async Task<PagedResult<Tercero>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? soloClientes,
        bool? soloProveedores,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(x =>
                x.RazonSocial.ToLower().Contains(term)  ||
                x.Legajo.ToLower().Contains(term)       ||
                x.NroDocumento.ToLower().Contains(term) ||
                (x.NombreFantasia != null && x.NombreFantasia.ToLower().Contains(term)));
        }

        if (soloClientes == true)
            query = query.Where(x => x.EsCliente);

        if (soloProveedores == true)
            query = query.Where(x => x.EsProveedor);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.RazonSocial)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Tercero>(items, page, pageSize, total);
    }

    public async Task<bool> ExisteLegajoAsync(
        string legajo,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x => x.Legajo == legajo.ToUpperInvariant());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<bool> ExisteNroDocumentoAsync(
        string nroDocumento,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x => x.NroDocumento == nroDocumento);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}