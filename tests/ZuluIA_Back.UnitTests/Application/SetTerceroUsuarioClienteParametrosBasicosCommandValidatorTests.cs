using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class SetTerceroUsuarioClienteParametrosBasicosCommandValidatorTests
{
    private readonly SetTerceroUsuarioClienteParametrosBasicosCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var command = new SetTerceroUsuarioClienteParametrosBasicosCommand(10, 3, "DEFAULT");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SucursalInvalida_DebeTenerError()
    {
        var command = new SetTerceroUsuarioClienteParametrosBasicosCommand(10, 0, "DEFAULT");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DefaultSucursalId);
    }
}
