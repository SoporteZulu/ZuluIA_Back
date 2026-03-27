using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Medio de contacto de una persona (teléfono, email, fax, etc.).
/// Migrado desde VB6: clsMedioContacto / PER_MEDIOCONTACTO.
/// Una persona puede tener múltiples medios; uno es el predeterminado.
/// </summary>
public class MedioContacto : BaseEntity
{
    public long   PersonaId          { get; private set; }  // per_id
    public long?  TipoMedioContactoId { get; private set; } // tmc_id
    public string Valor              { get; private set; } = "";  // mcon_valor
    public int    Orden              { get; private set; }  // mcon_orden
    public bool   EsDefecto         { get; private set; }  // mcon_defecto
    public string? Observacion      { get; private set; }  // mcon_observacion

    private MedioContacto() { }

    public static MedioContacto Crear(
        long personaId,
        string valor,
        long? tipoMedioContactoId = null,
        int  orden     = 0,
        bool esDefecto = false,
        string? observacion = null)
    {
        if (personaId <= 0) throw new ArgumentException("PersonaId es requerido.");
        if (string.IsNullOrWhiteSpace(valor)) throw new ArgumentException("El valor es requerido.");
        return new MedioContacto
        {
            PersonaId           = personaId,
            TipoMedioContactoId = tipoMedioContactoId,
            Valor               = valor.Trim(),
            Orden               = orden,
            EsDefecto           = esDefecto,
            Observacion         = observacion
        };
    }

    public void Actualizar(
        string valor,
        long? tipoMedioContactoId,
        int  orden,
        bool esDefecto,
        string? observacion)
    {
        if (string.IsNullOrWhiteSpace(valor)) throw new ArgumentException("El valor es requerido.");
        TipoMedioContactoId = tipoMedioContactoId;
        Valor               = valor.Trim();
        Orden               = orden;
        EsDefecto           = esDefecto;
        Observacion         = observacion;
    }
}
