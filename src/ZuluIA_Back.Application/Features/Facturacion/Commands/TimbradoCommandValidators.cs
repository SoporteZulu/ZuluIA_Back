using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreateTimbradoCommandValidator : AbstractValidator<CreateTimbradoCommand>
{
    public CreateTimbradoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.NroTimbrado).NotEmpty();
    }
}

public class UpdateTimbradoCommandValidator : AbstractValidator<UpdateTimbradoCommand>
{
    public UpdateTimbradoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateTimbradoCommandValidator : AbstractValidator<ActivateTimbradoCommand>
{
    public ActivateTimbradoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateTimbradoCommandValidator : AbstractValidator<DeactivateTimbradoCommand>
{
    public DeactivateTimbradoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}