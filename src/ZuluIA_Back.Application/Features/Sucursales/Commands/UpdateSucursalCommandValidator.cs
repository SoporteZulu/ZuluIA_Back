using FluentValidation;

namespace ZuluIA_Back.Application.Features.Sucursales.Commands;

public class UpdateSucursalCommandValidator : AbstractValidator<UpdateSucursalCommand>
{
    public UpdateSucursalCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la sucursal es inválido.");

        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es obligatoria.")
            .MaximumLength(200);

        RuleFor(x => x.Cuit)
            .NotEmpty().WithMessage("El CUIT es obligatorio.")
            .MaximumLength(20);

        RuleFor(x => x.CondicionIvaId)
            .GreaterThan(0).WithMessage("La condición IVA es obligatoria.");

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.PaisId)
            .GreaterThan(0).WithMessage("El país es obligatorio.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.PuertoAfip)
            .InclusiveBetween((short)1, short.MaxValue)
            .WithMessage("El puerto AFIP debe estar entre 1 y 65535.");
    }
}