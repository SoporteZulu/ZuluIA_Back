using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

/// <summary>
/// Inscripción de un cliente en una lista de sorteo.
/// Migrado desde VB6: SorteoListaXCliente.
/// </summary>
public class SorteoListaXCliente : BaseEntity
{
    public long   SorteoListaId { get; private set; }
    public long   TerceroId     { get; private set; }
    public int    NroTicket     { get; private set; }
    public bool   Ganador       { get; private set; }

    private SorteoListaXCliente() { }

    public static SorteoListaXCliente Inscribir(long sorteoListaId, long terceroId, int nroTicket)
    {
        if (nroTicket <= 0) throw new InvalidOperationException("El número de ticket debe ser positivo.");
        return new SorteoListaXCliente
        {
            SorteoListaId = sorteoListaId,
            TerceroId     = terceroId,
            NroTicket     = nroTicket,
            Ganador       = false
        };
    }

    public void MarcarGanador() => Ganador = true;
}
