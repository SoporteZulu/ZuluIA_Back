using FluentValidation;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateTerceroCommandValidator
    : AbstractValidator<UpdateTerceroCommand>
{
    private static readonly string[] PersoneriasValidas = [
        TipoPersoneriaTercero.Juridica.ToString().ToUpperInvariant(),
        TipoPersoneriaTercero.Fisica.ToString().ToUpperInvariant()
    ];

    public UpdateTerceroCommandValidator()
    {
        // ─── Id ───────────────────────────────────────────────────────────────
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El Id del tercero no es válido.");

        // ─── Identificación ───────────────────────────────────────────────────
        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es obligatoria.")
            .MaximumLength(200).WithMessage("La razón social no puede superar los 200 caracteres.");

        RuleFor(x => x.NombreFantasia)
            .MaximumLength(200)
            .When(x => x.NombreFantasia is not null)
            .WithMessage("El nombre de fantasía no puede superar los 200 caracteres.");

        RuleFor(x => x.TipoPersoneria)
            .Must(x => string.IsNullOrWhiteSpace(x) || PersoneriasValidas.Contains(x.Trim().ToUpperInvariant()))
            .WithMessage("La personería debe ser JURIDICA o FISICA.");

        RuleFor(x => x.Nombre)
            .NotEmpty().When(IsFisica)
            .WithMessage("Debe ingresar el nombre para una persona física.")
            .MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Nombre))
            .WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Apellido)
            .NotEmpty().When(IsFisica)
            .WithMessage("Debe ingresar el apellido para una persona física.")
            .MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Apellido))
            .WithMessage("El apellido no puede superar los 150 caracteres.");

        RuleFor(x => x)
            .Must(x => !IsFisica(x) || !x.EsEntidadGubernamental)
            .WithMessage("Una persona física no puede marcarse como entidad gubernamental.");

        RuleFor(x => x.ClaveFiscal)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ClaveFiscal));

        RuleFor(x => x.ValorClaveFiscal)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.ValorClaveFiscal));

        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.ClaveFiscal) == string.IsNullOrWhiteSpace(x.ValorClaveFiscal))
            .WithMessage("Debe informar tanto la clave fiscal como su valor.");

        // ─── Documento e IVA ──────────────────────────────────────────────────
        RuleFor(x => x.NroDocumento)
            .MaximumLength(30)
            .When(x => x.NroDocumento is not null)
            .WithMessage("El documento no puede superar los 30 caracteres.");

        RuleFor(x => x.CondicionIvaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una condición de IVA.");

        // ─── Roles ────────────────────────────────────────────────────────────
        RuleFor(x => x)
            .Must(x => x.EsCliente || x.EsProveedor || x.EsEmpleado)
            .WithName("Roles")
            .WithMessage("El tercero debe tener al menos un rol activo (cliente, proveedor o empleado).");

        // ─── Contacto ─────────────────────────────────────────────────────────
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El formato del email no es válido.")
            .MaximumLength(150)
            .When(x => x.Email is not null)
            .WithMessage("El email no puede superar los 150 caracteres.");

        RuleFor(x => x.Telefono)
            .MaximumLength(30)
            .When(x => x.Telefono is not null)
            .WithMessage("El teléfono no puede superar los 30 caracteres.");

        RuleFor(x => x.Celular)
            .MaximumLength(30)
            .When(x => x.Celular is not null)
            .WithMessage("El celular no puede superar los 30 caracteres.");

        RuleFor(x => x.Web)
            .MaximumLength(150)
            .When(x => x.Web is not null)
            .WithMessage("La web no puede superar los 150 caracteres.");

        // ─── Domicilio ────────────────────────────────────────────────────────
        RuleFor(x => x.Calle)
            .MaximumLength(150)
            .When(x => x.Calle is not null)
            .WithMessage("La calle no puede superar los 150 caracteres.");

        RuleFor(x => x.CodigoPostal)
            .MaximumLength(10)
            .When(x => x.CodigoPostal is not null)
            .WithMessage("El código postal no puede superar los 10 caracteres.");

        // ─── Comercial ────────────────────────────────────────────────────────
        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LimiteCredito.HasValue)
            .WithMessage("El límite de crédito no puede ser negativo.");

        RuleFor(x => x.PctComisionCobrador)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de comisión del cobrador debe estar entre 0 y 100.");

        RuleFor(x => x.PctComisionVendedor)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de comisión del vendedor debe estar entre 0 y 100.");

        // ─── Datos fiscales ───────────────────────────────────────────────────
        RuleFor(x => x.NroIngresosBrutos)
            .MaximumLength(30)
            .When(x => x.NroIngresosBrutos is not null)
            .WithMessage("El nro. de ingresos brutos no puede superar los 30 caracteres.");

        RuleFor(x => x.NroMunicipal)
            .MaximumLength(30)
            .When(x => x.NroMunicipal is not null)
            .WithMessage("El nro. municipal no puede superar los 30 caracteres.");
    }

    private static bool IsFisica(UpdateTerceroCommand command)
        => string.Equals(command.TipoPersoneria?.Trim(), TipoPersoneriaTercero.Fisica.ToString(), StringComparison.OrdinalIgnoreCase);
}