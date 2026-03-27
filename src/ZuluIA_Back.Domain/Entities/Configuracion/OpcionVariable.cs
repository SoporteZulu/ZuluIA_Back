using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Opción de valor predefinido para una variable dinámica (lista de selección).
/// Migrado desde VB6: clsOpcionVariable / FRA_OPCIONESVARIABLE.
/// </summary>
public class OpcionVariable : BaseEntity
{
    public string  Codigo       { get; private set; } = string.Empty;
    public string  Descripcion  { get; private set; } = string.Empty;
    public string? Observaciones{ get; private set; }

    private OpcionVariable() { }

    public static OpcionVariable Crear(string codigo, string descripcion, string? observaciones = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new OpcionVariable
        {
            Codigo        = codigo.Trim().ToUpperInvariant(),
            Descripcion   = descripcion.Trim(),
            Observaciones = observaciones?.Trim()
        };
    }

    public void Actualizar(string codigo, string descripcion, string? observaciones)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Codigo        = codigo.Trim().ToUpperInvariant();
        Descripcion   = descripcion.Trim();
        Observaciones = observaciones?.Trim();
    }
}
