using FluentValidation;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class CreateUnidadMedidaCommandValidator : AbstractValidator<CreateUnidadMedidaCommand>
{
    public CreateUnidadMedidaCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateUnidadMedidaCommandValidator : AbstractValidator<UpdateUnidadMedidaCommand>
{
    public UpdateUnidadMedidaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class ActivateUnidadMedidaCommandValidator : AbstractValidator<ActivateUnidadMedidaCommand>
{
    public ActivateUnidadMedidaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateUnidadMedidaCommandValidator : AbstractValidator<DeactivateUnidadMedidaCommand>
{
    public DeactivateUnidadMedidaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}