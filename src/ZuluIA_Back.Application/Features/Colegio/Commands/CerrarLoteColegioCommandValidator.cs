using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CerrarLoteColegioCommandValidator : AbstractValidator<CerrarLoteColegioCommand>
{
    public CerrarLoteColegioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
