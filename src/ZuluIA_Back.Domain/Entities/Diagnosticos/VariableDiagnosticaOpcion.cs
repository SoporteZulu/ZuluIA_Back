using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

public class VariableDiagnosticaOpcion : BaseEntity
{
    public long VariableId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal ValorNumerico { get; private set; }
    public short Orden { get; private set; }

    private VariableDiagnosticaOpcion() { }

    public static VariableDiagnosticaOpcion Crear(long variableId, string codigo, string descripcion, decimal valorNumerico, short orden)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new VariableDiagnosticaOpcion
        {
            VariableId = variableId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            ValorNumerico = valorNumerico,
            Orden = orden
        };
    }
}
