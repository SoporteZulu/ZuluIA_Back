using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class ConciliarDeuceOperacionCommandValidator : AbstractValidator<ConciliarDeuceOperacionCommand>
{
    public ConciliarDeuceOperacionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
