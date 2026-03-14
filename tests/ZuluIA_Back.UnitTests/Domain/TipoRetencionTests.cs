using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.UnitTests.Domain;

public class TipoRetencionTests
{
    private static TipoRetencion CrearTipoValido() =>
        TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);

    // ── Crear ────────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearActivo()
    {
        var t = TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);

        t.Descripcion.Should().Be("Ganancias");
        t.Regimen.Should().Be("GANANCIAS");
        t.Activo.Should().BeTrue();
        t.Escalas.Should().BeEmpty();
    }

    [Fact]
    public void Crear_RegimenEnMinusculas_DebeNormalizarAMayusculas()
    {
        var t = TipoRetencion.Crear("IIBB Córdoba", "iibb córdoba", 0m, false, null, null, null);

        t.Regimen.Should().Be("IIBB CÓRDOBA");
    }

    [Fact]
    public void Crear_DescripcionConEspacios_DebeRecortarse()
    {
        var t = TipoRetencion.Crear("  Ganancias  ", "GANANCIAS", 0m, false, null, null, null);

        t.Descripcion.Should().Be("Ganancias");
    }

    [Fact]
    public void Crear_DescripcionVacia_DebeLanzarArgumentException()
    {
        var act = () => TipoRetencion.Crear("", "GANANCIAS", 0m, false, null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_RegimenVacio_DebeLanzarArgumentException()
    {
        var act = () => TipoRetencion.Crear("Ganancias", "", 0m, false, null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_MinimoNoImponibleNegativo_DebePonerEnCero()
    {
        var t = TipoRetencion.Crear("Ganancias", "GANANCIAS", -500m, false, null, null, null);

        t.MinimoNoImponible.Should().Be(0m);
    }

    // ── AgregarEscala ────────────────────────────────────────────────────────

    [Fact]
    public void AgregarEscala_DatosValidos_DebeAgregarEscala()
    {
        var t = CrearTipoValido();

        t.AgregarEscala("Tramo 1", 0m, 10_000m, 2.5m);

        t.Escalas.Should().HaveCount(1);
    }

    [Fact]
    public void AgregarEscala_ImporteDesdenegativo_DebeLanzarExcepcion()
    {
        var t = CrearTipoValido();

        var act = () => t.AgregarEscala("Tramo 1", -1m, 10_000m, 2.5m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*negativo*");
    }

    [Fact]
    public void AgregarEscala_ImporteHastaMenorQueDesde_DebeLanzarExcepcion()
    {
        var t = CrearTipoValido();

        var act = () => t.AgregarEscala("Tramo 1", 5_000m, 1_000m, 2.5m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor*");
    }

    [Fact]
    public void AgregarEscala_PorcentajeMayorA100_DebeLanzarExcepcion()
    {
        var t = CrearTipoValido();

        var act = () => t.AgregarEscala("Tramo 1", 0m, 10_000m, 101m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*100*");
    }

    [Fact]
    public void AgregarEscala_PorcentajeNegativo_DebeLanzarExcepcion()
    {
        var t = CrearTipoValido();

        var act = () => t.AgregarEscala("Tramo 1", 0m, 10_000m, -1m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoverEscalas_DebeVaciarColeccion()
    {
        var t = CrearTipoValido();
        t.AgregarEscala("Tramo 1", 0m, 10_000m, 2.5m);
        t.AgregarEscala("Tramo 2", 10_001m, 0m, 5m);

        t.RemoverEscalas();

        t.Escalas.Should().BeEmpty();
    }

    // ── CalcularImporte ──────────────────────────────────────────────────────

    [Fact]
    public void CalcularImporte_BaseMenorAMinimoNoImponible_DebeRetornarCero()
    {
        var t = TipoRetencion.Crear("Ganancias", "GANANCIAS", 1_000m, false, null, null, null);
        t.AgregarEscala("Tramo 1", 0m, 0m, 5m);

        var resultado = t.CalcularImporte(500m);

        resultado.Should().Be(0m);
    }

    [Fact]
    public void CalcularImporte_SinEscalas_DebeRetornarCero()
    {
        var t = CrearTipoValido();

        var resultado = t.CalcularImporte(10_000m);

        resultado.Should().Be(0m);
    }

    [Fact]
    public void CalcularImporte_ConEscalaAplicable_DebeCalcularCorrectamente()
    {
        // 5 % de (10_000 - 0 minimoNoImponible) = 500
        var t = TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);
        t.AgregarEscala("Tramo 1", 0m, 0m, 5m); // importeHasta = 0 significa "sin límite superior"

        var resultado = t.CalcularImporte(10_000m);

        resultado.Should().Be(500m);
    }

    [Fact]
    public void CalcularImporte_ConMinimoNoImponible_DebeDescontarMinimoAntesDeAplicarEscala()
    {
        // 10 % de (10_000 - 2_000) = 800
        var t = TipoRetencion.Crear("IIBB", "IIBB CBA", 2_000m, false, null, null, null);
        t.AgregarEscala("Tramo 1", 0m, 0m, 10m);

        var resultado = t.CalcularImporte(10_000m);

        resultado.Should().Be(800m);
    }

    // ── Dar_De_Baja ──────────────────────────────────────────────────────────

    [Fact]
    public void DarDeBaja_DebeInactivar()
    {
        var t = CrearTipoValido();

        t.Dar_De_Baja(null);

        t.Activo.Should().BeFalse();
        t.IsDeleted.Should().BeTrue();
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    [Fact]
    public void Actualizar_ConDatosValidos_DebeActualizarCampos()
    {
        var t = CrearTipoValido();

        t.Actualizar("Nuevo Nombre", "NUEVO REGIMEN", 500m, true, 1, 2, null);

        t.Descripcion.Should().Be("Nuevo Nombre");
        t.Regimen.Should().Be("NUEVO REGIMEN");
        t.MinimoNoImponible.Should().Be(500m);
        t.AcumulaPago.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_DebeLanzarExcepcion()
    {
        var t = CrearTipoValido();

        var act = () => t.Actualizar("", "GANANCIAS", 0m, false, null, null, null);

        act.Should().Throw<ArgumentException>();
    }
}
