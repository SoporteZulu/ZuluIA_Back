using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class UpdateCajaCommandValidator : AbstractValidator<UpdateCajaCommand>
{
    public UpdateCajaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El Id de la caja es obligatorio.");
        RuleFor(x => x.Descripcion).NotEmpty().WithMessage("La descripción es obligatoria.").MaximumLength(200);
        RuleFor(x => x.TipoId).GreaterThan(0).WithMessage("El tipo de caja es obligatorio.");
        RuleFor(x => x.MonedaId).GreaterThan(0).WithMessage("La moneda es obligatoria.");
    }
}
