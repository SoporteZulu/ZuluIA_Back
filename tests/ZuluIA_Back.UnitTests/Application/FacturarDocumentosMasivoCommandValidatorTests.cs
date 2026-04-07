using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;

namespace ZuluIA_Back.UnitTests.Application;

public class FacturarDocumentosMasivoCommandValidatorTests
{
    private readonly FacturarDocumentosMasivoCommandValidator _validator = new();

    [Fact]
    public void Validar_SinComprobantes_DebeTenerError()
    {
        var cmd = new FacturarDocumentosMasivoCommand([], 1, null, new DateOnly(2025, 1, 1), null, null, OperacionStockVenta.Ninguna, OperacionCuentaCorrienteVenta.Debito);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteOrigenIds);
    }
}
