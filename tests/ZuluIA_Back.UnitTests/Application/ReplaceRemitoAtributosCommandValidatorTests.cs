using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Ventas.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ReplaceRemitoAtributosCommandValidatorTests
{
    private readonly ReplaceRemitoAtributosCommandValidator _validator = new();

    private static ReplaceRemitoAtributosCommand Command() => new(
        10,
        [
            new RemitoAtributoInput("Ruta", "A1", "TEXTO"),
            new RemitoAtributoInput("Prioridad", "Alta", "TEXTO")
        ]);

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var result = _validator.TestValidate(Command());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_ClaveDuplicada_DebeTenerError()
    {
        var result = _validator.TestValidate(Command() with
        {
            Atributos =
            [
                new RemitoAtributoInput("Ruta", "A1", "TEXTO"),
                new RemitoAtributoInput("RUTA", "A2", "TEXTO")
            ]
        });

        result.ShouldHaveValidationErrorFor(x => x.Atributos);
    }
}
