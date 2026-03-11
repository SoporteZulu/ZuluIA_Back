using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class SeguridadRepository : BaseRepository<Seguridad>, ISeguridadRepository
{
    private readonly AppDbContext _context;

    public SeguridadRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, bool>> GetPermisosUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default)
    {
        // Une la tabla seguridad con seguridad_usuario para este usuario
        var permisos = await (
            from s in _context.Seguridad.AsNoTracking()
            join su in _context.SeguridadUsuario.AsNoTracking()
                on new { SeguridadId = s.Id, UsuarioId = usuarioId }
                equals new { su.SeguridadId, su.UsuarioId }
                into suJoin
            from su in suJoin.DefaultIfEmpty()
            select new
            {
                s.Identificador,
                s.AplicaSeguridadPorUsuario,
                Valor = su != null ? su.Valor : false
            })
            .ToListAsync(ct);

        return permisos.ToDictionary(
            p => p.Identificador,
            // Si no aplica por usuario → siempre true
            p => !p.AplicaSeguridadPorUsuario || p.Valor);
    }

    public async Task<bool> TienePermisoAsync(
        long usuarioId,
        string identificador,
        CancellationToken ct = default)
    {
        var seguridad = await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Identificador == identificador.ToUpperInvariant(), ct);

        if (seguridad is null) return false;
        if (!seguridad.AplicaSeguridadPorUsuario) return true;

        return await _context.SeguridadUsuario
            .AsNoTracking()
            .AnyAsync(x =>
                x.SeguridadId == seguridad.Id &&
                x.UsuarioId   == usuarioId    &&
                x.Valor       == true,
                ct);
    }

    public async Task<Seguridad?> GetByIdentificadorAsync(
        string identificador,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Identificador == identificador.ToUpperInvariant(), ct);
}
