using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class DesimputarComprobanteCommandValidatorTests
{
    private readonly DesimputarComprobanteCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var cmd = new DesimputarComprobanteCommand(1, DateOnly.FromDateTime(DateTime.Today), "Ajuste");
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_ImputacionInvalida_DebeTenerError()
    {
        var cmd = new DesimputarComprobanteCommand(0, DateOnly.FromDateTime(DateTime.Today), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ImputacionId);
    }
}
