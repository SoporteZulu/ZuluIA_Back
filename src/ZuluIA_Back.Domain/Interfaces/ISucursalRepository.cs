using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Sucursales;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ISucursalRepository : IRepository<Sucursal>
{
    Task<IReadOnlyList<Sucursal>> GetAllActivasAsync(CancellationToken ct = default);
    Task<bool> ExisteCuitAsync(string cuit, long? excludeId = null, CancellationToken ct = default);
    Task<Sucursal?> GetCasaMatrizAsync(CancellationToken ct = default);
}