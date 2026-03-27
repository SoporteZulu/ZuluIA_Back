using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Proyectos;

/// <summary>
/// Vinculación entre un comprobante y un proyecto (asignación de cuota/porcentaje).
/// Migrado desde VB6: ClsComprobantesProyectos / ComprobantesProyectos.
/// </summary>
public class ComprobanteProyecto : BaseEntity
{
    public long    ComprobanteId { get; private set; }
    public long    ProyectoId    { get; private set; }
    /// <summary>Porcentaje del comprobante asignado al proyecto (0-100).</summary>
    public decimal Porcentaje    { get; private set; }
    public string? Observacion   { get; private set; }
    public int     NroCuota      { get; private set; }
    public decimal Importe       { get; private set; }
    public bool    Deshabilitada { get; private set; }

    private ComprobanteProyecto() { }

    public static ComprobanteProyecto Crear(long comprobanteId, long proyectoId,
        decimal porcentaje, decimal importe, int nroCuota = 0, string? observacion = null)
    {
        if (comprobanteId <= 0) throw new ArgumentException("El comprobante es requerido.");
        if (proyectoId    <= 0) throw new ArgumentException("El proyecto es requerido.");
        if (porcentaje < 0 || porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");

        return new ComprobanteProyecto
        {
            ComprobanteId = comprobanteId,
            ProyectoId    = proyectoId,
            Porcentaje    = porcentaje,
            Importe       = importe,
            NroCuota      = nroCuota,
            Observacion   = observacion?.Trim(),
            Deshabilitada = false
        };
    }

    public void Deshabilitar() => Deshabilitada = true;
    public void Habilitar()    => Deshabilitada = false;
    public void ActualizarImporte(decimal importe) => Importe = importe;
}
