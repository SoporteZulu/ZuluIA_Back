using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Diagnosticos;

public class PlanillaDiagnostica : AuditableEntity
{
    public long PlantillaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public DateOnly Fecha { get; private set; }
    public decimal? ResultadoTotal { get; private set; }
    public EstadoPlanillaDiagnostica Estado { get; private set; }
    public string? Observacion { get; private set; }

    private readonly List<PlanillaDiagnosticaRespuesta> _respuestas = [];
    public IReadOnlyCollection<PlanillaDiagnosticaRespuesta> Respuestas => _respuestas.AsReadOnly();

    private PlanillaDiagnostica() { }

    public static PlanillaDiagnostica Crear(long plantillaId, string nombre, DateOnly fecha, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        var planilla = new PlanillaDiagnostica
        {
            PlantillaId = plantillaId,
            Nombre = nombre.Trim(),
            Fecha = fecha,
            Estado = EstadoPlanillaDiagnostica.Borrador,
            Observacion = observacion?.Trim()
        };

        planilla.SetCreated(userId);
        return planilla;
    }

    public void RegistrarRespuesta(PlanillaDiagnosticaRespuesta respuesta)
    {
        var existente = _respuestas.FirstOrDefault(x => x.VariableId == respuesta.VariableId);
        if (existente is not null)
            _respuestas.Remove(existente);

        _respuestas.Add(respuesta);
        SetUpdated(null);
    }

    public void Evaluar(decimal resultadoTotal, string? observacion, long? userId)
    {
        if (resultadoTotal < 0)
            throw new InvalidOperationException("El resultado total no puede ser negativo.");

        ResultadoTotal = resultadoTotal;
        Estado = EstadoPlanillaDiagnostica.Evaluada;
        if (!string.IsNullOrWhiteSpace(observacion))
            Observacion = observacion.Trim();
        SetUpdated(userId);
    }
}
