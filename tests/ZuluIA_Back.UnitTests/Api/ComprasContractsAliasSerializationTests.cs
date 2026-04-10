using System.Text.Json;
using FluentAssertions;
using Xunit;
using ZuluIA_Back.Api.Controllers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprasContractsAliasSerializationTests
{
    [Fact]
    public void Serialize_RemitoCompras_ExponeAliasesDeComprobante()
    {
        var dto = new CompraRemitoResumenDto(
            1,
            "Valorizado",
            "Proveedor",
            "RVC-001-000044",
            new DateOnly(2026, 4, 20),
            "Central",
            "RECIBIDO",
            "OC-1",
            "REC-1",
            "Transportista",
            "Responsable",
            "Observacion",
            [],
            100m,
            []);

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"numero\":\"RVC-001-000044\"");
        json.Should().Contain("\"comprobante\":\"RVC-001-000044\"");
        json.Should().Contain("\"comprobanteReferencia\":\"RVC-001-000044\"");
    }

    [Fact]
    public void Serialize_AjusteCompras_ExponeAliasComprobante()
    {
        var dto = new CompraAjusteResumenDto(
            2,
            "Crédito",
            "Proveedor",
            "FC-0001-000001",
            "Motivo",
            "APLICADO",
            new DateOnly(2026, 4, 20),
            100m,
            "OC-1",
            "DEV-1",
            "Responsable",
            "Impacto",
            "Observacion",
            "Circuito",
            true,
            [],
            []);

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"comprobanteReferencia\":\"FC-0001-000001\"");
        json.Should().Contain("\"comprobante\":\"FC-0001-000001\"");
    }

    [Fact]
    public void Serialize_DevolucionCompras_ExponeAliasComprobanteReferencia()
    {
        var dto = new CompraDevolucionResumenDto(
            3,
            "Stock",
            "Proveedor",
            "FC-0001-000002",
            "Motivo",
            "PROCESADA",
            new DateOnly(2026, 4, 20),
            "Central",
            "OC-1",
            "REM-1",
            "REC-1",
            "Responsable",
            "Resolucion",
            "Impacto",
            false,
            [],
            100m,
            []);

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"comprobante\":\"FC-0001-000002\"");
        json.Should().Contain("\"comprobanteReferencia\":\"FC-0001-000002\"");
    }
}
