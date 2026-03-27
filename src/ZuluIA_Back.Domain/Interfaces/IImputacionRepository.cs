using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IImputacionRepository : IRepository<Imputacion>
{
    Task<IReadOnlyList<Imputacion>> GetByComprobanteOrigenAsync(
        long comprobanteId,
        bool incluirAnuladas = false,
        CancellationToken ct = default);

    Task<IReadOnlyList<Imputacion>> GetByComprobanteDestinoAsync(
        long comprobanteId,
        bool incluirAnuladas = false,
        CancellationToken ct = default);

    Task<IReadOnlyList<Imputacion>> GetHistorialByComprobanteAsync(
        long comprobanteId,
        bool incluirAnuladas = true,
        CancellationToken ct = default);

    Task<decimal> GetTotalImputadoAsync(
        long comprobanteDestinoId,
        CancellationToken ct = default);
}