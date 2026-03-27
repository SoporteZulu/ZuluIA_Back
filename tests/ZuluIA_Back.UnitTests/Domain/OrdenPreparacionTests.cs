using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class OrdenPreparacionTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static OrdenPreparacion CrearOrden() =>
        OrdenPreparacion.Crear(1L, null, null, Hoy, null, null);

    private static OrdenPreparacion CrearOrdenConDetalle()
    {
        var orden = CrearOrden();
        orden.AgregarDetalle(10L, 1L, 5m);
        return orden;
    }

    // ── Crear ────────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearEnEstadoPendiente()
    {
        var orden = CrearOrden();

        orden.Estado.Should().Be(EstadoOrdenPreparacion.Pendiente);
        orden.Detalles.Should().BeEmpty();
    }

    // ── AgregarDetalle ───────────────────────────────────────────────────────

    [Fact]
    public void AgregarDetalle_EnEstadoPendiente_DebeAgregarDetalle()
    {
        var orden = CrearOrden();

        orden.AgregarDetalle(10L, 1L, 5m);

        orden.Detalles.Should().HaveCount(1);
    }

    [Fact]
    public void AgregarDetalle_EnEstadoEnProceso_DebeAgregarDetalle()
    {
        var orden = CrearOrdenConDetalle();
        orden.IniciarPreparacion(null);

        orden.AgregarDetalle(20L, 1L, 3m);

        orden.Detalles.Should().HaveCount(2);
    }

    [Fact]
    public void AgregarDetalle_EnEstadoAnulada_DebeLanzarExcepcion()
    {
        var orden = CrearOrdenConDetalle();
        orden.Anular(null);

        var act = () => orden.AgregarDetalle(10L, 1L, 1m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*pendiente*");
    }

    [Fact]
    public void AgregarDetalle_EnEstadoCompletada_DebeLanzarExcepcion()
    {
        var orden = CrearOrdenConDetalle();
        orden.IniciarPreparacion(null);
        var detalle = orden.Detalles.Single();
        orden.RegistrarPicking(detalle.Id, detalle.Cantidad, null);
        orden.Confirmar(Hoy, null);

        var act = () => orden.AgregarDetalle(10L, 1L, 1m);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── IniciarPreparacion ───────────────────────────────────────────────────

    [Fact]
    public void IniciarPreparacion_DesdePendienteConDetalles_DebePonerEnProceso()
    {
        var orden = CrearOrdenConDetalle();

        orden.IniciarPreparacion(null);

        orden.Estado.Should().Be(EstadoOrdenPreparacion.EnProceso);
    }

    [Fact]
    public void IniciarPreparacion_SinDetalles_DebeLanzarExcepcion()
    {
        var orden = CrearOrden();

        var act = () => orden.IniciarPreparacion(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*sin detalles*");
    }

    [Fact]
    public void IniciarPreparacion_DesdeEstadoAnulada_DebeLanzarExcepcion()
    {
        var orden = CrearOrdenConDetalle();
        orden.Anular(null);

        var act = () => orden.IniciarPreparacion(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Pendiente*");
    }

    [Fact]
    public void IniciarPreparacion_DesdeEstadoEnProceso_DebeLanzarExcepcion()
    {
        var orden = CrearOrdenConDetalle();
        orden.IniciarPreparacion(null);

        var act = () => orden.IniciarPreparacion(null);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Confirmar ────────────────────────────────────────────────────────────

    [Fact]
    public void Confirmar_DesdeEnProceso_DebePonerCompletada()
    {
        var orden = CrearOrdenConDetalle();
        orden.IniciarPreparacion(null);
        var detalle = orden.Detalles.Single();
        orden.RegistrarPicking(detalle.Id, detalle.Cantidad, null);

        orden.Confirmar(Hoy, null);

        orden.Estado.Should().Be(EstadoOrdenPreparacion.Completada);
        orden.FechaConfirmacion.Should().Be(Hoy);
    }

    [Fact]
    public void Confirmar_DesdeEstadoPendiente_DebeLanzarExcepcion()
    {
        var orden = CrearOrdenConDetalle();

        var act = () => orden.Confirmar(Hoy, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*En Proceso*");
    }

    // ── Anular ───────────────────────────────────────────────────────────────

    [Fact]
    public void Anular_DesdePendiente_DebeAnularse()
    {
        var orden = CrearOrden();

        orden.Anular(null);

        orden.Estado.Should().Be(EstadoOrdenPreparacion.Anulada);
    }

    [Fact]
    public void Anular_DesdeEnProceso_DebeAnularse()
    {
        var orden = CrearOrdenConDetalle();
        orden.IniciarPreparacion(null);

        orden.Anular(null);

        orden.Estado.Should().Be(EstadoOrdenPreparacion.Anulada);
    }

    [Fact]
    public void Anular_DesdeCompletada_DebeLanzarExcepcion()
    {
        var orden = CrearOrdenConDetalle();
        orden.IniciarPreparacion(null);
        var detalle = orden.Detalles.Single();
        orden.RegistrarPicking(detalle.Id, detalle.Cantidad, null);
        orden.Confirmar(Hoy, null);

        var act = () => orden.Anular(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*completada*");
    }

    [Fact]
    public void Anular_YaAnulada_DebeLanzarExcepcion()
    {
        var orden = CrearOrden();
        orden.Anular(null);

        var act = () => orden.Anular(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*anulada*");
    }

    [Fact]
    public void Confirmar_ConPickingIncompleto_DebeLanzarExcepcion()
    {
        var orden = CrearOrdenConDetalle();
        orden.IniciarPreparacion(null);

        var act = () => orden.Confirmar(Hoy, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*picking incompleto*");
    }
}
