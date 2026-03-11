using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ConfiguracionRepository(AppDbContext context) : IConfiguracionRepository
{
    public async Task<ConfiguracionSistema?> GetByCampoAsync(string campo, CancellationToken ct = default) =>
        await context.Config
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Campo == campo.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<ConfiguracionSistema>> GetAllAsync(CancellationToken ct = default) =>
        await context.Config
            .AsNoTracking()
            .OrderBy(x => x.Campo)
            .ToListAsync(ct);

    public async Task UpsertAsync(string campo, string? valor, CancellationToken ct = default)
    {
        var existing = await context.Config
            .FirstOrDefaultAsync(x => x.Campo == campo.ToUpperInvariant(), ct);

        if (existing is null)
        {
            var nueva = ConfiguracionSistema.Crear(campo, valor, 1, null);
            context.Config.Add(nueva);
        }
        else
        {
            existing.SetValor(valor);
            context.Config.Update(existing);
        }

        await context.SaveChangesAsync(ct);
    }
}