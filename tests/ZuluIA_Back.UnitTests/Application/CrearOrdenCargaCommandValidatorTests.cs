using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CrearOrdenCargaCommandValidatorTests
{
    private readonly CrearOrdenCargaCommandValidator _validator = new();

    [Fact]
    public void Validar_DestinoVacio_DebeTenerError()
    {
        var cmd = new CrearOrdenCargaCommand(1, null, DateOnly.FromDateTime(DateTime.Today), "Origen", string.Empty, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Destino);
    }
}
