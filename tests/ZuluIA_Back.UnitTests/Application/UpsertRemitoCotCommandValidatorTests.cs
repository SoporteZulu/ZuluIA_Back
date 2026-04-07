using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Ventas.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class UpsertRemitoCotCommandValidatorTests
{
    private readonly UpsertRemitoCotCommandValidator _validator = new();

    private static UpsertRemitoCotCommand Command() => new(
        ComprobanteId: 10,
        Numero: "123456",
        FechaVigencia: new DateOnly(2026, 3, 31),
        Descripcion: "COT de prueba");

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var result = _validator.TestValidate(Command());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_NumeroCorto_DebeTenerError()
    {
        var result = _validator.TestValidate(Command() with { Numero = "12345" });
        result.ShouldHaveValidationErrorFor(x => x.Numero);
    }

    [Fact]
    public void Validar_FechaDefault_DebeTenerError()
    {
        var result = _validator.TestValidate(Command() with { FechaVigencia = default });
        result.ShouldHaveValidationErrorFor(x => x.FechaVigencia);
    }
}
