using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Configuración de parámetros para el ranking financiero de clientes.
/// Migrado desde VB6: VTARANKINGFINANCIERO, PCTORANKINGFINANCIERO, PCTODETALLEFINANCIERORANKING.
/// </summary>
public class RankingFinanciero : AuditableEntity
{
    public long     SucursalId   { get; private set; }
    public int      Periodo      { get; private set; }
    public long     TerceroId    { get; private set; }
    public decimal  SaldoPromedio { get; private set; }
    public decimal  DiasPromPago  { get; private set; }
    public decimal  LimiteCredito { get; private set; }
    public int      Posicion      { get; private set; }
    public string?  Categoria     { get; private set; }  // A, B, C

    private RankingFinanciero() { }

    public static RankingFinanciero Registrar(
        long sucursalId, int periodo, long terceroId,
        decimal saldoPromedio, decimal diasPromPago, decimal limiteCredito,
        int posicion, string? categoria, long? userId)
    {
        var r = new RankingFinanciero
        {
            SucursalId    = sucursalId,
            Periodo       = periodo,
            TerceroId     = terceroId,
            SaldoPromedio = saldoPromedio,
            DiasPromPago  = diasPromPago,
            LimiteCredito = limiteCredito,
            Posicion      = posicion,
            Categoria     = categoria?.Trim().ToUpperInvariant()
        };
        r.SetCreated(userId);
        return r;
    }

    public void ActualizarPosicion(int posicion, string? categoria, long? userId)
    {
        Posicion  = posicion;
        Categoria = categoria?.Trim().ToUpperInvariant();
        SetUpdated(userId);
    }
}
