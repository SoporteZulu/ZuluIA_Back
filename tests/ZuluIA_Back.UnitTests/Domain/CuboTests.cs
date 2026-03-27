using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.BI;

namespace ZuluIA_Back.UnitTests.Domain;

public class CuboTests
{
    // ── Crear ────────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDescripcionValida_DebeCrearCubo()
    {
        var cubo = Cubo.Crear("Ventas por período");

        cubo.Descripcion.Should().Be("Ventas por período");
        cubo.EsSistema.Should().BeFalse();
        cubo.AmbienteId.Should().Be(1);
    }

    [Fact]
    public void Crear_DescripcionConEspacios_DebeRecortarse()
    {
        var cubo = Cubo.Crear("  Ventas  ");

        cubo.Descripcion.Should().Be("Ventas");
    }

    [Fact]
    public void Crear_DescripcionVacia_DebeLanzarArgumentException()
    {
        var act = () => Cubo.Crear("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionNula_DebeLanzarArgumentException()
    {
        var act = () => Cubo.Crear("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ConOrigenDatosYObservacion_DebeAsignarCampos()
    {
        var cubo = Cubo.Crear("Inventario", "SELECT * FROM StockItem", "Obs test");

        cubo.OrigenDatos.Should().Be("SELECT * FROM StockItem");
        cubo.Observacion.Should().Be("Obs test");
    }

    [Fact]
    public void Crear_EsSistemaTrue_DebeAsignarEsSistema()
    {
        var cubo = Cubo.Crear("Cubo base", esSistema: true);

        cubo.EsSistema.Should().BeTrue();
    }

    // ── Actualizar ───────────────────────────────────────────────────────────

    [Fact]
    public void Actualizar_ConDatosValidos_DebeActualizarCampos()
    {
        var cubo = Cubo.Crear("Ventas");

        cubo.Actualizar("Ventas por región", "SELECT ...", "Nueva obs", 2, null, null);

        cubo.Descripcion.Should().Be("Ventas por región");
        cubo.OrigenDatos.Should().Be("SELECT ...");
        cubo.Observacion.Should().Be("Nueva obs");
        cubo.AmbienteId.Should().Be(2);
    }

    [Fact]
    public void Actualizar_DescripcionVacia_DebeLanzarArgumentException()
    {
        var cubo = Cubo.Crear("Ventas");

        var act = () => cubo.Actualizar("", null, null, 1, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_DescripcionConEspacios_DebeRecortarse()
    {
        var cubo = Cubo.Crear("Ventas");

        cubo.Actualizar("  Compras  ", null, null, 1, null, null);

        cubo.Descripcion.Should().Be("Compras");
    }
}
