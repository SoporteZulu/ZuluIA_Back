using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Contratos.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarImpactoContratoCommandValidatorTests
{
    private readonly RegistrarImpactoContratoCommandValidator _validator = new();

    [Fact]
    public void Validar_DescripcionVacia_DebeTenerError()
    {
        var cmd = new RegistrarImpactoContratoCommand(1, TipoImpactoContrato.Comercial, new DateOnly(2025, 1, 1), 10m, string.Empty);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }
}
