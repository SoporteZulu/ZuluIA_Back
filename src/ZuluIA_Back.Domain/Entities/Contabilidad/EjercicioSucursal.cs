using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

public class EjercicioSucursal : BaseEntity
{
    public long EjercicioId { get; private set; }
    public long SucursalId { get; private set; }
    public bool UsaContabilidad { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private EjercicioSucursal() { }

    public static EjercicioSucursal Crear(
        long ejercicioId,
        long sucursalId,
        bool usaContabilidad = true) =>
        new()
        {
            EjercicioId      = ejercicioId,
            SucursalId       = sucursalId,
            UsaContabilidad  = usaContabilidad,
            CreatedAt        = DateTimeOffset.UtcNow
        };

    public void SetUsaContabilidad(bool valor) => UsaContabilidad = valor;
}