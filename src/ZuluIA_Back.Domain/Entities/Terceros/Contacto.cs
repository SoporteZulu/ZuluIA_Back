using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Relación entre dos personas (cliente-contacto, proveedor-contacto, etc.).
/// Migrado desde VB6: clsRelacion / CONTACTOS.
/// </summary>
public class Contacto : BaseEntity
{
    public long  PersonaId        { get; private set; }  // id_persona
    public long  PersonaContactoId { get; private set; } // id_contacto
    public long? TipoRelacionId   { get; private set; }  // trel_id

    private Contacto() { }

    public static Contacto Crear(long personaId, long personaContactoId, long? tipoRelacionId = null)
    {
        if (personaId <= 0)        throw new ArgumentException("PersonaId es requerido.");
        if (personaContactoId <= 0) throw new ArgumentException("PersonaContactoId es requerido.");
        return new Contacto
        {
            PersonaId         = personaId,
            PersonaContactoId = personaContactoId,
            TipoRelacionId    = tipoRelacionId
        };
    }

    public void ActualizarTipoRelacion(long? tipoRelacionId) => TipoRelacionId = tipoRelacionId;
}
