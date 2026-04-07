using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ImputarComprobanteCommandValidatorTests
{
    private readonly ImputarComprobanteCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var cmd = new ImputarComprobanteCommand(1, 2, 100m, DateOnly.FromDateTime(DateTime.Today));
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_OrigenIgualADestino_DebeTenerError()
    {
        var cmd = new ImputarComprobanteCommand(1, 1, 100m, DateOnly.FromDateTime(DateTime.Today));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
