using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Produccion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CrearOrdenEmpaqueCommandValidatorTests
{
    private readonly CrearOrdenEmpaqueCommandValidator _validator = new();

    [Fact]
    public void Validar_ItemInvalido_DebeTenerError()
    {
        var cmd = new CrearOrdenEmpaqueCommand(1, 0, 1, DateOnly.FromDateTime(DateTime.Today), 1m, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ItemId);
    }
}
