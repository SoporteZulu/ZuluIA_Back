using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateEmpleadoCommandValidatorTests
{
    private readonly CreateEmpleadoCommandValidator _validator = new();

    private static CreateEmpleadoCommand ComandoValido() => new(
        TerceroId: 1,
        SucursalId: 1,
        Legajo: "EMP001",
        Cargo: "Analista",
        Area: null,
        FechaIngreso: DateOnly.FromDateTime(DateTime.Today),
        SueldoBasico: 100000m,
        MonedaId: 1
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
        var cmd = ComandoValido() with { Legajo = string.Empty };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Legajo);
    }

    [Fact]
    public void Validar_CargoVacio_DebeHaveError()
    {
        var cmd = ComandoValido() with { Cargo = string.Empty };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Cargo);
    }

    [Fact]
    public void Validar_SueldoNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with { SueldoBasico = -1m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.SueldoBasico);
    }

    [Fact]
    public void Validar_MonedaIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { MonedaId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }
}
