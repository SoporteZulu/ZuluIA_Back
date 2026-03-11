using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IPlanPagoRepository : IRepository<PlanPago>
{
    Task<IReadOnlyList<PlanPago>> GetActivosAsync(CancellationToken ct = default);
}