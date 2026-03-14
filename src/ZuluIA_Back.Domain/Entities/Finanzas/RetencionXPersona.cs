using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Asignación de un tipo de retención a un tercero (proveedor/cliente).
/// Determina qué retenciones se practican al emitir pagos a un proveedor específico.
/// Equivale a la tabla RetencionesXPersona del sistema VB6.
/// </summary>
public class RetencionXPersona : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long TipoRetencionId { get; private set; }
    public string? Descripcion { get; private set; }

    private RetencionXPersona() { }

    public static RetencionXPersona Crear(
        long terceroId,
        long tipoRetencionId,
        string? descripcion,
        long? userId)
    {
        var r = new RetencionXPersona
        {
            TerceroId        = terceroId,
            TipoRetencionId  = tipoRetencionId,
            Descripcion      = descripcion?.Trim()
        };
        r.SetCreated(userId);
        return r;
    }

    public void Actualizar(string? descripcion, long? userId)
    {
        Descripcion = descripcion?.Trim();
        SetUpdated(userId);
    }

    public void Eliminar(long? userId)
    {
        SetDeleted();
        SetUpdated(userId);
    }
}
