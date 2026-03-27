using FluentValidation;

namespace ZuluIA_Back.Application.Features.Recibos.Commands;

public class AnularReciboCommandValidator : AbstractValidator<AnularReciboCommand>
{
    public AnularReciboCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}