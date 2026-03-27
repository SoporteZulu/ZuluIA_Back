using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Vínculo entre dos personas (ej. cliente-transportista habitual).
/// Migrado desde VB6: clsVinculacionPersona / PER_VINCULACIONPERSONA.
/// </summary>
public class VinculacionPersona : BaseEntity
{
    public long  ClienteId        { get; private set; }  // cli_id
    public long  ProveedorId      { get; private set; }  // prov_id
    public bool  EsPredeterminado { get; private set; }  // vinc_default
    public long? TipoRelacionId   { get; private set; }  // trel_id

    private VinculacionPersona() { }

    public static VinculacionPersona Crear(long clienteId, long proveedorId, bool esPredeterminado = false, long? tipoRelacionId = null)
    {
        if (clienteId  <= 0) throw new ArgumentException("ClienteId es requerido.");
        if (proveedorId <= 0) throw new ArgumentException("ProveedorId es requerido.");
        return new VinculacionPersona
        {
            ClienteId        = clienteId,
            ProveedorId      = proveedorId,
            EsPredeterminado = esPredeterminado,
            TipoRelacionId   = tipoRelacionId
        };
    }

    public void Actualizar(bool esPredeterminado, long? tipoRelacionId)
    {
        EsPredeterminado = esPredeterminado;
        TipoRelacionId   = tipoRelacionId;
    }
}
