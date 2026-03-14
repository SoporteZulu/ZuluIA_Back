using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class CajaCuentaBancaria : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TipoId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public long MonedaId { get; private set; }
    public string? Banco { get; private set; }
    public string? NroCuenta { get; private set; }
    public string? Cbu { get; private set; }
    public long? UsuarioId { get; private set; }
    public int NroCierreActual { get; private set; }
    public bool EsCaja { get; private set; } = true;
    public bool Activa { get; private set; } = true;

    /// <summary>Indica si la caja está abierta (habilitada para operar).</summary>
    public bool EstaAbierta { get; private set; }

    /// <summary>Fecha de la última apertura registrada.</summary>
    public DateOnly? FechaUltimaApertura { get; private set; }

    /// <summary>Saldo inicial de la última apertura.</summary>
    public decimal SaldoApertura { get; private set; }

    private CajaCuentaBancaria() { }

    public static CajaCuentaBancaria Crear(
        long sucursalId,
        long tipoId,
        string descripcion,
        long monedaId,
        bool esCaja,
        long? usuarioId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var caja = new CajaCuentaBancaria
        {
            SucursalId     = sucursalId,
            TipoId         = tipoId,
            Descripcion    = descripcion.Trim(),
            MonedaId       = monedaId,
            EsCaja         = esCaja,
            UsuarioId      = usuarioId,
            NroCierreActual = 0,
            Activa         = true
        };

        caja.SetCreated(userId);
        return caja;
    }

    public void Actualizar(
        string descripcion,
        long tipoId,
        long monedaId,
        string? banco,
        string? nroCuenta,
        string? cbu,
        long? usuarioId,
        bool esCaja,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        Descripcion = descripcion.Trim();
        TipoId      = tipoId;
        MonedaId    = monedaId;
        Banco       = banco?.Trim();
        NroCuenta   = nroCuenta?.Trim();
        Cbu         = cbu?.Trim();
        UsuarioId   = usuarioId;
        EsCaja      = esCaja;
        SetUpdated(userId);
    }

    /// <summary>
    /// Incrementa el número de cierre y retorna el nuevo número.
    /// </summary>
    public int CerrarArqueo(long? userId)
    {
        if (!EstaAbierta)
            throw new InvalidOperationException("No se puede cerrar una caja que no está abierta.");

        NroCierreActual++;
        EstaAbierta = false;
        SetUpdated(userId);
        return NroCierreActual;
    }

    /// <summary>
    /// Abre la caja para operar, registrando el saldo inicial.
    /// Equivale a frmAperturaCajasCuentasBancarias del VB6.
    /// </summary>
    public void AbrirCaja(DateOnly fechaApertura, decimal saldoInicial, long? userId)
    {
        if (EstaAbierta)
            throw new InvalidOperationException("La caja ya está abierta.");

        EstaAbierta           = true;
        FechaUltimaApertura   = fechaApertura;
        SaldoApertura         = saldoInicial;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activa = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activa = true;
        SetUpdated(userId);
    }
}