using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

/// <summary>
/// Configuración de conexión y mapeo con sistemas externos.
/// Migrado desde VB6: INTEGRADORA.
/// </summary>
public class Integradora : AuditableEntity
{
    public string  Codigo        { get; private set; } = string.Empty;
    public string  Nombre        { get; private set; } = string.Empty;
    public string  TipoSistema   { get; private set; } = string.Empty;
    public string? UrlEndpoint   { get; private set; }
    public string? ApiKey        { get; private set; }
    public string? Configuracion { get; private set; }  // JSON con config extendida
    public bool    Activa        { get; private set; } = true;

    private Integradora() { }

    public static Integradora Crear(
        string codigo, string nombre, string tipoSistema,
        string? urlEndpoint, string? apiKey, string? configuracion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        var i = new Integradora
        {
            Codigo        = codigo.Trim().ToUpperInvariant(),
            Nombre        = nombre.Trim(),
            TipoSistema   = tipoSistema.Trim().ToUpperInvariant(),
            UrlEndpoint   = urlEndpoint?.Trim(),
            Configuracion = configuracion?.Trim(),
            Activa        = true
            // ApiKey is set with a dedicated method to not appear in requests
        };
        if (apiKey is not null) i.ApiKey = apiKey.Trim();
        i.SetCreated(userId);
        return i;
    }

    public void Actualizar(string nombre, string tipoSistema, string? urlEndpoint, string? configuracion, long? userId)
    {
        Nombre        = nombre.Trim();
        TipoSistema   = tipoSistema.Trim().ToUpperInvariant();
        UrlEndpoint   = urlEndpoint?.Trim();
        Configuracion = configuracion?.Trim();
        SetUpdated(userId);
    }

    public void RotarApiKey(string nuevaKey, long? userId) { ApiKey = nuevaKey.Trim(); SetUpdated(userId); }
    public void Desactivar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activa = true; SetUpdated(userId); }
}
