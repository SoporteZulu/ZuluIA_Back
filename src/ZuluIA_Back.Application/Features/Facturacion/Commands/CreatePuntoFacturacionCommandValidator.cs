using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreatePuntoFacturacionCommandValidator
    : AbstractValidator<CreatePuntoFacturacionCommand>
{
    public CreatePuntoFacturacionCommandValidator()
    {
        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.TipoId)
            .GreaterThan(0).WithMessage("El tipo de punto de facturación es obligatorio.");

        RuleFor(x => x.Numero)
            .GreaterThan((short)0).WithMessage("El número debe ser mayor a 0.")
            .LessThanOrEqualTo((short)9999).WithMessage("El número no puede superar 9999.");
    }
}