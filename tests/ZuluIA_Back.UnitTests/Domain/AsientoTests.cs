using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class AsientoTests
{
    private static Asiento CrearAsiento() =>
        Asiento.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1,
            "Venta Factura A", "comprobantes", 1, null);

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearEnEstadoBorrador()
    {
        var asiento = CrearAsiento();

        asiento.Estado.Should().Be(EstadoAsiento.Borrador);
        asiento.Lineas.Should().BeEmpty();
    }

    [Fact]
    public void AgregarLinea_EnBorrador_DebeAgregarLinea()
    {
        var asiento = CrearAsiento();
        var linea = AsientoLinea.Crear(0, 1, 1000m, 0m, "Clientes", 1);

        asiento.AgregarLinea(linea);

        asiento.Lineas.Should().HaveCount(1);
        asiento.TotalDebe.Should().Be(1000m);
    }

    [Fact]
    public void Contabilizar_AsientoBalanceado_DebeContabilizar()
    {
        var asiento = CrearAsiento();
        asiento.AgregarLinea(AsientoLinea.Crear(0, 1, 1000m, 0m, "Clientes", 1));
        asiento.AgregarLinea(AsientoLinea.Crear(0, 2, 0m, 826.45m, "Ventas", 2));
        asiento.AgregarLinea(AsientoLinea.Crear(0, 3, 0m, 173.55m, "IVA", 3));

        asiento.Contabilizar(null);

        asiento.Estado.Should().Be(EstadoAsiento.Contabilizado);
    }

    [Fact]
    public void Contabilizar_AsientoNoBalanceado_DebeLanzarExcepcion()
    {
        var asiento = CrearAsiento();
        asiento.AgregarLinea(AsientoLinea.Crear(0, 1, 1000m, 0m, "Clientes", 1));
        asiento.AgregarLinea(AsientoLinea.Crear(0, 2, 0m, 500m, "Ventas", 2));

        var act = () => asiento.Contabilizar(null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no balancea*");
    }

    [Fact]
    public void Contabilizar_DespuesDeContabilizar_DebeLanzarExcepcion()
    {
        var asiento = CrearAsiento();
        asiento.AgregarLinea(AsientoLinea.Crear(0, 1, 1000m, 0m, "Debe", 1));
        asiento.AgregarLinea(AsientoLinea.Crear(0, 2, 0m, 1000m, "Haber", 2));
        asiento.Contabilizar(null);

        var act = () => asiento.Contabilizar(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Anular_AsientoContabilizado_DebeAnular()
    {
        var asiento = CrearAsiento();
        asiento.AgregarLinea(AsientoLinea.Crear(0, 1, 1000m, 0m, "Debe", 1));
        asiento.AgregarLinea(AsientoLinea.Crear(0, 2, 0m, 1000m, "Haber", 2));
        asiento.Contabilizar(null);

        asiento.Anular(null);

        asiento.Estado.Should().Be(EstadoAsiento.Anulado);
    }

    [Fact]
    public void AsientoLinea_ConDebeYHaber_DebeLanzarExcepcion()
    {
        var act = () => AsientoLinea.Crear(0, 1, 1000m, 500m, "Error", 1);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*debe y haber*");
    }

    [Fact]
    public void AsientoLinea_SinDebeNiHaber_DebeLanzarExcepcion()
    {
        var act = () => AsientoLinea.Crear(0, 1, 0m, 0m, "Error", 1);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void Balancea_AsientoEquilibrado_DebeRetornarTrue()
    {
        var asiento = CrearAsiento();
        asiento.AgregarLinea(AsientoLinea.Crear(0, 1, 1000m, 0m, "Debe", 1));
        asiento.AgregarLinea(AsientoLinea.Crear(0, 2, 0m, 1000m, "Haber", 2));

        asiento.Balancea.Should().BeTrue();
    }
}