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
        TipoDocumentoId: 1,
        NroDocumento: "30-11111111-1",
        CondicionIvaId: 1,
        CategoriaId: null,
        EsCliente: true,
        EsProveedor: false,
        Calle: null,
        Nro: null,
        CodigoPostal: null,
        LocalidadId: null,
        BarrioId: null,
        Telefono: null,
        Celular: null,
        Email: null,
        Web: null,
        MonedaId: null,
        LimiteCredito: null,
        SucursalId: null
    );

    [Fact]
    public void Validar_ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_LegajoVacio_DebeHaveError()
    {
        var cmd = ComandoValido() with { Legajo = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Legajo);
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
    public void Validar_TipoDocumentoIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { TipoDocumentoId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TipoDocumentoId);
    }
}