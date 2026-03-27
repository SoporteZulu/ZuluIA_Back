using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ImportarOperacionesCommandValidatorTests
{
    private readonly ImportarOperacionesCommandValidator _validator = new();

    [Fact]
    public void Validar_NombreVacio_DebeTenerError()
    {
        var result = _validator.TestValidate(new ImportarOperacionesCommand(string.Empty, [new OperacionImportacionInput("1", "ALTA", null, true)], null));
        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }
}
