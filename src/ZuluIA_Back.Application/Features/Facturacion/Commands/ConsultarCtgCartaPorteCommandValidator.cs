using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ConsultarCtgCartaPorteCommandValidator : AbstractValidator<ConsultarCtgCartaPorteCommand>
{
    public ConsultarCtgCartaPorteCommandValidator()
    {
        RuleFor(x => x.CartaPorteId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.NroCtg) || !string.IsNullOrWhiteSpace(x.Error) || !string.IsNullOrWhiteSpace(x.Observacion))
            .WithMessage("Debe informar un CTG, un error o una observación de consulta.");
    }
}
