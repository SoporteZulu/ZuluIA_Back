using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.RRHH;

/// <summary>
/// Asignación de un perfil/rol a la relación empleado-área.
/// Migrado desde VB6: clsEmpleadoXPerfil / SUC_AREAXEMPLEADOXSUCURSAL.
/// </summary>
public class EmpleadoXPerfil : BaseEntity
{
    public long EmpleadoXAreaId { get; private set; }
    public long PerfilId        { get; private set; }
    public int  Orden           { get; private set; }

    private EmpleadoXPerfil() { }

    public static EmpleadoXPerfil Crear(long empleadoXAreaId, long perfilId, int orden = 0)
    {
        if (empleadoXAreaId <= 0) throw new ArgumentException("La asignación área-empleado es requerida.");
        if (perfilId        <= 0) throw new ArgumentException("El perfil es requerido.");

        return new EmpleadoXPerfil
        {
            EmpleadoXAreaId = empleadoXAreaId,
            PerfilId        = perfilId,
            Orden           = orden
        };
    }

    public void ActualizarOrden(int orden) => Orden = orden;
}
