using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Impuestos;

/// <summary>
/// Aplicación de un impuesto/percepción a una sucursal específica.
/// Migrado desde VB6: clsImpuestoXSucursal / IMP_IMPUESTOXSUCURSAL.
/// </summary>
public class ImpuestoPorSucursal : BaseEntity
{
    public long ImpuestoId  { get; private set; }
    public long SucursalId  { get; private set; }
    public string? Descripcion { get; private set; }
    public string? Observacion { get; private set; }

    private ImpuestoPorSucursal() { }

    public static ImpuestoPorSucursal Crear(long impuestoId, long sucursalId,
        string? descripcion = null, string? observacion = null)
    {
        if (impuestoId <= 0) throw new ArgumentException("El impuesto es requerido.");
        if (sucursalId <= 0) throw new ArgumentException("La sucursal es requerida.");

        return new ImpuestoPorSucursal
        {
            ImpuestoId  = impuestoId,
            SucursalId  = sucursalId,
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
