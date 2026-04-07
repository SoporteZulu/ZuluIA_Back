using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Plan de pago con tarjeta (cuotas, recargo, diferimiento).
/// Migrado desde VB6: PLANTARJETAS.
/// </summary>
public class PlanTarjeta : AuditableEntity
{
    public long    TarjetaTipoId   { get; private set; }
    public string  Codigo          { get; private set; } = string.Empty;
    public string  Descripcion     { get; private set; } = string.Empty;
    public int     CantidadCuotas  { get; private set; }
    public decimal Recargo         { get; private set; }  // porcentaje
    public int     DiasAcreditacion { get; private set; }
    public bool    Activo          { get; private set; } = true;

    private PlanTarjeta() { }

    public static PlanTarjeta Crear(
        long tarjetaTipoId, string codigo, string descripcion,
        int cantidadCuotas, decimal recargo, int diasAcreditacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (cantidadCuotas <= 0) throw new InvalidOperationException("La cantidad de cuotas debe ser mayor a 0.");
        if (recargo < 0)         throw new InvalidOperationException("El recargo no puede ser negativo.");

        var p = new PlanTarjeta
        {
            TarjetaTipoId    = tarjetaTipoId,
            Codigo           = codigo.Trim().ToUpperInvariant(),
            Descripcion      = descripcion.Trim(),
            CantidadCuotas   = cantidadCuotas,
            Recargo          = recargo,
            DiasAcreditacion = diasAcreditacion,
            Activo           = true
        };
        p.SetCreated(userId);
        return p;
    }

    public void Actualizar(string descripcion, int cantidadCuotas, decimal recargo, int diasAcreditacion, long? userId)
    {
        Descripcion      = descripcion.Trim();
        CantidadCuotas   = cantidadCuotas;
        Recargo          = recargo;
        DiasAcreditacion = diasAcreditacion;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
}
