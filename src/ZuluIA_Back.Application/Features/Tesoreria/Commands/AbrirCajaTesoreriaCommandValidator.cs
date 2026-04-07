using FluentValidation;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class AbrirCajaTesoreriaCommandValidator : AbstractValidator<AbrirCajaTesoreriaCommand>
{
    public AbrirCajaTesoreriaCommandValidator()
    {
        RuleFor(x => x.CajaId).GreaterThan(0);
        RuleFor(x => x.SaldoInicial).GreaterThanOrEqualTo(0);
    }
}
