using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Descuento específico asignado a un cliente individual.
/// Diferente de DescuentoComercial (escalas generales); este es por cliente.
/// Migrado desde VB6: DESCUENTOSCLIENTES.
/// </summary>
public class DescuentoCliente : AuditableEntity
{
    public long    TerceroId    { get; private set; }
    public long    SucursalId   { get; private set; }
    public long?   ItemId       { get; private set; }
    public long?   CategoriaItemId { get; private set; }
    public decimal Porcentaje   { get; private set; }
    public DateOnly? VigenciaDesde { get; private set; }
    public DateOnly? VigenciaHasta { get; private set; }
    public bool    Activo       { get; private set; } = true;

    private DescuentoCliente() { }

    public static DescuentoCliente Crear(
        long terceroId, long sucursalId, decimal porcentaje,
        long? itemId, long? categoriaItemId,
        DateOnly? vigenciaDesde, DateOnly? vigenciaHasta, long? userId)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new InvalidOperationException("El porcentaje debe estar entre 0 y 100.");
        var d = new DescuentoCliente
        {
            TerceroId       = terceroId,
            SucursalId      = sucursalId,
            ItemId          = itemId,
            CategoriaItemId = categoriaItemId,
            Porcentaje      = porcentaje,
            VigenciaDesde   = vigenciaDesde,
            VigenciaHasta   = vigenciaHasta,
            Activo          = true
        };
        d.SetCreated(userId);
        return d;
    }

    public void Actualizar(decimal porcentaje, DateOnly? vigenciaDesde, DateOnly? vigenciaHasta, long? userId)
    {
        Porcentaje    = porcentaje;
        VigenciaDesde = vigenciaDesde;
        VigenciaHasta = vigenciaHasta;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
}
