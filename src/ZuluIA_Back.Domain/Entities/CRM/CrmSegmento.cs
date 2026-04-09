using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmSegmento : AuditableEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string CriteriosJson { get; private set; } = "[]";
    public string TipoSegmento { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private CrmSegmento() { }

    public static CrmSegmento Crear(
        string nombre,
        string? descripcion,
        string criteriosJson,
        string tipoSegmento,
        long? userId)
    {
        var entity = new CrmSegmento
        {
            Nombre = NormalizeRequired(nombre, nameof(nombre)),
            Descripcion = NormalizeOptional(descripcion),
            CriteriosJson = string.IsNullOrWhiteSpace(criteriosJson) ? "[]" : criteriosJson.Trim(),
            TipoSegmento = NormalizeRequired(tipoSegmento, nameof(tipoSegmento)),
            Activo = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(string nombre, string? descripcion, string criteriosJson, string tipoSegmento, long? userId)
    {
        Nombre = NormalizeRequired(nombre, nameof(nombre));
        Descripcion = NormalizeOptional(descripcion);
        CriteriosJson = string.IsNullOrWhiteSpace(criteriosJson) ? "[]" : criteriosJson.Trim();
        TipoSegmento = NormalizeRequired(tipoSegmento, nameof(tipoSegmento));
        SetUpdated(userId);
    }

    /// <summary>
    /// Indica si el segmento CRM se administra por membresía manual.
    /// </summary>
    public bool EsEstatico()
        => string.Equals(TipoSegmento, "estatico", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si el segmento CRM se resuelve dinámicamente por criterios.
    /// </summary>
    public bool EsDinamico()
        => string.Equals(TipoSegmento, "dinamico", StringComparison.OrdinalIgnoreCase);

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
