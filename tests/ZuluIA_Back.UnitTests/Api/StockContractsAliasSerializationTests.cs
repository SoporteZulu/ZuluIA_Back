using System.Text.Json;
using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Features.Stock.DTOs;

namespace ZuluIA_Back.UnitTests.Api;

public class StockContractsAliasSerializationTests
{
    [Fact]
    public void Serialize_StockBajoMinimo_ExponeAliasesDelFrontend()
    {
        var dto = new StockBajoMinimoDto
        {
            ItemId = 7,
            ItemCodigo = "ITEM-7",
            ItemDescripcion = "Producto 7",
            DepositoId = 10,
            DepositoDescripcion = "Principal",
            CantidadActual = 2m,
            StockMinimo = 5m,
            Diferencia = 3m
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"itemCodigo\":\"ITEM-7\"");
        json.Should().Contain("\"codigo\":\"ITEM-7\"");
        json.Should().Contain("\"itemDescripcion\":\"Producto 7\"");
        json.Should().Contain("\"descripcion\":\"Producto 7\"");
        json.Should().Contain("\"cantidadActual\":2");
        json.Should().Contain("\"stockActual\":2");
    }
}
