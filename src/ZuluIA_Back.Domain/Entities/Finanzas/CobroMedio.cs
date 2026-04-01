using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class CobroMedio : BaseEntity
{
    public long CobroId { get; private set; }
    public long CajaId { get; private set; }
    public long FormaPagoId { get; private set; }
    public long? ChequeId { get; private set; }
    public decimal Importe { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;

    // Datos específicos para transferencias
    public string? BancoOrigen { get; private set; }
    public string? BancoDestino { get; private set; }
    public string? NumeroOperacion { get; private set; }
    public DateOnly? FechaAcreditacion { get; private set; }

    // Datos específicos para tarjetas
    public string? TerminalPOS { get; private set; }
    public string? NumeroCupon { get; private set; }
    public string? NumeroLote { get; private set; }
    public string? CodigoAutorizacion { get; private set; }
    public int? CantidadCuotas { get; private set; }
    public string? PlanCuotas { get; private set; }
    public DateOnly? FechaAcreditacionEstimada { get; private set; }

    private CobroMedio() { }

    public static CobroMedio Crear(
        long cobroId,
        long cajaId,
        long formaPagoId,
        long? chequeId,
        decimal importe,
        long monedaId,
        decimal cotizacion,
        // Transferencias
        string? bancoOrigen = null,
        string? bancoDestino = null,
        string? numeroOperacion = null,
        DateOnly? fechaAcreditacion = null,
        // Tarjetas
        string? terminalPOS = null,
        string? numeroCupon = null,
        string? numeroLote = null,
        string? codigoAutorizacion = null,
        int? cantidadCuotas = null,
        string? planCuotas = null,
        DateOnly? fechaAcreditacionEstimada = null)
    {
        if (importe <= 0)
            throw new InvalidOperationException("El importe del medio de cobro debe ser mayor a 0.");

        return new CobroMedio
        {
            CobroId                     = cobroId,
            CajaId                      = cajaId,
            FormaPagoId                 = formaPagoId,
            ChequeId                    = chequeId,
            Importe                     = importe,
            MonedaId                    = monedaId,
            Cotizacion                  = cotizacion <= 0 ? 1 : cotizacion,
            BancoOrigen                 = bancoOrigen?.Trim(),
            BancoDestino                = bancoDestino?.Trim(),
            NumeroOperacion             = numeroOperacion?.Trim(),
            FechaAcreditacion           = fechaAcreditacion,
            TerminalPOS                 = terminalPOS?.Trim(),
            NumeroCupon                 = numeroCupon?.Trim(),
            NumeroLote                  = numeroLote?.Trim(),
            CodigoAutorizacion          = codigoAutorizacion?.Trim(),
            CantidadCuotas              = cantidadCuotas,
            PlanCuotas                  = planCuotas?.Trim(),
            FechaAcreditacionEstimada   = fechaAcreditacionEstimada
        };
    }
}
