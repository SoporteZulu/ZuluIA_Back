using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// NumeroComprobante (record-based value object)
// ─────────────────────────────────────────────────────────────────────────────

public class NumeroComprobanteTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var nro = NumeroComprobante.Crear(1, 100);
        nro.Prefijo.Should().Be(1);
        nro.Numero.Should().Be(100);
    }

    [Fact]
    public void Crear_PrefijoCero_LanzaExcepcion()
    {
        var act = () => NumeroComprobante.Crear(0, 1);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_NumeroCero_LanzaExcepcion()
    {
        var act = () => NumeroComprobante.Crear(1, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Formateado_DebeFormatearCon4Y8Digitos()
    {
        var nro = NumeroComprobante.Crear(1, 100);
        nro.Formateado.Should().Be("0001-00000100");
    }

    [Fact]
    public void ToString_DebeRetornarFormateado()
    {
        var nro = NumeroComprobante.Crear(5, 99);
        nro.ToString().Should().Be("0005-00000099");
    }

    [Fact]
    public void Igualdad_MismosPrefijosYNumero_SonIguales()
    {
        var n1 = NumeroComprobante.Crear(1, 100);
        var n2 = NumeroComprobante.Crear(1, 100);
        n1.Should().Be(n2);
        (n1 == n2).Should().BeTrue();
    }

    [Fact]
    public void Igualdad_DiferentePrefijo_SonDistintos()
    {
        var n1 = NumeroComprobante.Crear(1, 100);
        var n2 = NumeroComprobante.Crear(2, 100);
        n1.Should().NotBe(n2);
    }

    [Fact]
    public void Parse_StringValido_RetornaInstanciaCorrecta()
    {
        var nro = NumeroComprobante.Parse("0001-00000050");
        nro.Prefijo.Should().Be(1);
        nro.Numero.Should().Be(50);
    }

    [Fact]
    public void Parse_FormatoInvalido_LanzaFormatException()
    {
        var act = () => NumeroComprobante.Parse("00010000050");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_PartesNoNumericas_LanzaFormatException()
    {
        var act = () => NumeroComprobante.Parse("XXXX-YYYYYYYY");
        act.Should().Throw<FormatException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Dinero — métodos no cubiertos (Restar, Multiplicar, Cero)
// ─────────────────────────────────────────────────────────────────────────────

public class DineroExtendedTests
{
    [Fact]
    public void Restar_DebeRetornarNuevoObjeto()
    {
        var d = new Dinero(1000m, 1);
        var resultado = d.Restar(300m);

        resultado.Importe.Should().Be(700m);
        resultado.MonedaId.Should().Be(1);
        d.Importe.Should().Be(1000m); // inmutable
    }

    [Fact]
    public void Multiplicar_DebeRetornarNuevoObjeto()
    {
        var d = new Dinero(100m, 1);
        var resultado = d.Multiplicar(3m);

        resultado.Importe.Should().Be(300m);
        d.Importe.Should().Be(100m); // inmutable
    }

    [Fact]
    public void Cero_RetornaDineroConImporteCero()
    {
        var d = Dinero.Cero(2);
        d.Importe.Should().Be(0m);
        d.MonedaId.Should().Be(2);
    }

    [Fact]
    public void Igualdad_DineroConMismaMonedaYImporte_SonIguales()
    {
        var d1 = new Dinero(500m, 1);
        var d2 = new Dinero(500m, 1);
        d1.Should().Be(d2);
    }

    [Fact]
    public void Igualdad_DineroConDistintaMoneda_SonDistintos()
    {
        var d1 = new Dinero(500m, 1);
        var d2 = new Dinero(500m, 2);
        d1.Should().NotBe(d2);
    }

    [Fact]
    public void ToString_DevuelveFormatoConImporteYMoneda()
    {
        var d = new Dinero(1234.56m, 3);

        d.ToString().Should().Be($"{1234.56m:N2} (Moneda: 3)");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Domicilio — métodos de fábrica no cubiertos (Vacio, Crear)
// ─────────────────────────────────────────────────────────────────────────────

public class DomicilioExtendedTests
{
    [Fact]
    public void Vacio_RetornaDomicilioConTodoNulo()
    {
        var d = Domicilio.Vacio();

        d.Calle.Should().BeNull();
        d.Nro.Should().BeNull();
        d.CodigoPostal.Should().BeNull();
        d.LocalidadId.Should().BeNull();
    }

    [Fact]
    public void Crear_EsEquivalenteAlConstructorDirecto()
    {
        var d1 = Domicilio.Crear("San Martín", "100", null, null, "5000", 1, null);
        var d2 = new Domicilio("San Martín", "100", null, null, "5000", 1, null);

        d1.Should().Be(d2);
    }

    [Fact]
    public void Completo_SinPisoNiDpto_OmiteCamposNulos()
    {
        var d = new Domicilio("Belgrano", "456", null, null, "5000", 1, null);
        d.Completo.Should().Be("Belgrano 456");
    }

    [Fact]
    public void Completo_TodosLoscampos_IncluirTodos()
    {
        var d = new Domicilio("San Martín", "123", "2", "A", "5000", 1, null);
        d.Completo.Should().Be("San Martín 123 2 A");
    }
}
public class ValueObjectOperatorTests
{
    [Fact]
    public void Igualdad_AmbosNull_DebeSerTrue()
    {
        Dinero? left = null;
        Dinero? right = null;

        (left == right).Should().BeTrue();
    }

    [Fact]
    public void Igualdad_IzquierdoNull_DebeSerFalse()
    {
        Dinero? left = null;
        var right = new Dinero(100m, 1L);

        (left == right).Should().BeFalse();
    }

    [Fact]
    public void Igualdad_DerechoNull_DebeSerFalse()
    {
        var left = new Dinero(100m, 1L);
        Dinero? right = null;

        (left == right).Should().BeFalse();
    }

    [Fact]
    public void Desigualdad_ValoresDistintos_DebeSerTrue()
    {
        var d1 = new Dinero(100m, 1L);
        var d2 = new Dinero(200m, 1L);

        (d1 != d2).Should().BeTrue();
    }

    [Fact]
    public void Desigualdad_ValoresIguales_DebeSerFalse()
    {
        var d1 = new Dinero(100m, 1L);
        var d2 = new Dinero(100m, 1L);

        (d1 != d2).Should().BeFalse();
    }
}