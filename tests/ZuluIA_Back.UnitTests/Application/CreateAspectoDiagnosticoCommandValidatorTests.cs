using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateAspectoDiagnosticoCommandValidatorTests
{
    private readonly CreateAspectoDiagnosticoCommandValidator _validator = new();

    [Fact]
    public void Validar_PesoInvalido_DebeTenerError()
    {
        var result = _validator.TestValidate(new CreateAspectoDiagnosticoCommand(1, "ASP", "Aspecto", 0m));
        result.ShouldHaveValidationErrorFor(x => x.Peso);
    }
}
