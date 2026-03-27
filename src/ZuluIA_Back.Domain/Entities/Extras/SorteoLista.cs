using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

/// <summary>
/// Lista de sorteo/rifa para fidelización de clientes.
/// Migrado desde VB6: SorteoLista.
/// </summary>
public class SorteoLista : AuditableEntity
{
    public long     SucursalId   { get; private set; }
    public long     TipoId       { get; private set; }
    public string   Descripcion  { get; private set; } = string.Empty;
    public DateOnly FechaInicio  { get; private set; }
    public DateOnly FechaFin     { get; private set; }
    public bool     Activa       { get; private set; } = true;

    private readonly List<SorteoListaXCliente> _participantes = [];
    public IReadOnlyCollection<SorteoListaXCliente> Participantes => _participantes.AsReadOnly();

    private SorteoLista() { }

    public static SorteoLista Crear(
        long sucursalId, long tipoId, string descripcion,
        DateOnly fechaInicio, DateOnly fechaFin, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var s = new SorteoLista
        {
            SucursalId  = sucursalId,
            TipoId      = tipoId,
            Descripcion = descripcion.Trim(),
            FechaInicio = fechaInicio,
            FechaFin    = fechaFin,
            Activa      = true
        };
        s.SetCreated(userId);
        return s;
    }

    public void Actualizar(string descripcion, DateOnly fechaInicio, DateOnly fechaFin, long? userId)
    {
        Descripcion = descripcion.Trim();
        FechaInicio = fechaInicio;
        FechaFin    = fechaFin;
        SetUpdated(userId);
    }

    public void Cerrar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
}
