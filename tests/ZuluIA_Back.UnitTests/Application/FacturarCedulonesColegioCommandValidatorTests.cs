using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Colegio.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class FacturarCedulonesColegioCommandValidatorTests
{
    private readonly FacturarCedulonesColegioCommandValidator _validator = new();

    [Fact]
    public void Validar_SinCedulones_DebeTenerError()
    {
        var cmd = new FacturarCedulonesColegioCommand([], null, new DateOnly(2025, 1, 1), null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CedulonIds);
    }
}
