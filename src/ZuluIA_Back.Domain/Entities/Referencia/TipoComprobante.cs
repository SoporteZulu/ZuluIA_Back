using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

public class TipoComprobante : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool EsVenta { get; private set; } = true;
    public bool EsCompra { get; private set; }
    public bool EsInterno { get; private set; }
    public bool AfectaStock { get; private set; }
    public bool AfectaCuentaCorriente { get; private set; } = true;
    public bool GeneraAsiento { get; private set; } = true;
    public short? TipoAfip { get; private set; }
    public char? LetraAfip { get; private set; }
    public bool Activo { get; private set; } = true;

    private TipoComprobante() { }
}