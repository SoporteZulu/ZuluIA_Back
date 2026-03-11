using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class OrdenTrabajoTests
{
    private static OrdenTrabajo CrearOrden() =>
        OrdenTrabajo.Crear(1, 1, 1, 2, DateOnly.FromDateTime(DateTime.Today), null, 5m, null, null);

    [Fact]
    public void Crear_DebeCrearEnEstadoPendiente()
    {
        var ot = CrearOrden();

        ot.Estado.Should().Be(EstadoOrdenTrabajo.Pendiente);
        ot.Cantidad.Should().Be(5m);
    }

    [Fact]
    public void Iniciar_DesdePendiente_DebePonerEnProceso()
    {
        var ot = CrearOrden();

        ot.Iniciar(null);

        ot.Estado.Should().Be(EstadoOrdenTrabajo.EnProceso);
    }

    [Fact]
    public void Iniciar_YaIniciada_DebeLanzarExcepcion()
    {
        var ot = CrearOrden();
        ot.Iniciar(null);

        var act = () => ot.Iniciar(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Finalizar_DesdeProceso_DebeFinalizar()
    {
        var ot = CrearOrden();
        ot.Iniciar(null);

        ot.Finalizar(DateOnly.FromDateTime(DateTime.Today), null);

        ot.Estado.Should().Be(EstadoOrdenTrabajo.Finalizada);
        ot.FechaFinReal.Should().NotBeNull();
    }

    [Fact]
    public void Cancelar_DesdePendiente_DebeCancelar()
    {
        var ot = CrearOrden();

        ot.Cancelar(null);

        ot.Estado.Should().Be(EstadoOrdenTrabajo.Cancelada);
    }

    [Fact]
    public void Cancelar_YaFinalizada_DebeLanzarExcepcion()
    {
        var ot = CrearOrden();
        ot.Iniciar(null);
        ot.Finalizar(DateOnly.FromDateTime(DateTime.Today), null);

        var act = () => ot.Cancelar(null);

        act.Should().Throw<InvalidOperationException>();
    }
}
