using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class EjecutarSyncLegacyEspecificoCommandValidatorTests
{
    private readonly EjecutarSyncLegacyEspecificoCommandValidator _validator = new();

    [Fact]
    public void Validar_CodigoVacio_DebeTenerError()
    {
        var cmd = new EjecutarSyncLegacyEspecificoCommand(string.Empty, 1, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Codigo);
    }
}
