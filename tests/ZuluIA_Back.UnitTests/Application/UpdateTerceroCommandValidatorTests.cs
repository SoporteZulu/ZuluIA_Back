using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class UpdateTerceroCommandValidatorCoverageTests
{
    private readonly UpdateTerceroCommandValidator _validator = new();

    private static UpdateTerceroCommand ComandoValido() => new(
        Id: 10,
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
        Facturable: true,
        CobradorId: null,
        AplicaComisionCobrador: false,
        PctComisionCobrador: 0,
        VendedorId: null,
        AplicaComisionVendedor: false,
        PctComisionVendedor: 0,
        Observacion: null,
        PerfilComercial: null,
        Domicilios: null,
        Contactos: null,
        SucursalesEntrega: null,
        Transportes: null,
        VentanasCobranza: null);

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var result = _validator.TestValidate(ComandoValido());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SucursalesConDosPrincipales_DebeTenerError()
    {
        var cmd = ComandoValido() with
        {
            SucursalesEntrega =
            [
                new ReplaceTerceroSucursalEntregaItem(null, "Casa Central", null, null, null, null, null, true, 0),
                new ReplaceTerceroSucursalEntregaItem(null, "Depósito", null, null, null, null, null, true, 1)
            ]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.SucursalesEntrega);
    }

    [Fact]
    public void Validar_VentanaConOrdenNegativo_DebeTenerError()
    {
        var cmd = ComandoValido() with
        {
            VentanasCobranza =
            [
                new ReplaceTerceroVentanaCobranzaItem(null, "Lunes", null, null, null, false, -1)
            ]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("VentanasCobranza[0].Orden");
    }

    [Fact]
    public void Validar_EstadoProveedorSinRolProveedor_DebeTenerError()
    {
        var cmd = ComandoValido() with
        {
            EstadoProveedorId = 7
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validar_PersonaFisicaSinNombreYApellido_DebeTenerError()
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
    public void Validar_PersonaFisicaEntidadGubernamental_DebeTenerError()
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
    public void Validar_ClaveFiscalSinValor_DebeTenerError()
    {
        var cmd = ComandoValido() with
        {
            ClaveFiscal = "AFIP",
            ValorClaveFiscal = null
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validar_PaisIdInvalido_DebeTenerError()
    {
        var cmd = ComandoValido() with { PaisId = 0 };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.PaisId);
    }

    [Fact]
    public void Validar_SexoInvalido_DebeTenerError()
    {
        var cmd = ComandoValido() with { TipoPersoneria = "FISICA", Nombre = "Juan", Apellido = "Perez", Sexo = "X" };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Sexo);
    }

    [Fact]
    public void Validar_FechaNacimientoFutura_DebeTenerError()
    {
        var cmd = ComandoValido() with { FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.FechaNacimiento);
    }

    [Fact]
    public void Validar_VigenciaCreditoInvertida_DebeTenerError()
    {
        var cmd = ComandoValido() with
        {
            VigenciaCreditoDesde = new DateOnly(2025, 12, 31),
            VigenciaCreditoHasta = new DateOnly(2025, 1, 1)
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validar_ComisionVendedorActivaSinVendedor_DebeTenerError()
    {
        var cmd = ComandoValido() with { AplicaComisionVendedor = true, VendedorId = null };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x);
    }
}
