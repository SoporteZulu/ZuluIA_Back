using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Produccion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class IniciarOrdenTrabajoCommandValidatorTests
{
    private readonly IniciarOrdenTrabajoCommandValidator _validator = new();

    [Fact]
    public void Validar_IdInvalido_DebeTenerError()
    {
        var cmd = new IniciarOrdenTrabajoCommand(0);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
