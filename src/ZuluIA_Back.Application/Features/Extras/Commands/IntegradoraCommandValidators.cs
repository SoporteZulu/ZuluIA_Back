using FluentValidation;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateIntegradoraCommandValidator : AbstractValidator<CreateIntegradoraCommand>
{
    public CreateIntegradoraCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.TipoSistema).NotEmpty();
    }
}

public class UpdateIntegradoraCommandValidator : AbstractValidator<UpdateIntegradoraCommand>
{
    public UpdateIntegradoraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.TipoSistema).NotEmpty();
    }
}

public class RotateIntegradoraApiKeyCommandValidator : AbstractValidator<RotateIntegradoraApiKeyCommand>
{
    public RotateIntegradoraApiKeyCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NuevaApiKey).NotEmpty();
    }
}

public class DeactivateIntegradoraCommandValidator : AbstractValidator<DeactivateIntegradoraCommand>
{
    public DeactivateIntegradoraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateIntegradoraCommandValidator : AbstractValidator<ActivateIntegradoraCommand>
{
    public ActivateIntegradoraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}