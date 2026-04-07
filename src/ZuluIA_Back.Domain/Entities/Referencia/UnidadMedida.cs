using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

/// <summary>
/// Unidad de medida con soporte de conversión entre unidades.
/// Migrado desde VB6: clsUnidades / ume_unidades.
/// </summary>
public class UnidadMedida : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    /// <summary>Abreviatura corta, ej: "kg", "lt".</summary>
    public string? Disminutivo { get; private set; }
    /// <summary>Factor de conversión respecto a la unidad base.</summary>
    public decimal Multiplicador { get; private set; } = 1m;
    public bool EsUnidadBase { get; private set; } = true;
    /// <summary>FK a la unidad base de referencia (null si EsUnidadBase = true).</summary>
    public long? UnidadBaseId { get; private set; }
    public bool Activa { get; private set; } = true;

    private UnidadMedida() { }

    public static UnidadMedida Crear(string codigo, string descripcion, string? disminutivo = null,
        decimal multiplicador = 1m, bool esUnidadBase = true, long? unidadBaseId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new UnidadMedida
        {
            Codigo        = codigo.Trim().ToUpperInvariant(),
            Descripcion   = descripcion.Trim(),
            Disminutivo   = disminutivo?.Trim(),
            Multiplicador = multiplicador <= 0 ? 1m : multiplicador,
            EsUnidadBase  = esUnidadBase,
            UnidadBaseId  = esUnidadBase ? null : unidadBaseId,
            Activa        = true
        };
    }

    public void Actualizar(string descripcion, string? disminutivo, decimal multiplicador, bool esUnidadBase, long? unidadBaseId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion   = descripcion.Trim();
        Disminutivo   = disminutivo?.Trim();
        Multiplicador = multiplicador <= 0 ? 1m : multiplicador;
        EsUnidadBase  = esUnidadBase;
        UnidadBaseId  = esUnidadBase ? null : unidadBaseId;
    }

    public void Activar()   => Activa = true;
    public void Desactivar() => Activa = false;
}