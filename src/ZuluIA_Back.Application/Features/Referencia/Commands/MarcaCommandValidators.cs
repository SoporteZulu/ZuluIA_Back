using FluentValidation;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class CreateMarcaCommandValidator : AbstractValidator<CreateMarcaCommand>
{
    public CreateMarcaCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class UpdateMarcaCommandValidator : AbstractValidator<UpdateMarcaCommand>
{
    public UpdateMarcaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class ActivateMarcaCommandValidator : AbstractValidator<ActivateMarcaCommand>
{
    public ActivateMarcaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class DeactivateMarcaCommandValidator : AbstractValidator<DeactivateMarcaCommand>
{
    public DeactivateMarcaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}