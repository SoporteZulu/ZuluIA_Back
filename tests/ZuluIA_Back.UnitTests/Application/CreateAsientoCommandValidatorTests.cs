using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateAsientoCommandValidatorTests
{
    private readonly CreateAsientoCommandValidator _validator = new();

    private static CreateAsientoCommand ComandoBalanceado() => new(
        EjercicioId: 1,
        SucursalId: 1,
        Fecha: DateOnly.FromDateTime(DateTime.Today),
        Descripcion: "Venta Factura A",
        OrigenTabla: null,
        OrigenId: null,
        Lineas: [
            new CreateAsientoLineaDto(1, 1000m, 0m,    "Clientes", 1),
            new CreateAsientoLineaDto(2, 0m,    826m,  "Ventas",   2),
            new CreateAsientoLineaDto(3, 0m,    174m,  "IVA",      3)
        ]
    );

    [Fact]
    public void Validar_AsientoBalanceado_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoBalanceado());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SinLineas_DebeHaveError()
    {
        var cmd = ComandoBalanceado() with { Lineas = [] };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Lineas);
    }

    [Fact]
    public void Validar_UnaLinea_DebeHaveError()
    {
        var cmd = ComandoBalanceado() with
        {
            Lineas = [new CreateAsientoLineaDto(1, 1000m, 0m, "Test", 1)]
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Lineas);
    }

    [Fact]
    public void Validar_AsientoNoBalanceado_DebeHaveError()
    {
        var cmd = ComandoBalanceado() with
        {
            Lineas = [
                new CreateAsientoLineaDto(1, 1000m, 0m,   "Debe",  1),
                new CreateAsientoLineaDto(2, 0m,    500m, "Haber", 2)
            ]
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Lineas);
    }

    [Fact]
    public void Validar_DescripcionVacia_DebeHaveError()
    {
        var cmd = ComandoBalanceado() with { Descripcion = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void Validar_EjercicioIdCero_DebeHaveError()
    {
        var cmd = ComandoBalanceado() with { EjercicioId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.EjercicioId);
    }
}