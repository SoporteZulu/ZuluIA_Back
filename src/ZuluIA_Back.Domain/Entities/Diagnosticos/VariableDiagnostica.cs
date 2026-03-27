using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Diagnosticos;

public class VariableDiagnostica : AuditableEntity
{
    public long AspectoId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public TipoVariableDiagnostica Tipo { get; private set; }
    public bool Requerida { get; private set; }
    public decimal Peso { get; private set; }
    public bool Activa { get; private set; } = true;

    private readonly List<VariableDiagnosticaOpcion> _opciones = [];
    public IReadOnlyCollection<VariableDiagnosticaOpcion> Opciones => _opciones.AsReadOnly();

    private VariableDiagnostica() { }

    public static VariableDiagnostica Crear(long aspectoId, string codigo, string descripcion, TipoVariableDiagnostica tipo, bool requerida, decimal peso, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (peso <= 0)
            throw new InvalidOperationException("El peso de la variable debe ser mayor a 0.");

        var variable = new VariableDiagnostica
        {
            AspectoId = aspectoId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Tipo = tipo,
            Requerida = requerida,
            Peso = peso,
            Activa = true
        };

        variable.SetCreated(userId);
        return variable;
    }

    public void AgregarOpcion(VariableDiagnosticaOpcion opcion)
    {
        if (Tipo != TipoVariableDiagnostica.Opcion)
            throw new InvalidOperationException("Solo las variables de opción admiten opciones.");

        if (_opciones.Any(x => x.Codigo == opcion.Codigo))
            throw new InvalidOperationException($"Ya existe una opción con código '{opcion.Codigo}'.");

        _opciones.Add(opcion);
        SetUpdated(null);
    }
}
