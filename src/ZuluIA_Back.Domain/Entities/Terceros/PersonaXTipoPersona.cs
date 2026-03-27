using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Asignación de un tipo de persona a una persona.
/// Migrado desde VB6: clsPersonaXTipoPersona / PER_PERSONAxTIPOPERSONA.
/// Permite que una persona sea a la vez cliente, proveedor, empleado, etc.
/// </summary>
public class PersonaXTipoPersona : BaseEntity
{
    public long PersonaId      { get; private set; }  // per_id
    public long TipoPersonaId  { get; private set; }  // tper_id
    public string? Legajo      { get; private set; }  // pxtp_legajo
    public int?    LegajoOrden { get; private set; }  // pxtp_legajoorden

    private PersonaXTipoPersona() { }

    public static PersonaXTipoPersona Crear(long personaId, long tipoPersonaId, string? legajo = null, int? legajoOrden = null)
    {
        if (personaId    <= 0) throw new ArgumentException("PersonaId es requerido.");
        if (tipoPersonaId <= 0) throw new ArgumentException("TipoPersonaId es requerido.");
        return new PersonaXTipoPersona
        {
            PersonaId     = personaId,
            TipoPersonaId = tipoPersonaId,
            Legajo        = legajo,
            LegajoOrden   = legajoOrden
        };
    }
}
