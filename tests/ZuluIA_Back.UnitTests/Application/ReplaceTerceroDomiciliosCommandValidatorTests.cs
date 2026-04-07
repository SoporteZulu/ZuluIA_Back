using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ReplaceTerceroDomiciliosCommandValidatorTests
{
    private readonly ReplaceTerceroDomiciliosCommandValidator _validator = new();

    [Fact]
    public void Validar_DosDomiciliosPorDefecto_DebeTenerError()
    {
        var command = new ReplaceTerceroDomiciliosCommand(
            10,
            [
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Casa central", null, null, null, true, 0),
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Depósito", null, null, null, true, 1)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Domicilios);
    }

    [Fact]
    public void Validar_MasDeTresDomicilios_DebeTenerError()
    {
        var command = new ReplaceTerceroDomiciliosCommand(
            10,
            [
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 1", null, null, null, true, 0),
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 2", null, null, null, false, 1),
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 3", null, null, null, false, 2),
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 4", null, null, null, false, 3)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Domicilios);
    }
}
