using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Contratos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RenovarContratosAutomaticamenteCommandValidatorTests
{
    private readonly RenovarContratosAutomaticamenteCommandValidator _validator = new();

    [Fact]
    public void Validar_PorcentajeMenorANegativo100_DebeTenerError()
    {
        var cmd = new RenovarContratosAutomaticamenteCommand(1, new DateOnly(2025, 1, 1), -101m);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PorcentajeAjuste);
    }
}
