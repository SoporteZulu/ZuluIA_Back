using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Cajas.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarTransferenciaCommandValidatorTests
{
    private readonly RegistrarTransferenciaCommandValidator _validator = new();

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static RegistrarTransferenciaCommand ComandoValido() =>
        new(
            SucursalId: 1L,
            CajaOrigenId: 10L,
            CajaDestinoId: 20L,
            Fecha: Hoy,
            Importe: 5_000m,
            MonedaId: 1L,
            Cotizacion: 1m,
            Concepto: "Traspaso"
        );

    [Fact]
    public void Validar_ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_OrigenIgualADestino_DebeHaveError()
    {
        var cmd = ComandoValido() with { CajaOrigenId = 10L, CajaDestinoId = 10L };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.CajaDestinoId);
    }

    [Fact]
    public void Validar_ImporteNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with { Importe = -1m };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Importe);
    }

    [Fact]
    public void Validar_ImporteCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { Importe = 0m };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Importe);
    }

    [Fact]
    public void Validar_SucursalIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { SucursalId = 0L };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.SucursalId);
    }

    [Fact]
    public void Validar_CajaOrigenIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { CajaOrigenId = 0L };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.CajaOrigenId);
    }

    [Fact]
    public void Validar_CajaDestinoIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { CajaDestinoId = 0L };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.CajaDestinoId);
    }

    [Fact]
    public void Validar_CotizacionCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { Cotizacion = 0m };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Cotizacion);
    }

    [Fact]
    public void Validar_MonedaIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { MonedaId = 0L };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }
}
