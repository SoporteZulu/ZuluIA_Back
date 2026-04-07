using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateTerceroCommandValidatorTests
{
    private readonly CreateTerceroCommandValidator _validator = new();

    private static CreateTerceroCommand ComandoValido() => new(
        Legajo: "CLI001",
        RazonSocial: "Empresa SA",
        NombreFantasia: null,
        TipoPersoneria: null,
        Nombre: null,
        Apellido: null,
        Tratamiento: null,
        Profesion: null,
        EstadoPersonaId: null,
        EstadoCivilId: null,
        EstadoCivil: null,
        Nacionalidad: null,
        Sexo: null,
        FechaNacimiento: null,
        FechaRegistro: null,
        EsEntidadGubernamental: false,
        ClaveFiscal: null,
        ValorClaveFiscal: null,
        TipoDocumentoId: 1,
        NroDocumento: "30-11111111-1",
        CondicionIvaId: 1,
        EsCliente: true,
        EsProveedor: false,
        EsEmpleado: false,
        Calle: null,
        Nro: null,
        Piso: null,
        Dpto: null,
        CodigoPostal: null,
        PaisId: null,
        ProvinciaId: null,
        LocalidadId: null,
        BarrioId: null,
        NroIngresosBrutos: null,
        NroMunicipal: null,
        Telefono: null,
        Celular: null,
        Email: null,
        Web: null,
        MonedaId: null,
        CategoriaId: null,
        CategoriaClienteId: null,
        EstadoClienteId: null,
        CategoriaProveedorId: null,
        EstadoProveedorId: null,
        LimiteCredito: null,
        PorcentajeMaximoDescuento: null,
        VigenciaCreditoDesde: null,
        VigenciaCreditoHasta: null,
        Facturable: false,
        CobradorId: null,
        AplicaComisionCobrador: false,
        PctComisionCobrador: 0,
        VendedorId: null,
        AplicaComisionVendedor: false,
        PctComisionVendedor: 0,
        Observacion: null,
        SucursalId: null,
        PerfilComercial: null,
        Domicilios: null,
        Contactos: null,
        SucursalesEntrega: null,
        Transportes: null,
        VentanasCobranza: null
    );

    [Fact]
    public void Validar_ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_LegajoVacio_NoDebeHaveError()
    {
        var cmd = ComandoValido() with { Legajo = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Legajo);
    }

    [Fact]
    public void Validar_FechaRegistroFutura_DebeTenerError()
    {
        var cmd = ComandoValido() with { FechaRegistro = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FechaRegistro);
    }

    [Fact]
    public void Validar_LegajoMuyLargo_DebeHaveError()
    {
        var cmd = ComandoValido() with { Legajo = new string('A', 21) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Legajo);
    }

    [Fact]
    public void Validar_RazonSocialVacia_DebeHaveError()
    {
        var cmd = ComandoValido() with { RazonSocial = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.RazonSocial);
    }

    [Fact]
    public void Validar_EmailInvalido_DebeHaveError()
    {
        var cmd = ComandoValido() with { Email = "no-es-email" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validar_EmailValido_NoDebeHaveError()
    {
        var cmd = ComandoValido() with { Email = "test@empresa.com" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validar_LimiteCreditoNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with { LimiteCredito = -100m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.LimiteCredito);
    }

    [Fact]
    public void Validar_PaisIdInvalido_DebeHaveError()
    {
        var cmd = ComandoValido() with { PaisId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PaisId);
    }

    [Fact]
    public void Validar_SexoInvalido_DebeHaveError()
    {
        var cmd = ComandoValido() with { TipoPersoneria = "FISICA", Nombre = "Juan", Apellido = "Perez", Sexo = "X" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Sexo);
    }

    [Fact]
    public void Validar_FechaNacimientoFutura_DebeHaveError()
    {
        var cmd = ComandoValido() with { FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FechaNacimiento);
    }

    [Fact]
    public void Validar_TipoDocumentoIdCero_NoDebeHaveError()
    {
        var cmd = ComandoValido() with { TipoDocumentoId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.TipoDocumentoId);
    }

    [Fact]
    public void Validar_ContactosConDosPrincipales_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Contactos =
            [
                new ReplaceTerceroContactoItem(null, "Ana", null, null, null, null, true, 0),
                new ReplaceTerceroContactoItem(null, "Juan", null, null, null, null, true, 1)
            ]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Contactos);
    }

    [Fact]
    public void Validar_PerfilComercialConRiesgoInvalido_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            PerfilComercial = new TerceroPerfilComercialPayload(
                ZonaComercialId: null,
                Rubro: "Mayorista",
                Subrubro: null,
                Sector: null,
                CondicionCobranza: null,
                RiesgoCrediticio: "CRITICO",
                SaldoMaximoVigente: null,
                VigenciaSaldo: null,
                VigenciaSaldoDesde: null,
                VigenciaSaldoHasta: null,
                CondicionVenta: null,
                PlazoCobro: null,
                FacturadorPorDefecto: null,
                MinimoFacturaMipymes: null,
                ObservacionComercial: null)
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("PerfilComercial.RiesgoCrediticio");
    }

    [Fact]
    public void Validar_CategoriaClienteSinRolCliente_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            EsCliente = false,
            EsProveedor = true,
            CategoriaClienteId = 2
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validar_PersonaFisicaSinNombreYApellido_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            TipoPersoneria = "FISICA",
            Nombre = null,
            Apellido = null
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Nombre);
        result.ShouldHaveValidationErrorFor(x => x.Apellido);
    }

    [Fact]
    public void Validar_PersonaFisicaEntidadGubernamental_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            TipoPersoneria = "FISICA",
            Nombre = "Juan",
            Apellido = "Perez",
            EsEntidadGubernamental = true
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validar_ClaveFiscalSinValor_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            ClaveFiscal = "AFIP",
            ValorClaveFiscal = null
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x);
    }
}