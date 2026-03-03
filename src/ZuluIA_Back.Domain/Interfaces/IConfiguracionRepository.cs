using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IConfiguracionRepository
{
    Task<ConfiguracionSistema?> GetByCampoAsync(string campo, CancellationToken ct = default);
    Task<IReadOnlyList<ConfiguracionSistema>> GetAllAsync(CancellationToken ct = default);
    Task UpsertAsync(string campo, string? valor, CancellationToken ct = default);
}