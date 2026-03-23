using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class UpdatePuntoFacturacionCommandValidator : AbstractValidator<UpdatePuntoFacturacionCommand>
{
    public UpdatePuntoFacturacionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.TipoId)
            .GreaterThan(0);

        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}