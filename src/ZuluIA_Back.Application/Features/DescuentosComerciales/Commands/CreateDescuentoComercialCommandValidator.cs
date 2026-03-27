using FluentValidation;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;

public class CreateDescuentoComercialCommandValidator : AbstractValidator<CreateDescuentoComercialCommand>
{
    public CreateDescuentoComercialCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0).WithMessage("El tercero es obligatorio.");
        RuleFor(x => x.ItemId).GreaterThan(0).WithMessage("El ítem es obligatorio.");
        RuleFor(x => x.FechaDesde).NotEmpty().WithMessage("La fecha desde es obligatoria.");
        RuleFor(x => x.Porcentaje).InclusiveBetween(0, 100).WithMessage("El porcentaje debe estar entre 0 y 100.");
        RuleFor(x => x).Must(x => x.FechaHasta == null || x.FechaHasta >= x.FechaDesde)
            .WithMessage("La fecha hasta debe ser mayor o igual a la fecha desde.");
    }
}
