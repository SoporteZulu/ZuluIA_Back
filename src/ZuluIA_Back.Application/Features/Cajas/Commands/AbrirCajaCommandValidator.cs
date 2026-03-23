using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class AbrirCajaCommandValidator : AbstractValidator<AbrirCajaCommand>
{
    public AbrirCajaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.SaldoInicial)
            .GreaterThanOrEqualTo(0m);
    }
}