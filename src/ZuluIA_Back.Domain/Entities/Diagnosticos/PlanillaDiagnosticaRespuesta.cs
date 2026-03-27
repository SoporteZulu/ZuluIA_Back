using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Diagnosticos;

public class PlanillaDiagnosticaRespuesta : BaseEntity
{
    public long PlanillaId { get; private set; }
    public long VariableId { get; private set; }
    public long? OpcionId { get; private set; }
    public string? ValorTexto { get; private set; }
    public decimal? ValorNumerico { get; private set; }
    public decimal PuntajeObtenido { get; private set; }

    private PlanillaDiagnosticaRespuesta() { }

    public static PlanillaDiagnosticaRespuesta Registrar(long planillaId, long variableId, long? opcionId, string? valorTexto, decimal? valorNumerico, decimal puntajeObtenido)
    {
        return new PlanillaDiagnosticaRespuesta
        {
            PlanillaId = planillaId,
            VariableId = variableId,
            OpcionId = opcionId,
            ValorTexto = valorTexto?.Trim(),
            ValorNumerico = valorNumerico,
            PuntajeObtenido = puntajeObtenido
        };
    }
}
