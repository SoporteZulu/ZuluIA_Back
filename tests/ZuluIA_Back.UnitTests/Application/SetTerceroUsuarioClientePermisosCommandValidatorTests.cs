using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class SetTerceroUsuarioClientePermisosCommandValidatorTests
{
    private readonly SetTerceroUsuarioClientePermisosCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var command = new SetTerceroUsuarioClientePermisosCommand(10, [new SetTerceroUsuarioClientePermisoItem(5, true)]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SeguridadDuplicada_DebeTenerError()
    {
        var command = new SetTerceroUsuarioClientePermisosCommand(
            10,
            [
                new SetTerceroUsuarioClientePermisoItem(5, true),
                new SetTerceroUsuarioClientePermisoItem(5, false)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Permisos);
    }
}
