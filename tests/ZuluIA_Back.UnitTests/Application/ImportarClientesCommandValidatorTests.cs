using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ImportarClientesCommandValidatorTests
{
    private readonly ImportarClientesCommandValidator _validator = new();

    [Fact]
    public void Validar_SinClientes_DebeTenerError()
    {
        var result = _validator.TestValidate(new ImportarClientesCommand([], true, null));
        result.ShouldHaveValidationErrorFor(x => x.Clientes);
    }
}
