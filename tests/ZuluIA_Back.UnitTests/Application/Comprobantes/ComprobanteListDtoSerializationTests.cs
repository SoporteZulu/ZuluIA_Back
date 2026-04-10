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

    [Fact]
    public void Serialize_CuandoTieneCamposOperativos_ExponeContratoFrontend()
    {
        var dto = new ComprobanteListDto
        {
            Observacion = "Recepción parcial",
            CreatedAt = new DateTimeOffset(2026, 4, 20, 10, 30, 0, TimeSpan.Zero),
            FechaVtoCae = new DateOnly(2026, 5, 10),
            QrData = "qr-demo"
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"observacion\":");
        json.Should().Contain("\"createdAt\":");
        json.Should().Contain("\"fechaVtoCae\":\"2026-05-10\"");
        json.Should().Contain("\"caeFechaVto\":\"2026-05-10\"");
        json.Should().Contain("\"qrData\":\"qr-demo\"");
    }
}
