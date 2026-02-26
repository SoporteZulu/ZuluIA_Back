using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void Domicilio_ConMismosDatos_DebeSerIgual()
    {
        var d1 = new Domicilio("San Martín", "123", null, null, "X5000", 1, null);
        var d2 = new Domicilio("San Martín", "123", null, null, "X5000", 1, null);

        d1.Should().Be(d2);
        (d1 == d2).Should().BeTrue();
    }

    [Fact]
    public void Domicilio_ConDiferentesDatos_DebeSerDistinto()
    {
        var d1 = new Domicilio("San Martín", "123", null, null, "X5000", 1, null);
        var d2 = new Domicilio("Belgrano", "456", null, null, "X5000", 1, null);

        d1.Should().NotBe(d2);
        (d1 != d2).Should().BeTrue();
    }

    [Fact]
    public void Domicilio_Completo_DebeFormatearCorrectamente()
    {
        var d = new Domicilio("San Martín", "123", "1", "B", "X5000", 1, null);

        d.Completo.Should().Be("San Martín 123 1 B");
    }

    [Fact]
    public void NroComprobante_Formateado_DebeFormatearCorrectamente()
    {
        var nro = new NroComprobante(1, 1);

        nro.Formateado.Should().Be("0001-00000001");
    }

    [Fact]
    public void NroComprobante_ConMismosDatos_DebeSerIgual()
    {
        var n1 = new NroComprobante(1, 100);
        var n2 = new NroComprobante(1, 100);

        n1.Should().Be(n2);
    }

    [Fact]
    public void NroComprobante_PrefijoCero_DebeLanzarExcepcion()
    {
        var act = () => new NroComprobante(0, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NroComprobante_NumeroCero_DebeLanzarExcepcion()
    {
        var act = () => new NroComprobante(1, 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Dinero_ConMismosDatos_DebeSerIgual()
    {
        var d1 = new Dinero(1000m, 1);
        var d2 = new Dinero(1000m, 1);

        d1.Should().Be(d2);
    }

    [Fact]
    public void Dinero_Sumar_DebeRetornarNuevoObjeto()
    {
        var d = new Dinero(1000m, 1);

        var resultado = d.Sumar(500m);

        resultado.Importe.Should().Be(1500m);
        resultado.MonedaId.Should().Be(1);
        d.Importe.Should().Be(1000m);
    }

    [Fact]
    public void Dinero_MonedaIdCero_DebeLanzarExcepcion()
    {
        var act = () => new Dinero(1000m, 0);

        act.Should().Throw<ArgumentException>();
    }
}