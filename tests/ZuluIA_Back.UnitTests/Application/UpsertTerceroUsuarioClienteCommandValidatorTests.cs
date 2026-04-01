using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class UpsertTerceroUsuarioClienteCommandValidatorTests
{
    private readonly UpsertTerceroUsuarioClienteCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var command = new UpsertTerceroUsuarioClienteCommand(10, "cliente.demo", "secret123", "secret123", 3);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_ContraseniaDistinta_DebeTenerError()
    {
        var command = new UpsertTerceroUsuarioClienteCommand(10, "cliente.demo", "secret123", "otra", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validar_GrupoInvalido_DebeTenerError()
    {
        var command = new UpsertTerceroUsuarioClienteCommand(10, "cliente.demo", null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UsuarioGrupoId);
    }
}
