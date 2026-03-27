using FluentValidation;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class CreateJurisdiccionCommandValidator : AbstractValidator<CreateJurisdiccionCommand>
{
    public CreateJurisdiccionCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class UpdateJurisdiccionCommandValidator : AbstractValidator<UpdateJurisdiccionCommand>
{
    public UpdateJurisdiccionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class ActivateJurisdiccionCommandValidator : AbstractValidator<ActivateJurisdiccionCommand>
{
    public ActivateJurisdiccionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class DeactivateJurisdiccionCommandValidator : AbstractValidator<DeactivateJurisdiccionCommand>
{
    public DeactivateJurisdiccionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}