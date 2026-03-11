using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IListaPreciosRepository : IRepository<ListaPrecios>
{
    /// <summary>
    /// Retorna todas las listas activas, opcionalmente vigentes en una fecha.
    /// </summary>
    Task<IReadOnlyList<ListaPrecios>> GetActivasAsync(
        DateOnly? fecha = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retorna una lista con todos sus ítems cargados.
    /// </summary>
    Task<ListaPrecios?> GetByIdConItemsAsync(
        long id,
        CancellationToken ct = default);

    /// <summary>
    /// Retorna el precio de un ítem específico en una lista.
    /// </summary>
    Task<ListaPreciosItem?> GetPrecioItemAsync(
        long listaId,
        long itemId,
        CancellationToken ct = default);

    /// <summary>
    /// Busca la primera lista vigente en una fecha que contenga el ítem.
    /// Útil para resolver precio automáticamente al facturar.
    /// </summary>
    Task<ListaPreciosItem?> ResolverPrecioItemAsync(
        long itemId,
        long monedaId,
        DateOnly fecha,
        CancellationToken ct = default);
}