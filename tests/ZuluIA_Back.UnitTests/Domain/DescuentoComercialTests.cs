using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.UnitTests.Domain;

public class DescuentoComercialTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static DescuentoComercial CrearValido() =>
        DescuentoComercial.Crear(1L, 2L, Hoy, Hoy.AddMonths(3), 10m, null);

    // ── Crear ────────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_DebeCrear()
    {
        var d = CrearValido();

        d.TerceroId.Should().Be(1L);
        d.ItemId.Should().Be(2L);
        d.Porcentaje.Should().Be(10m);
        d.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Crear_PorcentajeCero_DebeLanzarExcepcion()
    {
        var act = () => DescuentoComercial.Crear(1L, 2L, Hoy, null, 0m, null);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*porcentaje*");
    }

    [Fact]
    public void Crear_PorcentajeNegativo_DebeLanzarExcepcion()
    {
        var act = () => DescuentoComercial.Crear(1L, 2L, Hoy, null, -5m, null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Crear_PorcentajeMayor100_DebeLanzarExcepcion()
    {
        var act = () => DescuentoComercial.Crear(1L, 2L, Hoy, null, 101m, null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Crear_FechaHastaAnteriorAFechaDesde_DebeLanzarExcepcion()
    {
        var act = () => DescuentoComercial.Crear(1L, 2L, Hoy, Hoy.AddDays(-1), 10m, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*FechaHasta*");
    }

    // ── ObtenerPorcentajeVigente ──────────────────────────────────────────────

    [Fact]
    public void ObtenerPorcentajeVigente_FechaEnRango_DebeRetornarPorcentaje()
    {
        var d = DescuentoComercial.Crear(1L, 2L, Hoy, Hoy.AddDays(30), 15m, null);

        var pct = d.ObtenerPorcentajeVigente(Hoy.AddDays(10));

        pct.Should().Be(15m);
    }

    [Fact]
    public void ObtenerPorcentajeVigente_FechaIgualADesde_DebeRetornarPorcentaje()
    {
        var d = CrearValido();

        var pct = d.ObtenerPorcentajeVigente(Hoy);

        pct.Should().Be(10m);
    }

    [Fact]
    public void ObtenerPorcentajeVigente_FechaAnteriorADesde_DebeRetornarCero()
    {
        var d = DescuentoComercial.Crear(1L, 2L, Hoy.AddDays(5), Hoy.AddDays(30), 10m, null);

        var pct = d.ObtenerPorcentajeVigente(Hoy);

        pct.Should().Be(0m);
    }

    [Fact]
    public void ObtenerPorcentajeVigente_FechaPosteriorAHasta_DebeRetornarCero()
    {
        var d = DescuentoComercial.Crear(1L, 2L, Hoy.AddDays(-30), Hoy.AddDays(-1), 10m, null);

        var pct = d.ObtenerPorcentajeVigente(Hoy);

        pct.Should().Be(0m);
    }

    [Fact]
    public void ObtenerPorcentajeVigente_SinFechaHasta_DebeSerSiempreVigente()
    {
        var d = DescuentoComercial.Crear(1L, 2L, Hoy.AddDays(-100), null, 20m, null);

        var pct = d.ObtenerPorcentajeVigente(Hoy.AddDays(999));

        pct.Should().Be(20m);
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    [Fact]
    public void Actualizar_ConDatosValidos_DebeActualizarCampos()
    {
        var d = CrearValido();
        var nuevaDesde = Hoy.AddDays(1);
        var nuevaHasta = Hoy.AddDays(60);

        d.Actualizar(nuevaDesde, nuevaHasta, 25m, null);

        d.FechaDesde.Should().Be(nuevaDesde);
        d.FechaHasta.Should().Be(nuevaHasta);
        d.Porcentaje.Should().Be(25m);
    }

    [Fact]
    public void Actualizar_FechaHastaAnteriorADesde_DebeLanzarExcepcion()
    {
        var d = CrearValido();

        var act = () => d.Actualizar(Hoy, Hoy.AddDays(-1), 10m, null);

        act.Should().Throw<ArgumentException>();
    }

    // ── Eliminar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Eliminar_DebeMarcarComoEliminado()
    {
        var d = CrearValido();

        d.Eliminar(null);

        d.IsDeleted.Should().BeTrue();
    }
}
