using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarPickingOrdenPreparacionCommandValidatorTests
{
    private readonly RegistrarPickingOrdenPreparacionCommandValidator _validator = new();

    [Fact]
    public void Validar_SinDetalles_DebeTenerError()
    {
        var cmd = new RegistrarPickingOrdenPreparacionCommand(1, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Detalles);
    }
}
