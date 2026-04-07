using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Documentos;

/// <summary>
/// Documento ingresado en Mesa de Entrada para seguimiento y derivación interna.
/// Migrado desde VB6: MesaEntrada, MesaEntradaEstados, MesaEntradaTipos.
/// </summary>
public class MesaEntrada : AuditableEntity
{
    public long           SucursalId    { get; private set; }
    public long           TipoId        { get; private set; }
    public long?          TerceroId     { get; private set; }    // remitente externo
    public long?          EstadoId      { get; private set; }
    public string         NroDocumento  { get; private set; } = string.Empty;
    public string         Asunto        { get; private set; } = string.Empty;
    public DateOnly       FechaIngreso  { get; private set; }
    public DateOnly?      FechaVencimiento { get; private set; }
    public string?        Observacion   { get; private set; }
    public long?          AsignadoA     { get; private set; }   // UsuarioId responsable
    public EstadoMesaEntrada EstadoFlow { get; private set; }

    private MesaEntrada() { }

    public static MesaEntrada Crear(
        long sucursalId, long tipoId, long? terceroId,
        string nroDocumento, string asunto, DateOnly fechaIngreso,
        DateOnly? fechaVencimiento, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroDocumento);
        ArgumentException.ThrowIfNullOrWhiteSpace(asunto);
        var m = new MesaEntrada
        {
            SucursalId       = sucursalId,
            TipoId           = tipoId,
            TerceroId        = terceroId,
            NroDocumento     = nroDocumento.Trim(),
            Asunto           = asunto.Trim(),
            FechaIngreso     = fechaIngreso,
            FechaVencimiento = fechaVencimiento,
            Observacion      = observacion?.Trim(),
            EstadoFlow       = EstadoMesaEntrada.Pendiente
        };
        m.SetCreated(userId);
        return m;
    }

    public void AsignarResponsable(long usuarioId, long? userId) { AsignadoA = usuarioId; SetUpdated(userId); }
    public void CambiarEstado(long estadoId, EstadoMesaEntrada flujo, string? obs, long? userId)
    {
        EstadoId   = estadoId;
        EstadoFlow = flujo;
        if (obs != null) Observacion = obs.Trim();
        SetUpdated(userId);
    }
    public void Archivar(long? userId)  { EstadoFlow = EstadoMesaEntrada.Archivado; SetDeleted(); SetUpdated(userId); }
    public void Anular(long? userId)    { EstadoFlow = EstadoMesaEntrada.Anulado;   SetDeleted(); SetUpdated(userId); }
}
