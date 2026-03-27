using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Diagnosticos;

public class PlantillaDiagnostica : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activa { get; private set; } = true;
    public string? Observacion { get; private set; }

    private readonly List<PlantillaDiagnosticaVariable> _variables = [];
    public IReadOnlyCollection<PlantillaDiagnosticaVariable> Variables => _variables.AsReadOnly();

    private PlantillaDiagnostica() { }

    public static PlantillaDiagnostica Crear(string codigo, string descripcion, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var plantilla = new PlantillaDiagnostica
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Observacion = observacion?.Trim(),
            Activa = true
        };

        plantilla.SetCreated(userId);
        return plantilla;
    }

    public void AgregarVariable(PlantillaDiagnosticaVariable variable)
    {
        if (_variables.Any(x => x.VariableId == variable.VariableId))
            throw new InvalidOperationException("La variable ya fue incorporada a la plantilla.");

        _variables.Add(variable);
        SetUpdated(null);
    }
}
