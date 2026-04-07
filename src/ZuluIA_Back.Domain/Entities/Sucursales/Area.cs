using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Sucursales;

/// <summary>
/// Área/departamento dentro de una sucursal (producción, ventas, administración, etc.).
/// Permite agrupar empleados por área funcional.
/// Migrado desde VB6: clsAreas / SUC_AREA.
/// </summary>
public class Area : BaseEntity
{
    public string  Descripcion { get; private set; } = string.Empty;
    public string? Codigo      { get; private set; }
    public long?   SucursalId  { get; private set; }

    private Area() { }

    public static Area Crear(string descripcion, string? codigo = null, long? sucursalId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new Area
        {
            Descripcion = descripcion.Trim(),
            Codigo      = codigo?.Trim().ToUpperInvariant(),
            SucursalId  = sucursalId
        };
    }

    public void Actualizar(string descripcion, string? codigo, long? sucursalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
        Codigo      = codigo?.Trim().ToUpperInvariant();
        SucursalId  = sucursalId;
    }
}
