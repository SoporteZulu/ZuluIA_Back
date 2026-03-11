using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ICartaPorteRepository : IRepository<CartaPorte>
{
    Task<PagedResult<CartaPorte>> GetPagedAsync(
        int page,
        int pageSize,
        long? comprobanteId,
        EstadoCartaPorte? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<CartaPorte?> GetByNroCtgAsync(
        string nroCtg,
        CancellationToken ct = default);
}