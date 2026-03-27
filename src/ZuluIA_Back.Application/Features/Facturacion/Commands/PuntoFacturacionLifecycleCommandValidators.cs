using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class DeletePuntoFacturacionCommandValidator : AbstractValidator<DeletePuntoFacturacionCommand>
{
    public DeletePuntoFacturacionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class ActivatePuntoFacturacionCommandValidator : AbstractValidator<ActivatePuntoFacturacionCommand>
{
    public ActivatePuntoFacturacionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}