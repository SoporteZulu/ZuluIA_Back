using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CrearOrdenCompraCommandValidatorTests
{
    private readonly CrearOrdenCompraCommandValidator _validator = new();

    private static CrearOrdenCompraCommand Command() => new(
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
        FechaEntregaReq: DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
        CondicionesEntrega: "A coordinar",
        Items:
        [
            new ComprobanteItemInput(1, "MP", 5m, 0, 100, 0m, 1, 1, 1)
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
