using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.PuntoVenta.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ProcesarComprobanteDeuceCommandValidatorTests
{
    private readonly ProcesarComprobanteDeuceCommandValidator _validator = new();

    [Fact]
    public void Validar_ReferenciaVacia_DebeTenerError()
    {
        var cmd = new ProcesarComprobanteDeuceCommand(1, string.Empty, null, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ReferenciaExterna);
    }
}
