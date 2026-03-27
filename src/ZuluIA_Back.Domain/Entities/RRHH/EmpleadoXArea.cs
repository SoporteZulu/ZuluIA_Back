using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.RRHH;

/// <summary>
/// Asignación de un empleado a un área funcional de la empresa.
/// Migrado desde VB6: clsEmpleadoXArea / SUC_AREAXEMPLEADO.
/// </summary>
public class EmpleadoXArea : BaseEntity
{
    public long EmpleadoId { get; private set; }
    public long AreaId     { get; private set; }
    public int  Orden      { get; private set; }

    private EmpleadoXArea() { }

    public static EmpleadoXArea Crear(long empleadoId, long areaId, int orden = 0)
    {
        if (empleadoId <= 0) throw new ArgumentException("El empleado es requerido.");
        if (areaId     <= 0) throw new ArgumentException("El área es requerida.");

        return new EmpleadoXArea
        {
            EmpleadoId = empleadoId,
            AreaId     = areaId,
            Orden      = orden
        };
    }

    public void ActualizarOrden(int orden) => Orden = orden;
}
