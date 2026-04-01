using FluentValidation;
using ZuluIA_Back.Domain.Enums;

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
    private static readonly string[] PersoneriasValidas = [
        TipoPersoneriaTercero.Juridica.ToString().ToUpperInvariant(),
        TipoPersoneriaTercero.Fisica.ToString().ToUpperInvariant()
    ];

    public CreateTerceroCommandValidator()
    {
        // ─── Identificación ───────────────────────────────────────────────────
        RuleFor(x => x.Legajo)
            .MaximumLength(20).WithMessage("El legajo no puede superar los 20 caracteres.")
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("El legajo solo puede contener letras, números y guiones.")
            .When(x => !string.IsNullOrWhiteSpace(x.Legajo));

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

        RuleFor(x => x.Tratamiento)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Tratamiento))
            .WithMessage("El tratamiento no puede superar los 250 caracteres.");

        RuleFor(x => x.Profesion)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Profesion))
            .WithMessage("La profesión no puede superar los 250 caracteres.");

        RuleFor(x => x.EstadoPersonaId)
            .GreaterThan(0).When(x => x.EstadoPersonaId.HasValue)
            .WithMessage("El estado general informado no es válido.");

        RuleFor(x => x.EstadoCivil)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.EstadoCivil))
            .WithMessage("El estado civil no puede superar los 250 caracteres.");

        RuleFor(x => x.EstadoCivilId)
            .GreaterThan(0).When(x => x.EstadoCivilId.HasValue)
            .WithMessage("El estado civil informado no es válido.");

        RuleFor(x => x.Nacionalidad)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Nacionalidad))
            .WithMessage("La nacionalidad no puede superar los 250 caracteres.");

        RuleFor(x => x.Sexo)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Trim().Equals("M", StringComparison.OrdinalIgnoreCase) || x.Trim().Equals("F", StringComparison.OrdinalIgnoreCase))
            .WithMessage("El sexo debe ser M o F.");

        RuleFor(x => x.FechaNacimiento)
            .Must(x => !x.HasValue || x.Value <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("La fecha de nacimiento no puede ser futura.");

        RuleFor(x => x.FechaRegistro)
            .Must(x => !x.HasValue || x.Value <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("La fecha de registro no puede ser futura.");

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
            .MaximumLength(30).WithMessage("El documento no puede superar los 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.NroDocumento));

        RuleFor(x => x.CondicionIvaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una condición de IVA.");

        // ─── Roles ────────────────────────────────────────────────────────────
        RuleFor(x => x)
            .Must(x => x.EsCliente || x.EsProveedor || x.EsEmpleado)
            .WithName("Roles")
            .WithMessage("El tercero debe ser al menos cliente, proveedor o empleado.");

        RuleFor(x => x)
            .Must(x => x.EsCliente || (!x.CategoriaClienteId.HasValue && !x.EstadoClienteId.HasValue))
            .WithMessage("No puede informar categoría/estado de cliente si el rol Cliente no está activo.");

        RuleFor(x => x)
            .Must(x => x.EsProveedor || (!x.CategoriaProveedorId.HasValue && !x.EstadoProveedorId.HasValue))
            .WithMessage("No puede informar categoría/estado de proveedor si el rol Proveedor no está activo.");

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

        RuleFor(x => x.PaisId)
            .GreaterThan(0).When(x => x.PaisId.HasValue)
            .WithMessage("El país informado no es válido.");

        RuleFor(x => x.ProvinciaId)
            .GreaterThan(0).When(x => x.ProvinciaId.HasValue)
            .WithMessage("La provincia informada no es válida.");

        RuleFor(x => x.LocalidadId)
            .GreaterThan(0).When(x => x.LocalidadId.HasValue)
            .WithMessage("La localidad informada no es válida.");

        RuleFor(x => x.BarrioId)
            .GreaterThan(0).When(x => x.BarrioId.HasValue)
            .WithMessage("El barrio informado no es válido.");

        RuleFor(x => x)
            .Must(x => !x.BarrioId.HasValue || x.LocalidadId.HasValue)
            .WithMessage("Debe informar la localidad cuando se indica un barrio.");

        // ─── Comercial ────────────────────────────────────────────────────────
        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0).When(x => x.LimiteCredito.HasValue)
            .WithMessage("El límite de crédito no puede ser negativo.");

        RuleFor(x => x.PorcentajeMaximoDescuento)
            .InclusiveBetween(0, 100).When(x => x.PorcentajeMaximoDescuento.HasValue)
            .WithMessage("El porcentaje máximo de descuento debe estar entre 0 y 100.");

        RuleFor(x => x)
            .Must(x => !x.VigenciaCreditoDesde.HasValue || !x.VigenciaCreditoHasta.HasValue || x.VigenciaCreditoDesde.Value <= x.VigenciaCreditoHasta.Value)
            .WithMessage("La vigencia desde del crédito no puede ser mayor que la vigencia hasta.");

        RuleFor(x => x)
            .Must(x => !x.AplicaComisionCobrador || x.CobradorId.HasValue)
            .WithMessage("Debe asignar un cobrador si la comisión del cobrador está activa.");

        RuleFor(x => x)
            .Must(x => !x.AplicaComisionVendedor || x.VendedorId.HasValue)
            .WithMessage("Debe asignar un vendedor si la comisión del vendedor está activa.");

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

        RuleFor(x => x.PerfilComercial)
            .SetValidator(new TerceroPerfilComercialPayloadValidator()!)
            .When(x => x.PerfilComercial is not null);

        RuleForEach(x => x.Domicilios!)
            .SetValidator(new ReplaceTerceroDomicilioItemValidator())
            .When(x => x.Domicilios is not null);

        RuleFor(x => x.Domicilios)
            .Must(items => items is null || items.Count(x => x.EsDefecto) <= 1)
            .WithMessage("Solo puede marcarse un domicilio por defecto.");

        RuleForEach(x => x.Contactos!)
            .SetValidator(new ReplaceTerceroContactoItemValidator())
            .When(x => x.Contactos is not null);

        RuleFor(x => x.Contactos)
            .Must(contactos => contactos is null || contactos.Count(c => c.Principal) <= 1)
            .WithMessage("Solo puede marcarse un contacto principal.");

        RuleForEach(x => x.SucursalesEntrega!)
            .SetValidator(new ReplaceTerceroSucursalEntregaItemValidator())
            .When(x => x.SucursalesEntrega is not null);

        RuleFor(x => x.SucursalesEntrega)
            .Must(items => items is null || items.Count(x => x.Principal) <= 1)
            .WithMessage("Solo puede marcarse una sucursal/punto de entrega principal.");

        RuleForEach(x => x.Transportes!)
            .SetValidator(new ReplaceTerceroTransporteItemValidator())
            .When(x => x.Transportes is not null);

        RuleFor(x => x.Transportes)
            .Must(items => items is null || items.Count(x => x.Principal) <= 1)
            .WithMessage("Solo puede marcarse un transporte principal o por defecto.");

        RuleForEach(x => x.VentanasCobranza!)
            .SetValidator(new ReplaceTerceroVentanaCobranzaItemValidator())
            .When(x => x.VentanasCobranza is not null);

        RuleFor(x => x.VentanasCobranza)
            .Must(items => items is null || items.Count(x => x.Principal) <= 1)
            .WithMessage("Solo puede marcarse una ventana de cobranza principal.");
    }

    private static bool IsFisica(CreateTerceroCommand command)
        => string.Equals(command.TipoPersoneria?.Trim(), TipoPersoneriaTercero.Fisica.ToString(), StringComparison.OrdinalIgnoreCase);
}