using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateTerceroCommandValidator : AbstractValidator<CreateTerceroCommand>
{
    public CreateTerceroCommandValidator()
    {
        RuleFor(x => x.Legajo)
            .NotEmpty().WithMessage("El legajo es obligatorio.")
            .MaximumLength(20).WithMessage("El legajo no puede superar los 20 caracteres.");

        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es obligatoria.")
            .MaximumLength(200).WithMessage("La razón social no puede superar los 200 caracteres.");

        RuleFor(x => x.NroDocumento)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(30).WithMessage("El número de documento no puede superar los 30 caracteres.");

        RuleFor(x => x.TipoDocumentoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un tipo de documento.");

        RuleFor(x => x.CondicionIvaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una condición de IVA.");

        RuleFor(x => x)
            .Must(x => x.EsCliente || x.EsProveedor || x.EsEmpleado())
            .WithMessage("El tercero debe ser al menos cliente, proveedor o empleado.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0).WithMessage("El límite de crédito no puede ser negativo.")
            .When(x => x.LimiteCredito.HasValue);

        RuleFor(x => x.NombreFantasia)
            .MaximumLength(200).WithMessage("El nombre fantasía no puede superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.NombreFantasia));
    }
}

file static class CreateTerceroCommandExtensions
{
    public static bool EsEmpleado(this CreateTerceroCommand cmd) => false;
}