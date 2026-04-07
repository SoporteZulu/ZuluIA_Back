using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class EmitirRemitosVentaMasivosCommandValidator : AbstractValidator<EmitirRemitosVentaMasivosCommand>
{
    public EmitirRemitosVentaMasivosCommandValidator()
    {
        RuleFor(x => x.ComprobanteIds)
            .NotEmpty()
            .WithMessage("Debe indicar al menos un remito.");

        RuleForEach(x => x.ComprobanteIds).GreaterThan(0);
    }
}
