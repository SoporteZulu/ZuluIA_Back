using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

/// <summary>
/// Validaciones de formato y presencia de campos.
/// Las validaciones de unicidad (legajo/doc duplicado) se hacen
/// en el Handler porque requieren acceso a la BD.
/// Equivalente al validarDatos() de la capa de presentación del VB6.
/// </summary>
public class CreateTerceroCommandValidator
    : AbstractValidator<CreateTerceroCommand>
{
    public CreateTerceroCommandValidator()
    {
        // ─── Identificación ───────────────────────────────────────────────────
        RuleFor(x => x.Legajo)
            .NotEmpty().WithMessage("El legajo es obligatorio.")
            .MaximumLength(20).WithMessage("El legajo no puede superar los 20 caracteres.")
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("El legajo solo puede contener letras, números y guiones.");

        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es obligatoria.")
            .MaximumLength(200).WithMessage("La razón social no puede superar los 200 caracteres.");

        RuleFor(x => x.NombreFantasia)
            .MaximumLength(200)
            .When(x => x.NombreFantasia is not null)
            .WithMessage("El nombre de fantasía no puede superar los 200 caracteres.");

        // ─── Documento e IVA ──────────────────────────────────────────────────
        RuleFor(x => x.TipoDocumentoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un tipo de documento.");

        RuleFor(x => x.NroDocumento)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(30).WithMessage("El documento no puede superar los 30 caracteres.");

        RuleFor(x => x.CondicionIvaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una condición de IVA.");

        // ─── Roles ────────────────────────────────────────────────────────────
        RuleFor(x => x)
            .Must(x => x.EsCliente || x.EsProveedor || x.EsEmpleado)
            .WithName("Roles")
            .WithMessage("El tercero debe ser al menos cliente, proveedor o empleado.");

        // ─── Contacto ─────────────────────────────────────────────────────────
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El formato del email no es válido.")
            .MaximumLength(150).When(x => x.Email is not null)
            .WithMessage("El email no puede superar los 150 caracteres.");

        RuleFor(x => x.Telefono)
            .MaximumLength(30).When(x => x.Telefono is not null)
            .WithMessage("El teléfono no puede superar los 30 caracteres.");

        RuleFor(x => x.Celular)
            .MaximumLength(30).When(x => x.Celular is not null)
            .WithMessage("El celular no puede superar los 30 caracteres.");

        RuleFor(x => x.Web)
            .MaximumLength(150).When(x => x.Web is not null)
            .WithMessage("La web no puede superar los 150 caracteres.");

        // ─── Domicilio ────────────────────────────────────────────────────────
        RuleFor(x => x.Calle)
            .MaximumLength(150).When(x => x.Calle is not null)
            .WithMessage("La calle no puede superar los 150 caracteres.");

        RuleFor(x => x.CodigoPostal)
            .MaximumLength(10).When(x => x.CodigoPostal is not null)
            .WithMessage("El código postal no puede superar los 10 caracteres.");

        // ─── Comercial ────────────────────────────────────────────────────────
        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0).When(x => x.LimiteCredito.HasValue)
            .WithMessage("El límite de crédito no puede ser negativo.");

        RuleFor(x => x.PctComisionCobrador)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de comisión del cobrador debe estar entre 0 y 100.");

        RuleFor(x => x.PctComisionVendedor)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de comisión del vendedor debe estar entre 0 y 100.");

        // ─── Datos fiscales ───────────────────────────────────────────────────
        RuleFor(x => x.NroIngresosBrutos)
            .MaximumLength(30).When(x => x.NroIngresosBrutos is not null)
            .WithMessage("El nro. de ingresos brutos no puede superar los 30 caracteres.");

        RuleFor(x => x.NroMunicipal)
            .MaximumLength(30).When(x => x.NroMunicipal is not null)
            .WithMessage("El nro. municipal no puede superar los 30 caracteres.");
    }
}