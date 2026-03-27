using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class ChequeHistorial : AuditableEntity
{
    public long ChequeId { get; private set; }
    public long CajaId { get; private set; }
    public long? TerceroId { get; private set; }
    public TipoOperacionCheque Operacion { get; private set; }
    public EstadoCheque? EstadoAnterior { get; private set; }
    public EstadoCheque EstadoNuevo { get; private set; }
    public DateOnly FechaOperacion { get; private set; }
    public DateOnly? FechaAcreditacion { get; private set; }
    public string? Observacion { get; private set; }

    private ChequeHistorial() { }

    public static ChequeHistorial Registrar(
        long chequeId,
        long cajaId,
        long? terceroId,
        TipoOperacionCheque operacion,
        EstadoCheque? estadoAnterior,
        EstadoCheque estadoNuevo,
        DateOnly fechaOperacion,
        DateOnly? fechaAcreditacion,
        string? observacion,
        long? userId)
    {
        var historial = new ChequeHistorial
        {
            ChequeId = chequeId,
            CajaId = cajaId,
            TerceroId = terceroId,
            Operacion = operacion,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            FechaOperacion = fechaOperacion,
            FechaAcreditacion = fechaAcreditacion,
            Observacion = observacion?.Trim()
        };

        historial.SetCreated(userId);
        return historial;
    }
}
