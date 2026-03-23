using FluentValidation;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class IniciarOrdenTrabajoCommandValidator : AbstractValidator<IniciarOrdenTrabajoCommand>
{
    public IniciarOrdenTrabajoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CancelarOrdenTrabajoCommandValidator : AbstractValidator<CancelarOrdenTrabajoCommand>
{
    public CancelarOrdenTrabajoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
