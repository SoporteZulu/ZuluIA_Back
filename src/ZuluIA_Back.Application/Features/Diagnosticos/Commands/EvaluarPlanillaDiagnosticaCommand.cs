using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public record RespuestaDiagnosticaInput(long VariableId, long? OpcionId, string? ValorTexto, decimal? ValorNumerico);

public record EvaluarPlanillaDiagnosticaCommand(
    long PlanillaId,
    IReadOnlyList<RespuestaDiagnosticaInput> Respuestas,
    string? Observacion
) : IRequest<Result<decimal>>;
