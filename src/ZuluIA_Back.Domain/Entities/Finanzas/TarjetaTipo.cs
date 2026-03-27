using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Tipo/marca de tarjeta de crédito o débito (VISA, MASTERCARD, etc.).
/// Migrado desde VB6: TARJETAS.
/// </summary>
public class TarjetaTipo : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   EsDebito    { get; private set; }
    public bool   Activa      { get; private set; } = true;

    private TarjetaTipo() { }

    public static TarjetaTipo Crear(string codigo, string descripcion, bool esDebito, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var t = new TarjetaTipo
        {
            Codigo      = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            EsDebito    = esDebito,
            Activa      = true
        };
        t.SetCreated(userId);
        return t;
    }

    public void Actualizar(string descripcion, bool esDebito, long? userId)
    {
        Descripcion = descripcion.Trim();
        EsDebito    = esDebito;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activa = true; SetUpdated(userId); }
}
