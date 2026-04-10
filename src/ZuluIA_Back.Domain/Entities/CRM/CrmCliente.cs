using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmCliente : AuditableEntity
{
    public long TerceroId { get; private set; }
    public string TipoCliente { get; private set; } = string.Empty;
    public string Segmento { get; private set; } = string.Empty;
    public string? Industria { get; private set; }
    public string Pais { get; private set; } = string.Empty;
    public string? Provincia { get; private set; }
    public string? Ciudad { get; private set; }
    public string? Direccion { get; private set; }
    public string OrigenCliente { get; private set; } = string.Empty;
    public string EstadoRelacion { get; private set; } = string.Empty;
    public long? ResponsableId { get; private set; }
    public string? NotasGenerales { get; private set; }
    public bool Activo { get; private set; } = true;

    private CrmCliente() { }

    public static CrmCliente Crear(
        long terceroId,
        string tipoCliente,
        string segmento,
        string? industria,
        string pais,
        string? provincia,
        string? ciudad,
        string? direccion,
        string origenCliente,
        string estadoRelacion,
        long? responsableId,
        string? notasGenerales,
        long? userId)
    {
        if (terceroId <= 0)
            throw new ArgumentException("El cliente CRM requiere un tercero válido.", nameof(terceroId));

        var entity = new CrmCliente
        {
            TerceroId = terceroId,
            TipoCliente = NormalizeRequired(tipoCliente, nameof(tipoCliente)),
            Segmento = NormalizeRequired(segmento, nameof(segmento)),
            Industria = NormalizeOptional(industria),
            Pais = NormalizeRequired(pais, nameof(pais)),
            Provincia = NormalizeOptional(provincia),
            Ciudad = NormalizeOptional(ciudad),
            Direccion = NormalizeOptional(direccion),
            OrigenCliente = NormalizeRequired(origenCliente, nameof(origenCliente)),
            EstadoRelacion = NormalizeRequired(estadoRelacion, nameof(estadoRelacion)),
            ResponsableId = responsableId,
            NotasGenerales = NormalizeOptional(notasGenerales),
            Activo = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(
        string tipoCliente,
        string segmento,
        string? industria,
        string pais,
        string? provincia,
        string? ciudad,
        string? direccion,
        string origenCliente,
        string estadoRelacion,
        long? responsableId,
        string? notasGenerales,
        long? userId)
    {
        TipoCliente = NormalizeRequired(tipoCliente, nameof(tipoCliente));
        Segmento = NormalizeRequired(segmento, nameof(segmento));
        Industria = NormalizeOptional(industria);
        Pais = NormalizeRequired(pais, nameof(pais));
        Provincia = NormalizeOptional(provincia);
        Ciudad = NormalizeOptional(ciudad);
        Direccion = NormalizeOptional(direccion);
        OrigenCliente = NormalizeRequired(origenCliente, nameof(origenCliente));
        EstadoRelacion = NormalizeRequired(estadoRelacion, nameof(estadoRelacion));
        ResponsableId = responsableId;
        NotasGenerales = NormalizeOptional(notasGenerales);
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activo = true;
        SetDeletedAt(null);
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
