using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.BI;

/// <summary>
/// Cubo de análisis de datos (OLAP/pivot) definido por el usuario.
/// Migrado desde VB6: clsCubo / BI_CUBO.
/// Los campos y filtros del cubo se encuentran en CuboCampo y CuboFiltro.
/// </summary>
public class Cubo : AuditableEntity
{
    public string  Descripcion    { get; private set; } = "";  // cub_descripcion
    public long?   MenuCuboId     { get; private set; }        // mcub_id
    public string? OrigenDatos    { get; private set; }        // cub_origendatos (SQL)
    public string? Observacion    { get; private set; }        // cub_observacion
    public int     AmbienteId     { get; private set; } = 1;   // amb_id (1=sistema)
    public long?   CuboOrigenId   { get; private set; }        // cub_id_origen (copia)
    public bool    EsSistema      { get; private set; }        // sistema (1=sistema, 0=usuario)
    public long?   FormatoId      { get; private set; }        // for_id
    public long?   UsuarioAltaId  { get; private set; }        // usuario_alta

    private Cubo() { }

    public static Cubo Crear(
        string descripcion,
        string? origenDatos         = null,
        string? observacion         = null,
        int ambienteId              = 1,
        long? menuCuboId            = null,
        long? cuboOrigenId          = null,
        bool esSistema              = false,
        long? formatoId             = null,
        long? usuarioAltaId         = null)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("La descripción es requerida.");
        return new Cubo
        {
            Descripcion   = descripcion.Trim(),
            OrigenDatos   = origenDatos,
            Observacion   = observacion,
            AmbienteId    = ambienteId,
            MenuCuboId    = menuCuboId,
            CuboOrigenId  = cuboOrigenId,
            EsSistema     = esSistema,
            FormatoId     = formatoId,
            UsuarioAltaId = usuarioAltaId
        };
    }

    public void Actualizar(
        string descripcion,
        string? origenDatos,
        string? observacion,
        int ambienteId,
        long? menuCuboId,
        long? formatoId)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("La descripción es requerida.");
        Descripcion = descripcion.Trim();
        OrigenDatos = origenDatos;
        Observacion = observacion;
        AmbienteId  = ambienteId;
        MenuCuboId  = menuCuboId;
        FormatoId   = formatoId;
    }
}
