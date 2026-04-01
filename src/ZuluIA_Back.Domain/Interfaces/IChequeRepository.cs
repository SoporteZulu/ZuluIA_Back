using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IChequeRepository : IRepository<Cheque>
{
    Task<PagedResult<Cheque>> GetPagedAsync(
        int page,
        int pageSize,
        long? cajaId,
        long? terceroId,
        EstadoCheque? estado,
        TipoCheque? tipo,
        bool? esALaOrden,
        bool? esCruzado,
        string? banco,
        string? nroCheque,
        string? titular,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<IReadOnlyList<Cheque>> GetCarteraAsync(
        long cajaId,
        CancellationToken ct = default);
}