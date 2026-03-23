using FluentValidation;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;

public class UpdateDescuentoComercialCommandValidator : AbstractValidator<UpdateDescuentoComercialCommand>
{
    public UpdateDescuentoComercialCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El Id del descuento es obligatorio.");
        RuleFor(x => x.FechaDesde).NotEmpty().WithMessage("La fecha desde es obligatoria.");
        RuleFor(x => x.Porcentaje).InclusiveBetween(0, 100).WithMessage("El porcentaje debe estar entre 0 y 100.");
        RuleFor(x => x).Must(x => x.FechaHasta == null || x.FechaHasta >= x.FechaDesde)
            .WithMessage("La fecha hasta debe ser mayor o igual a la fecha desde.");
    }
}
