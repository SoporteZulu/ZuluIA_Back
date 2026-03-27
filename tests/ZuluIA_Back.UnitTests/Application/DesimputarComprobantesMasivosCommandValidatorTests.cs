using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class DesimputarComprobantesMasivosCommandValidatorTests
{
    private readonly DesimputarComprobantesMasivosCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var cmd = new DesimputarComprobantesMasivosCommand(DateOnly.FromDateTime(DateTime.Today), "Ajuste", [1, 2]);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SinIds_DebeTenerError()
    {
        var cmd = new DesimputarComprobantesMasivosCommand(DateOnly.FromDateTime(DateTime.Today), null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ImputacionIds);
    }
}
