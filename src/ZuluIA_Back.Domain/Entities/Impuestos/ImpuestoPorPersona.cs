using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Impuestos;

/// <summary>
/// Aplicación de un impuesto/percepción a un tercero específico
/// (con descripción y condiciones particulares).
/// Migrado desde VB6: clsImpuestoXPersona / IMP_IMPUESTOXPERSONA.
/// </summary>
public class ImpuestoPorPersona : BaseEntity
{
    public long ImpuestoId { get; private set; }
    public long TerceroId { get; private set; }
    public string? Descripcion { get; private set; }
    public string? Observacion { get; private set; }

    private ImpuestoPorPersona() { }

    public static ImpuestoPorPersona Crear(long impuestoId, long terceroId,
        string? descripcion = null, string? observacion = null)
    {
        if (impuestoId <= 0) throw new ArgumentException("El impuesto es requerido.");
        if (terceroId  <= 0) throw new ArgumentException("El tercero es requerido.");

        return new ImpuestoPorPersona
        {
            ImpuestoId  = impuestoId,
            TerceroId   = terceroId,
            Descripcion = descripcion?.Trim(),
            Observacion = observacion?.Trim()
        };
    }

    public void Actualizar(string? descripcion, string? observacion)
    {
        Descripcion = descripcion?.Trim();
        Observacion = observacion?.Trim();
    }
}
