using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Produccion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateFormulaProduccionCommandValidatorTests
{
    private readonly CreateFormulaProduccionCommandValidator _validator = new();

    private static CreateFormulaProduccionCommand ComandoValido() => new(
        Codigo: "FP001",
        Descripcion: "Fórmula de Producción Test",
        ItemResultadoId: 1,
        CantidadResultado: 10m,
        UnidadMedidaId: null,
        Observacion: null,
        Ingredientes: [new IngredienteInput(1, 2m, null, false, 1)]
    );

    [Fact]
    public void Validar_ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_CodigoVacio_DebeHaveError()
    {
        var cmd = ComandoValido() with { Codigo = string.Empty };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Codigo);
    }

    [Fact]
    public void Validar_SinIngredientes_DebeHaveError()
    {
        var cmd = ComandoValido() with { Ingredientes = [] };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Ingredientes);
    }

    [Fact]
    public void Validar_CantidadResultadoCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { CantidadResultado = 0m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CantidadResultado);
    }

    [Fact]
    public void Validar_IngredienteConCantidadCero_DebeHaveError()
    {
        var ingredienteInvalido = new IngredienteInput(1, 0m, null, false, 1);
        var cmd = ComandoValido() with { Ingredientes = [ingredienteInvalido] };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
