using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

/// <summary>
/// Configuración de impresora fiscal / ECF por punto de facturación.
/// Migrado desde VB6: frmAjustesPV / FISC_CONFIGURACIONES_GENERALES.
/// Campos: fcg_id_tipocomprobante, fcg_pfac_id, fcg_marco, fcg_puerto,
///         fcg_copias_default, fcg_clave_activacion, tec_id (tecnología),
///         if_id (interfaz fiscal), fcg_directorio_local, fcg_online.
/// </summary>
public class ConfiguracionFiscal : BaseEntity
{
    public long    PuntoFacturacionId    { get; private set; }
    public long    TipoComprobanteId     { get; private set; }
    public long?   TecnologiaId          { get; private set; }  // tec_id
    public long?   InterfazFiscalId      { get; private set; }  // if_id
    public int?    Marco                 { get; private set; }  // fcg_marco
    public string? Puerto                { get; private set; }  // fcg_puerto
    public int     CopiasDefault         { get; private set; } = 2;
    public string? ClaveActivacion       { get; private set; }
    public string? DirectorioLocal       { get; private set; }
    public string? DirectorioLocalBackup { get; private set; }
    public int?    TimerInicial          { get; private set; }
    public int?    TimerSincronizacion   { get; private set; }
    public bool    Online                { get; private set; }

    private ConfiguracionFiscal() { }

    public static ConfiguracionFiscal Crear(
        long    puntoFacturacionId,
        long    tipoComprobanteId,
        long?   tecnologiaId          = null,
        long?   interfazFiscalId      = null,
        int?    marco                 = null,
        string? puerto                = null,
        int     copiasDefault         = 2,
        string? claveActivacion       = null,
        string? directorioLocal       = null,
        string? directorioLocalBackup = null,
        int?    timerInicial          = null,
        int?    timerSincronizacion   = null,
        bool    online                = false)
    {
        return new ConfiguracionFiscal
        {
            PuntoFacturacionId    = puntoFacturacionId,
            TipoComprobanteId     = tipoComprobanteId,
            TecnologiaId          = tecnologiaId,
            InterfazFiscalId      = interfazFiscalId,
            Marco                 = marco,
            Puerto                = puerto?.Trim(),
            CopiasDefault         = copiasDefault,
            ClaveActivacion       = claveActivacion?.Trim(),
            DirectorioLocal       = directorioLocal?.Trim(),
            DirectorioLocalBackup = directorioLocalBackup?.Trim(),
            TimerInicial          = timerInicial,
            TimerSincronizacion   = timerSincronizacion,
            Online                = online
        };
    }

    public void Actualizar(
        long?   tecnologiaId,
        long?   interfazFiscalId,
        int?    marco,
        string? puerto,
        int     copiasDefault,
        string? claveActivacion,
        string? directorioLocal,
        string? directorioLocalBackup,
        int?    timerInicial,
        int?    timerSincronizacion,
        bool    online)
    {
        TecnologiaId          = tecnologiaId;
        InterfazFiscalId      = interfazFiscalId;
        Marco                 = marco;
        Puerto                = puerto?.Trim();
        CopiasDefault         = copiasDefault;
        ClaveActivacion       = claveActivacion?.Trim();
        DirectorioLocal       = directorioLocal?.Trim();
        DirectorioLocalBackup = directorioLocalBackup?.Trim();
        TimerInicial          = timerInicial;
        TimerSincronizacion   = timerSincronizacion;
        Online                = online;
    }
}
