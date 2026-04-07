using FluentValidation;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class PagarCedulonCommandValidator : AbstractValidator<PagarCedulonCommand>
{
    public PagarCedulonCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Importe)
            .GreaterThan(0m);
    }
}

public class CreateCedulonCommandValidator : AbstractValidator<CreateCedulonCommand>
{
    public CreateCedulonCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.NroCedulon).NotEmpty();
        RuleFor(x => x.Importe).GreaterThan(0m);
    }
}