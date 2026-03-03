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
        NroCierreActual++;
        SetUpdated(userId);
        return NroCierreActual;
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