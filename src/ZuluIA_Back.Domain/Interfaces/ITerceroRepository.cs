using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ITerceroRepository : IRepository<Tercero>
{
    Task<Tercero?> GetByLegajoAsync(string legajo, CancellationToken ct = default);
    Task<Tercero?> GetByNroDocumentoAsync(string nroDocumento, CancellationToken ct = default);
    Task<PagedResult<Tercero>> GetPagedAsync(int page, int pageSize, string? search, bool? soloClientes, bool? soloProveedores, CancellationToken ct = default);
    Task<bool> ExisteLegajoAsync(string legajo, long? excludeId = null, CancellationToken ct = default);
    Task<bool> ExisteNroDocumentoAsync(string nroDocumento, long? excludeId = null, CancellationToken ct = default);
}