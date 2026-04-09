using System.Text.Json;
using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.UnitTests.Application.Comprobantes;

public class ComprobanteListDtoSerializationTests
{
    [Fact]
    public void Serialize_CuandoTieneEstado_StringEnPayloadWeb()
    {
        var dto = new ComprobanteListDto
        {
            Estado = "EMITIDO"
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"estado\":\"EMITIDO\"");
    }

    [Fact]
    public void Serialize_CuandoTieneAlias_ExponeNroComprobante()
    {
        var dto = new ComprobanteListDto
        {
            NumeroFormateado = "0001-00000025"
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"nroComprobante\":\"0001-00000025\"");
    }
}
