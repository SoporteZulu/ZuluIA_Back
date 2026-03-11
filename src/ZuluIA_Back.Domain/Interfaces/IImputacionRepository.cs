using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IImputacionRepository : IRepository<Imputacion>
{
    Task<IReadOnlyList<Imputacion>> GetByComprobanteOrigenAsync(
        long comprobanteId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Imputacion>> GetByComprobanteDestinoAsync(
        long comprobanteId,
        CancellationToken ct = default);

    Task<decimal> GetTotalImputadoAsync(
        long comprobanteDestinoId,
        CancellationToken ct = default);
}