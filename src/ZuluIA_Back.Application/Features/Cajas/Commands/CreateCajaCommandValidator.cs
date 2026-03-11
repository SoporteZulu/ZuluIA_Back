using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class CreateCajaCommandValidator : AbstractValidator<CreateCajaCommand>
{
    public CreateCajaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0).WithMessage("La sucursal es obligatoria.");
        RuleFor(x => x.TipoId).GreaterThan(0).WithMessage("El tipo de caja es obligatorio.");
        RuleFor(x => x.Descripcion).NotEmpty().WithMessage("La descripción es obligatoria.").MaximumLength(200);
        RuleFor(x => x.MonedaId).GreaterThan(0).WithMessage("La moneda es obligatoria.");
    }
}