using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Precios;

/// <summary>
/// Asigna una lista de precios a una persona (cliente/proveedor).
/// Migrado desde VB6: frmListasPrecios / LISTASPRECIOSPERSONAS.
/// </summary>
public class ListaPrecioPersona : BaseEntity
{
    public long ListaPreciosId { get; private set; }
    public long PersonaId      { get; private set; }

    private ListaPrecioPersona() { }

    public static ListaPrecioPersona Crear(long listaPreciosId, long personaId)
    {
        if (listaPreciosId <= 0) throw new ArgumentException("La lista de precios es requerida.");
        if (personaId <= 0)      throw new ArgumentException("La persona es requerida.");
        return new ListaPrecioPersona { ListaPreciosId = listaPreciosId, PersonaId = personaId };
    }
}
