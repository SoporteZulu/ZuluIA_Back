using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

/// <summary>
/// Registro de copia de seguridad del sistema.
/// Migrado desde VB6: COPIASSEGURIDAD.
/// </summary>
public class CopiaSeguridad : AuditableEntity
{
    public long          SucursalId   { get; private set; }
    public DateTimeOffset Fecha       { get; private set; }
    public string        Tipo         { get; private set; } = string.Empty;  // COMPLETA | INCREMENTAL
    public string?       Ruta         { get; private set; }
    public bool          Exitosa      { get; private set; }
    public string?       Mensaje      { get; private set; }
    public long?         UsuarioId    { get; private set; }

    private CopiaSeguridad() { }

    public static CopiaSeguridad Registrar(
        long sucursalId, DateTimeOffset fecha, string tipo,
        string? ruta, bool exitosa, string? mensaje, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipo);
        var c = new CopiaSeguridad
        {
            SucursalId = sucursalId,
            Fecha      = fecha,
            Tipo       = tipo.Trim().ToUpperInvariant(),
            Ruta       = ruta?.Trim(),
            Exitosa    = exitosa,
            Mensaje    = mensaje?.Trim(),
            UsuarioId  = userId
        };
        c.SetCreated(userId);
        return c;
    }
}
