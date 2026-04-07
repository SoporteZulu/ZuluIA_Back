using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Domicilio de una persona (cliente, proveedor, empleado, etc.).
/// Migrado desde VB6: clsDomicilio / PER_DOMICILIO.
/// Una persona puede tener múltiples domicilios; uno es el predeterminado.
/// </summary>
public class PersonaDomicilio : BaseEntity
{
    public const int MaxCantidadPorPersona = 3;

    public long  PersonaId        { get; private set; }  // per_id
    public long? TipoDomicilioId  { get; private set; }  // tdom_id
    public long? ProvinciaId      { get; private set; }  // prov_id
    public long? LocalidadId      { get; private set; }  // loc_id
    public string? Calle          { get; private set; }  // dom_domicilio
    public string? Barrio         { get; private set; }  // dom_barrio
    public string? CodigoPostal   { get; private set; }  // dom_cp
    public string? Observacion    { get; private set; }  // dom_observacion
    public int    Orden           { get; private set; }  // dom_orden
    public bool   EsDefecto       { get; private set; }  // dom_defecto

    private PersonaDomicilio() { }

    public static PersonaDomicilio Crear(
        long personaId,
        long? tipoDomicilioId  = null,
        long? provinciaId      = null,
        long? localidadId      = null,
        string? calle          = null,
        string? barrio         = null,
        string? codigoPostal   = null,
        string? observacion    = null,
        int  orden             = 0,
        bool esDefecto         = false)
    {
        if (personaId <= 0) throw new ArgumentException("PersonaId es requerido.");
        return new PersonaDomicilio
        {
            PersonaId       = personaId,
            TipoDomicilioId = tipoDomicilioId,
            ProvinciaId     = provinciaId,
            LocalidadId     = localidadId,
            Calle           = calle,
            Barrio          = barrio,
            CodigoPostal    = codigoPostal,
            Observacion     = observacion,
            Orden           = orden,
            EsDefecto       = esDefecto
        };
    }

    public void Actualizar(
        long? tipoDomicilioId,
        long? provinciaId,
        long? localidadId,
        string? calle,
        string? barrio,
        string? codigoPostal,
        string? observacion,
        int  orden,
        bool esDefecto)
    {
        TipoDomicilioId = tipoDomicilioId;
        ProvinciaId     = provinciaId;
        LocalidadId     = localidadId;
        Calle           = calle;
        Barrio          = barrio;
        CodigoPostal    = codigoPostal;
        Observacion     = observacion;
        Orden           = orden;
        EsDefecto       = esDefecto;
    }
}
