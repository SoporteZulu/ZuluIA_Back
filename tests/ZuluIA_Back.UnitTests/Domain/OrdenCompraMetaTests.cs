using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class OrdenCompraMetaTests
{
    [Fact]
    public void Crear_ConCantidadValida_DebeIniciarPendienteYSinRecepciones()
    {
        var orden = OrdenCompraMeta.Crear(1, 2, null, "Entrega parcial", 10m);

        orden.EstadoOc.Should().Be(EstadoOrdenCompra.Pendiente);
        orden.CantidadTotal.Should().Be(10m);
        orden.CantidadRecibida.Should().Be(0m);
    }

    [Fact]
    public void RegistrarRecepcion_Parcial_DebeActualizarEstadoYSaldo()
    {
        var orden = OrdenCompraMeta.Crear(1, 2, null, null, 10m);

        orden.RegistrarRecepcion(4m, DateOnly.FromDateTime(DateTime.Today));

        orden.EstadoOc.Should().Be(EstadoOrdenCompra.ParcialmenteRecibida);
        orden.CantidadRecibida.Should().Be(4m);
    }

    [Fact]
    public void RegistrarRecepcion_Total_DebeMarcarRecibida()
    {
        var orden = OrdenCompraMeta.Crear(1, 2, null, null, 10m);

        orden.RegistrarRecepcion(10m, DateOnly.FromDateTime(DateTime.Today));

        orden.EstadoOc.Should().Be(EstadoOrdenCompra.Recibida);
        orden.CantidadRecibida.Should().Be(10m);
    }

    [Fact]
    public void RegistrarRecepcion_SuperandoSaldo_DebeLanzarExcepcion()
    {
        var orden = OrdenCompraMeta.Crear(1, 2, null, null, 10m);
        orden.RegistrarRecepcion(7m, DateOnly.FromDateTime(DateTime.Today));

        var act = () => orden.RegistrarRecepcion(4m, DateOnly.FromDateTime(DateTime.Today));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*saldo pendiente*");
    }
}
