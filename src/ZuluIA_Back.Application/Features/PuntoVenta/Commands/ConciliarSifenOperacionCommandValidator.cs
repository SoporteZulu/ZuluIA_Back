using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class ConciliarSifenOperacionCommandValidator : AbstractValidator<ConciliarSifenOperacionCommand>
{
    public ConciliarSifenOperacionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
