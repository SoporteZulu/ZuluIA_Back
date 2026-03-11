using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Relación entre una caja/cuenta y una forma de pago habilitada.
/// Mapea la tabla forma_pago_caja.
/// </summary>
public class FormaPagoCaja : BaseEntity
{
    public long CajaId { get; private set; }
    public long FormaPagoId { get; private set; }
    public long MonedaId { get; private set; }
    public bool Habilitado { get; private set; } = true;

    private FormaPagoCaja() { }

    public static FormaPagoCaja Crear(long cajaId, long formaPagoId, long monedaId)
    {
        return new FormaPagoCaja
        {
            CajaId      = cajaId,
            FormaPagoId = formaPagoId,
            MonedaId    = monedaId,
            Habilitado  = true
        };
    }

    public void Habilitar() => Habilitado = true;
    public void Deshabilitar() => Habilitado = false;
}