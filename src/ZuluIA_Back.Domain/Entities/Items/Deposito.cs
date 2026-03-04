using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Items;

public class Deposito : BaseEntity
{
    public long SucursalId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public bool EsDefault { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private Deposito() { }

    public static Deposito Crear(
        long sucursalId,
        string descripcion,
        bool esDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Deposito
        {
            SucursalId  = sucursalId,
            Descripcion = descripcion.Trim(),
            EsDefault   = esDefault,
            Activo      = true,
            CreatedAt   = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(string descripcion, bool esDefault)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
        EsDefault   = esDefault;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
    public void SetDefault() => EsDefault = true;
    public void UnsetDefault() => EsDefault = false;
}