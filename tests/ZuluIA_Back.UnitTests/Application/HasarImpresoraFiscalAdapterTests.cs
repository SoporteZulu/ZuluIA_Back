using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Services;

namespace ZuluIA_Back.UnitTests.Application;

public class HasarImpresoraFiscalAdapterTests
{
    [Fact]
    public void Imprimir_DebeGenerarPayloadHasar()
    {
        var adapter = new HasarImpresoraFiscalAdapter();
        var result = adapter.Imprimir(CrearComprobante());
        result.Marca.Should().Be("HASAR");
        result.PayloadFiscal.Should().Contain("HASAR|INICIO");
    }

    private static ComprobanteDetalleDto CrearComprobante() => new()
    {
        Id = 1,
        NumeroFormateado = "0001-00000001",
        Fecha = new DateOnly(2025, 1, 1),
        TerceroRazonSocial = "Cliente",
        TerceroCuit = "20111111112",
        Total = 121m,
        Items = [new ComprobanteItemDto { ItemCodigo = "ITM1", Descripcion = "Producto", Cantidad = 1m, TotalLinea = 121m }]
    };
}
