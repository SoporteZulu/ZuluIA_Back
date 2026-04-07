using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Chequera (talonario de cheques) vinculada a una caja/cuenta bancaria.
/// Migrado desde VB6: CHEQUERAS.
/// </summary>
public class Chequera : AuditableEntity
{
    public long   CajaId          { get; private set; }
    public string Banco           { get; private set; } = string.Empty;
    public string NroCuenta       { get; private set; } = string.Empty;
    public int    NroDesde        { get; private set; }
    public int    NroHasta        { get; private set; }
    public int    UltimoChequeUsado { get; private set; }
    public bool   Activa          { get; private set; } = true;
    public string? Observacion    { get; private set; }

    private Chequera() { }

    public static Chequera Crear(
        long cajaId, string banco, string nroCuenta,
        int nroDesde, int nroHasta, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(banco);
        ArgumentException.ThrowIfNullOrWhiteSpace(nroCuenta);
        if (nroDesde <= 0 || nroHasta < nroDesde)
            throw new InvalidOperationException("Rango de numeración inválido.");

        var c = new Chequera
        {
            CajaId            = cajaId,
            Banco             = banco.Trim(),
            NroCuenta         = nroCuenta.Trim(),
            NroDesde          = nroDesde,
            NroHasta          = nroHasta,
            UltimoChequeUsado = nroDesde - 1,
            Activa            = true,
            Observacion       = observacion?.Trim()
        };
        c.SetCreated(userId);
        return c;
    }

    public void UsarCheque(int numero, long? userId)
    {
        if (numero < NroDesde || numero > NroHasta)
            throw new InvalidOperationException("Número fuera del rango de la chequera.");
        if (numero <= UltimoChequeUsado)
            throw new InvalidOperationException("Ese número de cheque ya fue usado.");
        UltimoChequeUsado = numero;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }

    public void Activar(long? userId)
    {
        Activa = true;
        SetUpdated(userId);
    }

    public void Actualizar(string banco, string nroCuenta, string? observacion, long? userId)
    {
        Banco       = banco.Trim();
        NroCuenta   = nroCuenta.Trim();
        Observacion = observacion?.Trim();
        SetUpdated(userId);
    }
}
