using FluentValidation;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreatePlanTarjetaCommandValidator : AbstractValidator<CreatePlanTarjetaCommand>
{
    public CreatePlanTarjetaCommandValidator()
    {
        RuleFor(x => x.TarjetaTipoId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.CantidadCuotas).GreaterThan(0);
        RuleFor(x => x.Recargo).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePlanTarjetaCommandValidator : AbstractValidator<UpdatePlanTarjetaCommand>
{
    public UpdatePlanTarjetaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.CantidadCuotas).GreaterThan(0);
        RuleFor(x => x.Recargo).GreaterThanOrEqualTo(0);
    }
}

public class ActivatePlanTarjetaCommandValidator : AbstractValidator<ActivatePlanTarjetaCommand>
{
    public ActivatePlanTarjetaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivatePlanTarjetaCommandValidator : AbstractValidator<DeactivatePlanTarjetaCommand>
{
    public DeactivatePlanTarjetaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateTarjetaTipoCommandValidator : AbstractValidator<CreateTarjetaTipoCommand>
{
    public CreateTarjetaTipoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateTarjetaTipoCommandValidator : AbstractValidator<UpdateTarjetaTipoCommand>
{
    public UpdateTarjetaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class ActivateTarjetaTipoCommandValidator : AbstractValidator<ActivateTarjetaTipoCommand>
{
    public ActivateTarjetaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateTarjetaTipoCommandValidator : AbstractValidator<DeactivateTarjetaTipoCommand>
{
    public DeactivateTarjetaTipoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
