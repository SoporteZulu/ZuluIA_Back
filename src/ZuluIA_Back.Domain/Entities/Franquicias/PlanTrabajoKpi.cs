using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Franquicias;

public class PlanTrabajoKpi : BaseEntity
{
    public long PlanTrabajoId { get; private set; }
    public long? AspectoId { get; private set; }
    public long? VariableId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal ValorObjetivo { get; private set; }
    public decimal Peso { get; private set; }

    private PlanTrabajoKpi() { }

    public static PlanTrabajoKpi Crear(
        long planTrabajoId,
        long? aspectoId,
        long? variableId,
        string descripcion,
        decimal valorObjetivo,
        decimal peso)
    {
        return new PlanTrabajoKpi
        {
            PlanTrabajoId = planTrabajoId,
            AspectoId     = aspectoId,
            VariableId    = variableId,
            Descripcion   = descripcion.Trim(),
            ValorObjetivo = valorObjetivo,
            Peso          = peso
        };
    }
}
