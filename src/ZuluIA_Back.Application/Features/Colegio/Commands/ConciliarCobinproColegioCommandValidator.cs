using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class ConciliarCobinproColegioCommandValidator : AbstractValidator<ConciliarCobinproColegioCommand>
{
    public ConciliarCobinproColegioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
