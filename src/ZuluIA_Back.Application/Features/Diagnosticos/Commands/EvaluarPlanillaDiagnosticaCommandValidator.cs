using FluentValidation;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class EvaluarPlanillaDiagnosticaCommandValidator : AbstractValidator<EvaluarPlanillaDiagnosticaCommand>
{
    public EvaluarPlanillaDiagnosticaCommandValidator()
    {
        RuleFor(x => x.PlanillaId).GreaterThan(0);
        RuleFor(x => x.Respuestas).NotEmpty();
        RuleForEach(x => x.Respuestas).ChildRules(respuesta =>
        {
            respuesta.RuleFor(x => x.VariableId).GreaterThan(0);
        });
    }
}
