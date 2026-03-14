using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Retenciones.Commands;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateTipoRetencionCommandValidatorTests
{
    private readonly CreateTipoRetencionCommandValidator _validator = new();

    private static EscalaRetencionInputDto EscalaValida() =>
        new("Tramo 1", 0m, 50_000m, 5m);

    private static CreateTipoRetencionCommand ComandoValido() =>
        new(
            Descripcion: "Ganancias 4ta Categoría",
            Regimen: "GANANCIAS",
            MinimoNoImponible: 0m,
            AcumulaPago: false,
            TipoComprobanteId: null,
            ItemId: null,
            Escalas: []
        );

    [Fact]
    public void Validar_ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_ComandoConEscalaValida_NoDebeHaveErrores()
    {
        var cmd = ComandoValido() with { Escalas = [EscalaValida()] };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_DescripcionVacia_DebeHaveError()
    {
        var cmd = ComandoValido() with { Descripcion = "" };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void Validar_RegimenVacio_DebeHaveError()
    {
        var cmd = ComandoValido() with { Regimen = "" };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Regimen);
    }

    [Fact]
    public void Validar_MinimoNoImponibleNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with { MinimoNoImponible = -1m };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.MinimoNoImponible);
    }

    [Fact]
    public void Validar_EscalaConDescripcionVacia_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Escalas = [new EscalaRetencionInputDto("", 0m, 50_000m, 5m)]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Escalas[0].Descripcion");
    }

    [Fact]
    public void Validar_EscalaConPorcentajeMayor100_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Escalas = [new EscalaRetencionInputDto("Tramo", 0m, 50_000m, 101m)]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Escalas[0].Porcentaje");
    }

    [Fact]
    public void Validar_EscalaConPorcentajeNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Escalas = [new EscalaRetencionInputDto("Tramo", 0m, 50_000m, -1m)]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Escalas[0].Porcentaje");
    }

    [Fact]
    public void Validar_EscalaConImporteDesdenegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Escalas = [new EscalaRetencionInputDto("Tramo", -1m, 50_000m, 5m)]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Escalas[0].ImporteDesde");
    }

    [Fact]
    public void Validar_SinEscalas_NoDebeHaveError()
    {
        var cmd = ComandoValido() with { Escalas = [] };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
