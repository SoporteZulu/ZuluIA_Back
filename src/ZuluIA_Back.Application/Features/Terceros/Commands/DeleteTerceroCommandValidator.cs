using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class DeleteTerceroCommandValidator
    : AbstractValidator<DeleteTerceroCommand>
{
    public DeleteTerceroCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El Id del tercero no es válido.");
    }
}

public class ActivarTerceroCommandValidator
    : AbstractValidator<ActivarTerceroCommand>
{
    public ActivarTerceroCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El Id del tercero no es válido.");
    }
}