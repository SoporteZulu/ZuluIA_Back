using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class PuntoFacturacion : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TipoId { get; private set; }
    public short Numero { get; private set; }
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; } = true;

    private PuntoFacturacion() { }

    public static PuntoFacturacion Crear(
        long sucursalId,
        long tipoId,
        short numero,
        string? descripcion,
        long? userId)
    {
        if (numero <= 0)
            throw new InvalidOperationException("El número de punto de facturación debe ser mayor a 0.");

        var punto = new PuntoFacturacion
        {
            SucursalId  = sucursalId,
            TipoId      = tipoId,
            Numero      = numero,
            Descripcion = descripcion?.Trim(),
            Activo      = true
        };

        punto.SetCreated(userId);
        return punto;
    }

    public void Actualizar(
        long tipoId,
        string? descripcion,
        long? userId)
    {
        TipoId      = tipoId;
        Descripcion = descripcion?.Trim();
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
        SetUpdated(userId);
    }
}