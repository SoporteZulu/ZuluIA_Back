using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class CancelarContratoCommandValidator : AbstractValidator<CancelarContratoCommand>
{
    public CancelarContratoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
