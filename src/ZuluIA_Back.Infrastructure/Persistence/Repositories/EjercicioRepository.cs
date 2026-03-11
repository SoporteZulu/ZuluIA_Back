using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class EjercicioRepository(AppDbContext context)
    : BaseRepository<Ejercicio>(context), IEjercicioRepository
{
    public async Task<Ejercicio?> GetVigenteAsync(
        DateOnly fecha,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.FechaInicio <= fecha &&
                x.FechaFin    >= fecha &&
                !x.Cerrado,
                ct);

    public async Task<Ejercicio?> GetByIdConSucursalesAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Sucursales)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    new public async Task<IReadOnlyList<Ejercicio>> GetAllAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .OrderByDescending(x => x.FechaInicio)
            .ToListAsync(ct);
}