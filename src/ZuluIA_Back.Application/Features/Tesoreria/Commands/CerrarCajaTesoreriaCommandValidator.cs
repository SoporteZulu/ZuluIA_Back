using FluentValidation;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class CerrarCajaTesoreriaCommandValidator : AbstractValidator<CerrarCajaTesoreriaCommand>
{
    public CerrarCajaTesoreriaCommandValidator()
    {
        RuleFor(x => x.CajaId).GreaterThan(0);
        RuleFor(x => x.SaldoInformado).GreaterThanOrEqualTo(0);
    }
}
