using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Impuestos;

/// <summary>
/// Aplicación de un impuesto/percepción a un ítem específico.
/// Migrado desde VB6: clsImpuestoXItem / IMP_IMPUESTOXITEM.
/// </summary>
public class ImpuestoPorItem : BaseEntity
{
    public long ImpuestoId { get; private set; }
    public long ItemId { get; private set; }
    public string? Descripcion { get; private set; }
    public string? Observacion { get; private set; }

    private ImpuestoPorItem() { }

    public static ImpuestoPorItem Crear(long impuestoId, long itemId,
        string? descripcion = null, string? observacion = null)
    {
        if (impuestoId <= 0) throw new ArgumentException("El impuesto es requerido.");
        if (itemId     <= 0) throw new ArgumentException("El ítem es requerido.");

        return new ImpuestoPorItem
        {
            ImpuestoId  = impuestoId,
            ItemId      = itemId,
            Descripcion = descripcion?.Trim(),
            Observacion = observacion?.Trim()
        };
    }

    public void Actualizar(string? descripcion, string? observacion)
    {
        Descripcion = descripcion?.Trim();
        Observacion = observacion?.Trim();
    }
}
