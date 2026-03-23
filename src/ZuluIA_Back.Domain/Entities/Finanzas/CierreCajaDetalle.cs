using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Línea de detalle de un cierre de caja: indica qué caja/cuenta
/// participó y la diferencia de arqueo detectada.
/// Migrado desde VB6: clsCierresCajasDetalle / CierresCajasDetalle.
/// </summary>
public class CierreCajaDetalle : BaseEntity
{
    public long    CierreId             { get; private set; }
    public long    CajaCuentaBancariaId { get; private set; }
    public decimal Diferencia           { get; private set; }

    private CierreCajaDetalle() { }

    public static CierreCajaDetalle Crear(long cierreId, long cajaCuentaBancariaId, decimal diferencia)
    {
        if (cierreId             <= 0) throw new ArgumentException("El cierre es requerido.");
        if (cajaCuentaBancariaId <= 0) throw new ArgumentException("La caja/cuenta es requerida.");

        return new CierreCajaDetalle
        {
            CierreId             = cierreId,
            CajaCuentaBancariaId = cajaCuentaBancariaId,
            Diferencia           = diferencia
        };
    }

    public void ActualizarDiferencia(decimal diferencia) => Diferencia = diferencia;
}
