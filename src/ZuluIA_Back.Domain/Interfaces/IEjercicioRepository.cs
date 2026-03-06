using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IEjercicioRepository : IRepository<Ejercicio>
{
    Task<Ejercicio?> GetVigenteAsync(
        DateOnly fecha,
        CancellationToken ct = default);

    Task<Ejercicio?> GetByIdConSucursalesAsync(
        long id,
        CancellationToken ct = default);

    Task<IReadOnlyList<Ejercicio>> GetAllAsync(
        CancellationToken ct = default);
}