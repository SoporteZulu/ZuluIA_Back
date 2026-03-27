using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ExportarZgcotLegacyCommandValidatorTests
{
    private readonly ExportarZgcotLegacyCommandValidator _validator = new();

    [Fact]
    public void Validar_SucursalInvalida_DebeTenerError()
    {
        var cmd = new ExportarZgcotLegacyCommand(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), 0);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.SucursalId);
    }
}
