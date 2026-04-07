using FluentValidation;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreateVariableDiagnosticaCommandValidator : AbstractValidator<CreateVariableDiagnosticaCommand>
{
    public CreateVariableDiagnosticaCommandValidator()
    {
        RuleFor(x => x.AspectoId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Peso).GreaterThan(0);

        RuleFor(x => x.Opciones)
            .NotEmpty()
            .When(x => x.Tipo == TipoVariableDiagnostica.Opcion)
            .WithMessage("Las variables de opción deben definir opciones.");
    }
}
