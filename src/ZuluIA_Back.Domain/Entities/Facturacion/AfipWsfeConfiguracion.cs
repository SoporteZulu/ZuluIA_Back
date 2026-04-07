using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class AfipWsfeConfiguracion : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long PuntoFacturacionId { get; private set; }
    public bool Habilitado { get; private set; }
    public bool Produccion { get; private set; }
    public bool UsaCaeaPorDefecto { get; private set; }
    public string CuitEmisor { get; private set; } = string.Empty;
    public string? CertificadoAlias { get; private set; }
    public string? Observacion { get; private set; }

    private AfipWsfeConfiguracion() { }

    public static AfipWsfeConfiguracion Crear(
        long sucursalId,
        long puntoFacturacionId,
        bool habilitado,
        bool produccion,
        bool usaCaeaPorDefecto,
        string cuitEmisor,
        string? certificadoAlias,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuitEmisor);

        var config = new AfipWsfeConfiguracion
        {
            SucursalId = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            Habilitado = habilitado,
            Produccion = produccion,
            UsaCaeaPorDefecto = usaCaeaPorDefecto,
            CuitEmisor = cuitEmisor.Trim(),
            CertificadoAlias = certificadoAlias?.Trim(),
            Observacion = observacion?.Trim()
        };

        config.SetCreated(userId);
        return config;
    }

    public void Actualizar(
        bool habilitado,
        bool produccion,
        bool usaCaeaPorDefecto,
        string cuitEmisor,
        string? certificadoAlias,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuitEmisor);

        Habilitado = habilitado;
        Produccion = produccion;
        UsaCaeaPorDefecto = usaCaeaPorDefecto;
        CuitEmisor = cuitEmisor.Trim();
        CertificadoAlias = certificadoAlias?.Trim();
        Observacion = observacion?.Trim();
        SetUpdated(userId);
    }
}
