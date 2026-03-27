using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ImportarNotasPedidoCommandValidatorTests
{
    private readonly ImportarNotasPedidoCommandValidator _validator = new();

    [Fact]
    public void Validar_SinNotas_DebeTenerError()
    {
        var result = _validator.TestValidate(new ImportarNotasPedidoCommand([], null));
        result.ShouldHaveValidationErrorFor(x => x.NotasPedido);
    }
}
