using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Registro de un cierre/arqueo de caja con fecha, número de cierre y usuario responsable.
/// Permite llevar historial de aperturas y cierres de caja.
/// Migrado desde VB6: clsCierresCajas / CierresCajas.
/// </summary>
public class CierreCaja : BaseEntity
{
    public DateTimeOffset FechaCierre             { get; private set; }
    public long           UsuarioId               { get; private set; }
    public DateTimeOffset FechaApertura           { get; private set; }
    public DateTimeOffset FechaAlta               { get; private set; }
    public DateTimeOffset? FechaControlTesoreria  { get; private set; }
    public int            NroCierre               { get; private set; }

    private CierreCaja() { }

    public static CierreCaja Crear(DateTimeOffset fechaApertura, DateTimeOffset fechaCierre,
        long usuarioId, int nroCierre, DateTimeOffset? fechaControlTesoreria = null)
    {
        if (usuarioId <= 0) throw new ArgumentException("El usuario es requerido.");
        if (nroCierre <= 0) throw new ArgumentException("El número de cierre debe ser mayor a cero.");
        if (fechaCierre < fechaApertura) throw new ArgumentException("La fecha de cierre no puede ser anterior a la apertura.");

        return new CierreCaja
        {
            FechaApertura          = fechaApertura,
            FechaCierre            = fechaCierre,
            UsuarioId              = usuarioId,
            FechaAlta              = DateTimeOffset.UtcNow,
            NroCierre              = nroCierre,
            FechaControlTesoreria  = fechaControlTesoreria
        };
    }

    public void RegistrarControlTesoreria(DateTimeOffset fecha)
    {
        FechaControlTesoreria = fecha;
    }
}
