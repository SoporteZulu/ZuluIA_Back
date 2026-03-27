using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Sucursales;

/// <summary>
/// Domicilio de una sucursal.
/// Migrado desde VB6: clsSucDomicilio / SUC_DOMICILIO.
/// Una sucursal puede tener múltiples domicilios; uno es el predeterminado.
/// </summary>
public class SucursalDomicilio : BaseEntity
{
    public long  SucursalId       { get; private set; }  // suc_id
    public long? TipoDomicilioId  { get; private set; }  // tdom_id
    public long? ProvinciaId      { get; private set; }  // prov_id
    public long? LocalidadId      { get; private set; }  // loc_id
    public string? Calle          { get; private set; }  // sdom_domicilio
    public string? Barrio         { get; private set; }  // sdom_barrio
    public string? CodigoPostal   { get; private set; }  // sdom_cp
    public string? Observacion    { get; private set; }  // sdom_observacion
    public int    Orden           { get; private set; }  // sdom_orden
    public bool   EsDefecto       { get; private set; }  // sdom_defecto

    private SucursalDomicilio() { }

    public static SucursalDomicilio Crear(
        long sucursalId,
        long? tipoDomicilioId = null,
        long? provinciaId     = null,
        long? localidadId     = null,
        string? calle         = null,
        string? barrio        = null,
        string? codigoPostal  = null,
        string? observacion   = null,
        int  orden            = 0,
        bool esDefecto        = false)
    {
        if (sucursalId <= 0) throw new ArgumentException("SucursalId es requerido.");
        return new SucursalDomicilio
        {
            SucursalId      = sucursalId,
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
