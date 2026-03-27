using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Variable dinámica del sistema (campo configurable para tipos de comprobante, etc.).
/// Permite extender los formularios con campos opcionales definidos en tiempo de ejecución.
/// Migrado desde VB6: clsVariables / FRA_VARIABLES.
/// </summary>
public class Variable : BaseEntity
{
    public long?   TipoVariableId   { get; private set; }
    public long?   TipoComprobanteId{ get; private set; }
    public string  Codigo           { get; private set; } = string.Empty;
    public string? CodigoEstructura { get; private set; }
    public string  Descripcion      { get; private set; } = string.Empty;
    public string? Observacion      { get; private set; }
    public long?   AspectoId        { get; private set; }
    public int     Nivel            { get; private set; }
    public int     Orden            { get; private set; }
    public string? Condicionante    { get; private set; }
    public bool    Editable         { get; private set; } = true;

    private Variable() { }

    public static Variable Crear(string codigo, string descripcion,
        long? tipoVariableId = null, long? tipoComprobanteId = null,
        long? aspectoId = null, int nivel = 0, int orden = 0,
        string? codigoEstructura = null, string? observacion = null,
        string? condicionante = null, bool editable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Variable
        {
            Codigo            = codigo.Trim().ToUpperInvariant(),
            Descripcion       = descripcion.Trim(),
            TipoVariableId    = tipoVariableId,
            TipoComprobanteId = tipoComprobanteId,
            AspectoId         = aspectoId,
            Nivel             = nivel,
            Orden             = orden,
            CodigoEstructura  = codigoEstructura?.Trim(),
            Observacion       = observacion?.Trim(),
            Condicionante     = condicionante?.Trim(),
            Editable          = editable
        };
    }

    public void Actualizar(string descripcion, long? tipoVariableId,
        long? tipoComprobanteId, long? aspectoId, int nivel, int orden,
        string? codigoEstructura, string? observacion, string? condicionante, bool editable)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion       = descripcion.Trim();
        TipoVariableId    = tipoVariableId;
        TipoComprobanteId = tipoComprobanteId;
        AspectoId         = aspectoId;
        Nivel             = nivel;
        Orden             = orden;
        CodigoEstructura  = codigoEstructura?.Trim();
        Observacion       = observacion?.Trim();
        Condicionante     = condicionante?.Trim();
        Editable          = editable;
    }
}
