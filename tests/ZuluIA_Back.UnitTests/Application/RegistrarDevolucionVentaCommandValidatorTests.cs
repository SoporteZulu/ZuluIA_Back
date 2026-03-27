using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Ventas.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarDevolucionVentaCommandValidatorTests
{
    private readonly RegistrarDevolucionVentaCommandValidator _validator = new();

    private static RegistrarDevolucionVentaCommand Command() => new(
        SucursalId: 1,
        PuntoFacturacionId: 1,
        TipoComprobanteId: 1,
        Fecha: DateOnly.FromDateTime(DateTime.Today),
        FechaVencimiento: null,
        TerceroId: 1,
        MonedaId: 1,
        Cotizacion: 1m,
        Percepciones: 0m,
        Observacion: null,
        ComprobanteOrigenId: 10,
        Items:
        [
            new ComprobanteItemInput(1, "Producto", 2m, 0, 1000, 0m, 1, 1, 1)
        ],
        ReingresaStock: true,
        AcreditaCuentaCorriente: true);

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var result = _validator.TestValidate(Command());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_CotizacionInvalida_DebeTenerError()
    {
        var result = _validator.TestValidate(Command() with { Cotizacion = 0m });
        result.ShouldHaveValidationErrorFor(x => x.Cotizacion);
    }
}
