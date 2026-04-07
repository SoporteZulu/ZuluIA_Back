using FluentAssertions;
using System.Text;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Services;

namespace ZuluIA_Back.UnitTests.Application;

public class ArchivoTabularParserServiceTests
{
    [Fact]
    public void Parse_Csv_DebeLeerHeadersYFilas()
    {
        var parser = new ArchivoTabularParserService();
        var csv = Encoding.UTF8.GetBytes("Legajo,RazonSocial,NroDocumento\nC1,Cliente 1,201\n");

        var rows = parser.Parse("clientes.csv", csv);

        rows.Should().HaveCount(1);
        rows[0]["Legajo"].Should().Be("C1");
        rows[0]["RazonSocial"].Should().Be("Cliente 1");
        rows[0]["NroDocumento"].Should().Be("201");
    }

    [Fact]
    public void Parse_TxtConPuntoYComa_DebeDetectarSeparador()
    {
        var parser = new ArchivoTabularParserService();
        var txt = Encoding.UTF8.GetBytes("Legajo;RazonSocial;NroDocumento\nC1;Cliente 1;201\n");

        var rows = parser.Parse("clientes.txt", txt);

        rows.Should().HaveCount(1);
        rows[0]["Legajo"].Should().Be("C1");
        rows[0]["RazonSocial"].Should().Be("Cliente 1");
        rows[0]["NroDocumento"].Should().Be("201");
    }
}
