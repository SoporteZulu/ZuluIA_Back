using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;

namespace ZuluIA_Back.UnitTests.Application;

public class EjecutarFacturacionAutomaticaCommandValidatorTests
{
    private readonly EjecutarFacturacionAutomaticaCommandValidator _validator = new();

    [Fact]
    public void Validar_RangoInvalido_DebeTenerError()
    {
        var cmd = new EjecutarFacturacionAutomaticaCommand(1, 1, 2, new DateOnly(2025, 2, 1), new DateOnly(2025, 1, 31), null, true, null, new DateOnly(2025, 2, 1), null, null, OperacionStockVenta.Ninguna, OperacionCuentaCorrienteVenta.Debito);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Hasta);
    }
}
