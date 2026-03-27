using FluentValidation;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class IniciarOrdenPreparacionCommandValidator : AbstractValidator<IniciarOrdenPreparacionCommand>
{
    public IniciarOrdenPreparacionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
