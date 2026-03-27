using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Produccion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class FinalizarOrdenTrabajoCommandValidatorTests
{
    private readonly FinalizarOrdenTrabajoCommandValidator _validator = new();

    [Fact]
    public void Validar_CantidadProducidaNegativa_DebeTenerError()
    {
        var cmd = new FinalizarOrdenTrabajoCommand(1, DateOnly.FromDateTime(DateTime.Today), -1m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CantidadProducida);
    }
}
