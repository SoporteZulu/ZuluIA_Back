using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Banco (entidad financiera).
/// Catálogo simple utilizado en cuentas bancarias y cheques.
/// Migrado desde VB6: clsBancos / BANCOS.
/// </summary>
public class Banco : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;

    private Banco() { }

    public static Banco Crear(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new Banco { Descripcion = descripcion.Trim() };
    }

    public void Actualizar(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
    }
}
