using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class UpdateLoteColegioCommandValidator : AbstractValidator<UpdateLoteColegioCommand>
{
    public UpdateLoteColegioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FechaVencimiento).GreaterThanOrEqualTo(x => x.FechaEmision);
    }
}
