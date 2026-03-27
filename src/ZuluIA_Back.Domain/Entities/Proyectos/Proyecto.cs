using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Proyectos;

/// <summary>
/// Proyecto de trabajo o contrato de servicios, vinculado a comprobantes y terceros.
/// Migrado desde VB6: clsProyectos / Proyectos.
/// </summary>
public class Proyecto : BaseEntity
{
    public string  Codigo       { get; private set; } = string.Empty;
    public string  Descripcion  { get; private set; } = string.Empty;
    public string? Observacion  { get; private set; }
    /// <summary>Estado del proyecto: "activo", "finalizado", "anulado".</summary>
    public string  Estado       { get; private set; } = "activo";
    public DateOnly? FechaInicio { get; private set; }
    public DateOnly? FechaFin   { get; private set; }
    public long    SucursalId   { get; private set; }
    public long?   TerceroId    { get; private set; }
    public bool    Anulada      { get; private set; }
    public int     TotalCuotas  { get; private set; }
    /// <summary>Si es verdadero, sólo actúa como nodo padre (no recibe comprobantes directos).</summary>
    public bool    SoloPadre    { get; private set; }

    private Proyecto() { }

    public static Proyecto Crear(string codigo, string descripcion, long sucursalId,
        long? terceroId = null, DateOnly? fechaInicio = null, DateOnly? fechaFin = null,
        int totalCuotas = 0, bool soloPadre = false, string? observacion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (sucursalId <= 0) throw new ArgumentException("La sucursal es requerida.");

        return new Proyecto
        {
            Codigo       = codigo.Trim().ToUpperInvariant(),
            Descripcion  = descripcion.Trim(),
            Observacion  = observacion?.Trim(),
            Estado       = "activo",
            FechaInicio  = fechaInicio,
            FechaFin     = fechaFin,
            SucursalId   = sucursalId,
            TerceroId    = terceroId,
            Anulada      = false,
            TotalCuotas  = totalCuotas,
            SoloPadre    = soloPadre
        };
    }

    public void Actualizar(string descripcion, DateOnly? fechaInicio, DateOnly? fechaFin,
        int totalCuotas, bool soloPadre, string? observacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
        FechaInicio = fechaInicio;
        FechaFin    = fechaFin;
        TotalCuotas = totalCuotas;
        SoloPadre   = soloPadre;
        Observacion = observacion?.Trim();
    }

    public void Finalizar()  => Estado = "finalizado";
    public void Reactivar()  => Estado = "activo";
    public void Anular()     { Anulada = true; Estado = "anulado"; }
}
