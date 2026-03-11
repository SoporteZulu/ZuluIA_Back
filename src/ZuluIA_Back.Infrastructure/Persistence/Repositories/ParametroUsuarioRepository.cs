using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ParametroUsuarioRepository : BaseRepository<ParametroUsuario>, IParametroUsuarioRepository
{
    private readonly AppDbContext _context;

    public ParametroUsuarioRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ParametroUsuario?> GetByClaveAsync(
        long usuarioId,
        string clave,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UsuarioId == usuarioId &&
                x.Clave     == clave.ToUpperInvariant(),
                ct);

    public async Task<IReadOnlyList<ParametroUsuario>> GetByUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId)
            .OrderBy(x => x.Clave)
            .ToListAsync(ct);

    public async Task UpsertAsync(
        long usuarioId,
        string clave,
        string? valor,
        CancellationToken ct = default)
    {
        var claveNorm = clave.Trim().ToUpperInvariant();
        var existing = await DbSet
            .FirstOrDefaultAsync(x =>
                x.UsuarioId == usuarioId && x.Clave == claveNorm, ct);

        if (existing is null)
        {
            var nuevo = ParametroUsuario.Crear(usuarioId, clave, valor);
            await DbSet.AddAsync(nuevo, ct);
        }
        else
        {
            existing.SetValor(valor);
            DbSet.Update(existing);
        }

        await _context.SaveChangesAsync(ct);
    }
}
