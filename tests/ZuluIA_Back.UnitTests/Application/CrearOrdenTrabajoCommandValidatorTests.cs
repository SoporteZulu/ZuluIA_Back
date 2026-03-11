using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Produccion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CrearOrdenTrabajoCommandValidatorTests
{
    private readonly CrearOrdenTrabajoCommandValidator _validator = new();

    private static CrearOrdenTrabajoCommand ComandoValido() => new(
        SucursalId: 1,
        FormulaId: 1,
        DepositoOrigenId: 1,
        DepositoDestinoId: 2,
        Fecha: DateOnly.FromDateTime(DateTime.Today),
        FechaFinPrevista: null,
        Cantidad: 5m,
        Observacion: null
    );

    [Fact]
    public void Validar_ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SucursalIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { SucursalId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.SucursalId);
    }

    [Fact]
    public void Validar_FormulaIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { FormulaId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FormulaId);
    }

    [Fact]
    public void Validar_CantidadCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { Cantidad = 0m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Cantidad);
    }

    [Fact]
    public void Validar_FechaFinPrevistaAnteriorAFecha_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            FechaFinPrevista = DateOnly.FromDateTime(DateTime.Today)
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FechaFinPrevista);
    }
}
