using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Application.Common.Interfaces;

public interface IAfipCaeComprobanteService
{
    Task<Result> SolicitarYAsignarAsync(Comprobante comprobante, CancellationToken cancellationToken = default);
}