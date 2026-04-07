using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Sucursales;

/// <summary>
/// Medio de contacto de una sucursal (teléfono, email, fax, etc.).
/// Migrado desde VB6: clsSucMedioContacto / SUC_MEDIOCONTACTO.
/// </summary>
public class SucursalMedioContacto : BaseEntity
{
    public long   SucursalId          { get; private set; }  // suc_id
    public long?  TipoMedioContactoId { get; private set; }  // tmc_id
    public string Valor               { get; private set; } = "";  // mcon_valor
    public int    Orden               { get; private set; }  // mcon_orden
    public bool   EsDefecto          { get; private set; }  // mcon_defecto
    public string? Observacion       { get; private set; }  // mcon_observacion

    private SucursalMedioContacto() { }

    public static SucursalMedioContacto Crear(
        long sucursalId,
        string valor,
        long? tipoMedioContactoId = null,
        int  orden     = 0,
        bool esDefecto = false,
        string? observacion = null)
    {
        if (sucursalId <= 0) throw new ArgumentException("SucursalId es requerido.");
        if (string.IsNullOrWhiteSpace(valor)) throw new ArgumentException("El valor es requerido.");
        return new SucursalMedioContacto
        {
            SucursalId          = sucursalId,
            TipoMedioContactoId = tipoMedioContactoId,
            Valor               = valor.Trim(),
            Orden               = orden,
            EsDefecto           = esDefecto,
            Observacion         = observacion
        };
    }

    public void Actualizar(string valor, long? tipoMedioContactoId, int orden, bool esDefecto, string? observacion)
    {
        if (string.IsNullOrWhiteSpace(valor)) throw new ArgumentException("El valor es requerido.");
        TipoMedioContactoId = tipoMedioContactoId;
        Valor               = valor.Trim();
        Orden               = orden;
        EsDefecto           = esDefecto;
        Observacion         = observacion;
    }
}
