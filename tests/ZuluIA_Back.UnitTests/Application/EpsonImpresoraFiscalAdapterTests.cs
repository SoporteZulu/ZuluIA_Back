using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Services;

namespace ZuluIA_Back.UnitTests.Application;

public class EpsonImpresoraFiscalAdapterTests
{
    [Fact]
    public void Imprimir_DebeGenerarPayloadEpson()
    {
        var adapter = new EpsonImpresoraFiscalAdapter();
        var result = adapter.Imprimir(CrearComprobante());
        result.Marca.Should().Be("EPSON");
        result.PayloadFiscal.Should().Contain("EPSON|ABRIR_COMPROBANTE");
    }

    private static ComprobanteDetalleDto CrearComprobante() => new()
    {
        Id = 1,
        NumeroFormateado = "0001-00000001",
        Fecha = new DateOnly(2025, 1, 1),
        TerceroRazonSocial = "Cliente",
        TerceroCuit = "20111111112",
        Total = 121m,
        Items = [new ComprobanteItemDto { Descripcion = "Producto", Cantidad = 1m, PrecioUnitario = 100m, TotalLinea = 121m }]
    };
}
