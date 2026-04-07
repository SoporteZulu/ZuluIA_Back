using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Tesoreria.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CerrarCajaTesoreriaCommandValidatorTests
{
    private readonly CerrarCajaTesoreriaCommandValidator _validator = new();

    [Fact]
    public void Validar_CajaInvalida_DebeTenerError()
    {
        var result = _validator.TestValidate(new CerrarCajaTesoreriaCommand(0, DateOnly.FromDateTime(DateTime.Today), 100m, null));
        result.ShouldHaveValidationErrorFor(x => x.CajaId);
    }
}
