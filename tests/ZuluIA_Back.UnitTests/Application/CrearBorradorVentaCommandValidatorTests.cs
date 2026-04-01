using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Ventas.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CrearBorradorVentaCommandValidatorTests
{
    private readonly CrearBorradorVentaCommandValidator _validator = new();

    private static CrearBorradorVentaCommand Command() => new(
        SucursalId: 1,
        PuntoFacturacionId: 1,
        TipoComprobanteId: 1,
        Fecha: DateOnly.FromDateTime(DateTime.Today),
        FechaVencimiento: null,
        FechaEntregaCompromiso: null,
        TerceroId: 1,
        MonedaId: 1,
        Cotizacion: 1m,
        Percepciones: 0m,
        Observacion: null,
        ComprobanteOrigenId: null,
        Items:
        [
            new ComprobanteItemInput(1, "Producto", 1m, 0, 1000, 0m, 1, 1, 1)
        ]);

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var result = _validator.TestValidate(Command());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SinItems_DebeTenerError()
    {
        var result = _validator.TestValidate(Command() with { Items = [] });
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }
}
