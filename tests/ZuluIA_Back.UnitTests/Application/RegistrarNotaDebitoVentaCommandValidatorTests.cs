using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Ventas.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarNotaDebitoVentaCommandValidatorTests
{
    private readonly RegistrarNotaDebitoVentaCommandValidator _validator = new();

    private static RegistrarNotaDebitoVentaCommand ComandoValido() => new(
        SucursalId: 1,
        PuntoFacturacionId: 1,
        TipoComprobanteId: 10,
        Fecha: new DateOnly(2026, 3, 31),
        FechaVencimiento: new DateOnly(2026, 4, 15),
        TerceroId: 100,
        MonedaId: 1,
        Cotizacion: 1m,
        Percepciones: 0m,
        Observacion: "Ajuste comercial",
        ComprobanteOrigenId: 200,
        MotivoDebitoId: 5,
        MotivoDebitoObservacion: "Diferencia de precio",
        Items:
        [
            new ComprobanteItemInput(
                ItemId: 50,
                Descripcion: "Producto A",
                Cantidad: 2m,
                CantidadBonificada: 0,
                PrecioUnitario: 100,
                DescuentoPct: 0m,
                AlicuotaIvaId: 1,
                DepositoId: null,
                Orden: 1,
                ComprobanteItemOrigenId: 500,
                CantidadDocumentoOrigen: 2m,
                PrecioDocumentoOrigen: 95m)
        ],
        ListaPreciosId: 2,
        VendedorId: 3,
        CanalVentaId: 4,
        CondicionPagoId: 5,
        PlazoDias: 30,
        Emitir: true);

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SinMotivoDebito_DebeTenerError()
    {
        var cmd = ComandoValido() with { MotivoDebitoId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.MotivoDebitoId);
    }

    [Fact]
    public void Validar_SinItems_DebeTenerError()
    {
        var cmd = ComandoValido() with { Items = [] };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Validar_ItemConCantidadDocumentoOrigenNegativa_DebeTenerError()
    {
        var item = ComandoValido().Items[0] with { CantidadDocumentoOrigen = -1m };
        var cmd = ComandoValido() with { Items = [item] };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
