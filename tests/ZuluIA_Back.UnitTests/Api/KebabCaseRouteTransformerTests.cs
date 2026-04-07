using FluentAssertions;
using Xunit;
using ZuluIA_Back.Api.Utilities;

namespace ZuluIA_Back.UnitTests.Api;

public class KebabCaseRouteTransformerTests
{
    private readonly KebabCaseRouteTransformer _transformer = new();

    // -----------------------------------------------------------
    // null / non-string → null
    // -----------------------------------------------------------

    [Fact]
    public void TransformOutbound_ValorNulo_RetornaNulo()
    {
        var result = _transformer.TransformOutbound(null);
        result.Should().BeNull();
    }

    [Fact]
    public void TransformOutbound_ValorNoString_RetornaNulo()
    {
        var result = _transformer.TransformOutbound(42);
        result.Should().BeNull();
    }

    // -----------------------------------------------------------
    // PascalCase → kebab-case
    // -----------------------------------------------------------

    [Theory]
    [InlineData("OrdenesTrabajos",       "ordenes-trabajos")]
    [InlineData("PlanCuentas",           "plan-cuentas")]
    [InlineData("ListasPrecios",         "listas-precios")]
    [InlineData("CuentaCorriente",       "cuenta-corriente")]
    [InlineData("OrdenesPreparacion",    "ordenes-preparacion")]
    [InlineData("PuntosFacturacion",     "puntos-facturacion")]
    [InlineData("CategoriasItems",       "categorias-items")]
    [InlineData("DescuentosComerciales", "descuentos-comerciales")]
    [InlineData("MovimientosStock",      "movimientos-stock")]
    [InlineData("PeriodosContables",     "periodos-contables")]
    public void TransformOutbound_PascalCase_ConvierteAKebabCase(string input, string expected)
    {
        var result = _transformer.TransformOutbound(input);
        result.Should().Be(expected);
    }

    // -----------------------------------------------------------
    // Ya en minúsculas → sin cambios (sin guiones extra)
    // -----------------------------------------------------------

    [Theory]
    [InlineData("items",      "items")]
    [InlineData("cajas",      "cajas")]
    [InlineData("pagos",      "pagos")]
    [InlineData("terceros",   "terceros")]
    [InlineData("usuarios",   "usuarios")]
    public void TransformOutbound_YaEnMinusculas_RetornaSinCambios(string input, string expected)
    {
        var result = _transformer.TransformOutbound(input);
        result.Should().Be(expected);
    }

    // -----------------------------------------------------------
    // Palabra única PascalCase → solo minúsculas
    // -----------------------------------------------------------

    [Fact]
    public void TransformOutbound_PalabraUnica_SoloMinusculas()
    {
        // "Items" → "items" (single word, no transition between lower→upper)
        var result = _transformer.TransformOutbound("Items");
        result.Should().Be("items");
    }

    // -----------------------------------------------------------
    // Cadena vacía → cadena vacía
    // -----------------------------------------------------------

    [Fact]
    public void TransformOutbound_CadenaVacia_RetornaCadenaVacia()
    {
        var result = _transformer.TransformOutbound("");
        result.Should().Be("");
    }

    // -----------------------------------------------------------
    // Resultado siempre en minúsculas
    // -----------------------------------------------------------

    [Fact]
    public void TransformOutbound_ResultadoSiempreEnMinusculas()
    {
        var result = _transformer.TransformOutbound("LibroIva");
        result.Should().Be(result!.ToLowerInvariant());
    }
}
