using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.UnitTests.Domain;

public class ContratoPresupuestoTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static Contrato ContratoValido() => Contrato.Crear(
        terceroId: 1,
        sucursalTerceroId: null,
        vendedorId: null,
        tipoComprobanteId: null,
        puntoFacturacionId: null,
        condicionVentaId: null,
        monedaId: 1,
        cotizacion: 1m,
        fechaDesde: Hoy,
        fechaVencimiento: Hoy.AddMonths(12),
        fechaInicioFacturacion: Hoy,
        periodoMeses: 1,
        duracion: 12,
        total: 5000m,
        observacion: null);

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearContrato()
    {
        var contrato = ContratoValido();

        contrato.Should().NotBeNull();
        contrato.TerceroId.Should().Be(1);
        contrato.Estado.Should().Be("VIGENTE");
        contrato.Anulado.Should().BeFalse();
        contrato.PeriodoMeses.Should().Be(1);
        contrato.CuotasRestantes.Should().Be(12);
        contrato.Total.Should().Be(5000m);
    }

    [Fact]
    public void Crear_CotizacionNegativa_DebeUsarUno()
    {
        var contrato = Contrato.Crear(1, null, null, null, null, null, 1, -1m,
            Hoy, Hoy.AddMonths(1), Hoy, 1, 1, 100m, null);

        contrato.Cotizacion.Should().Be(1m);
    }

    [Fact]
    public void Crear_TerceroIdCero_DebeArrojarExcepcion()
    {
        var act = () => Contrato.Crear(0, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddMonths(1), Hoy, 1, 1, 100m, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*tercero*");
    }

    [Fact]
    public void Crear_FechaVencimientoAnteriorAFechaDesde_DebeArrojarExcepcion()
    {
        var act = () => Contrato.Crear(1, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddDays(-1), Hoy, 1, 1, 100m, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*vencimiento*");
    }

    [Fact]
    public void Crear_PeriodoMesesCero_DebeArrojarExcepcion()
    {
        var act = () => Contrato.Crear(1, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddMonths(1), Hoy, 0, 1, 100m, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*período*");
    }

    [Fact]
    public void Crear_TotalNegativo_DebeArrojarExcepcion()
    {
        var act = () => Contrato.Crear(1, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddMonths(1), Hoy, 1, 1, -1m, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*total*");
    }

    [Fact]
    public void Anular_ContratoVigente_DebeAnularYCambiarEstado()
    {
        var contrato = ContratoValido();

        contrato.Anular("Cliente solicitó cancelación");

        contrato.Anulado.Should().BeTrue();
        contrato.Estado.Should().Be("ANULADO");
    }

    [Fact]
    public void Anular_ContratoYaAnulado_DebeArrojarExcepcion()
    {
        var contrato = ContratoValido();
        contrato.Anular();

        var act = () => contrato.Anular();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*anulado*");
    }

    [Fact]
    public void MarcarVencido_ContratoVigente_DebeCambiarEstado()
    {
        var contrato = ContratoValido();

        contrato.MarcarVencido();

        contrato.Estado.Should().Be("VENCIDO");
        contrato.Anulado.Should().BeFalse();
    }

    [Fact]
    public void MarcarVencido_ContratoAnulado_NoDebeModificarEstado()
    {
        var contrato = ContratoValido();
        contrato.Anular();

        contrato.MarcarVencido();

        contrato.Estado.Should().Be("ANULADO");
    }

    [Fact]
    public void RegistrarFacturacion_DebeDecrementarCuotasRestantes()
    {
        var contrato = ContratoValido();
        var cuotasIniciales = contrato.CuotasRestantes;

        contrato.RegistrarFacturacion();

        contrato.CuotasRestantes.Should().Be(cuotasIniciales - 1);
    }

    [Fact]
    public void RegistrarFacturacion_UltimaCuota_DebeMarcarVencido()
    {
        var contrato = Contrato.Crear(1, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddMonths(1), Hoy, 1, 1, 100m, null);

        contrato.RegistrarFacturacion();

        contrato.CuotasRestantes.Should().Be(0);
        contrato.Estado.Should().Be("VENCIDO");
    }

    [Fact]
    public void Actualizar_ContratoAnulado_DebeArrojarExcepcion()
    {
        var contrato = ContratoValido();
        contrato.Anular();

        var act = () => contrato.Actualizar(null, null, Hoy.AddMonths(1), 1, 12, 5000m, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*anulado*");
    }

    [Fact]
    public void AgregarDetalle_ConDatosValidos_DebeAgregarDetalle()
    {
        var contrato = ContratoValido();

        var detalle = contrato.AgregarDetalle(
            itemId: 1,
            descripcion: "Servicio mensual",
            cantidad: 1,
            precioUnitario: 500m,
            porcentajeIva: 21m,
            fechaDesde: Hoy,
            fechaHasta: Hoy.AddMonths(12),
            fechaPrimeraFactura: Hoy,
            frecuenciaMeses: 1,
            corte: 31,
            dominio: null);

        contrato.Detalles.Should().HaveCount(1);
        detalle.Descripcion.Should().Be("Servicio mensual");
        detalle.Total.Should().Be(500m);
    }

    [Fact]
    public void AgregarDetalle_DescripcionVacia_DebeArrojarExcepcion()
    {
        var contrato = ContratoValido();

        var act = () => contrato.AgregarDetalle(null, "", 1, 100m, null, Hoy,
            Hoy.AddMonths(1), Hoy, 1, 1, null);

        act.Should().Throw<ArgumentException>();
    }
}

public class ContratoDetalleTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static Contrato ContratoConDetalle()
    {
        var c = Contrato.Crear(1, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddMonths(12), Hoy, 1, 12, 5000m, null);
        c.AgregarDetalle(1, "Servicio", 2, 100m, 21m, Hoy, Hoy.AddMonths(12), Hoy, 1, 1, "ABC123");
        return c;
    }

    [Fact]
    public void AgregarDetalle_DominioDebe_SerNormalizado()
    {
        var contrato = ContratoConDetalle();
        contrato.Detalles.First().Dominio.Should().Be("ABC123");
    }

    [Fact]
    public void AgregarDetalle_CantidadCero_DebeArrojarExcepcion()
    {
        var c = Contrato.Crear(1, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddMonths(1), Hoy, 1, 1, 100m, null);

        var act = () => c.AgregarDetalle(1, "Servicio", 0, 100m, null, Hoy,
            Hoy.AddMonths(1), Hoy, 1, 1, null);

        act.Should().Throw<ArgumentException>().WithMessage("*cantidad*");
    }

    [Fact]
    public void AgregarDetalle_PrecioNegativo_DebeArrojarExcepcion()
    {
        var c = Contrato.Crear(1, null, null, null, null, null, 1, 1m,
            Hoy, Hoy.AddMonths(1), Hoy, 1, 1, 100m, null);

        var act = () => c.AgregarDetalle(1, "Servicio", 1, -10m, null, Hoy,
            Hoy.AddMonths(1), Hoy, 1, 1, null);

        act.Should().Throw<ArgumentException>().WithMessage("*precio*");
    }

    [Fact]
    public void Anular_MarcaDetalleComoAnuladoYCambiaEstado()
    {
        var c = ContratoConDetalle();
        var detalle = c.Detalles.First();

        detalle.Anular();

        detalle.Anulado.Should().BeTrue();
        detalle.Estado.Should().Be("ANULADO");
    }
}

public class PresupuestoTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static Presupuesto PresupuestoValido() => Presupuesto.Crear(
        sucursalId: 1,
        terceroId: 1,
        fecha: Hoy,
        fechaVigencia: Hoy.AddDays(30),
        monedaId: 1,
        cotizacion: 1m,
        observacion: null,
        userId: null);

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearPresupuesto()
    {
        var p = PresupuestoValido();

        p.Should().NotBeNull();
        p.Estado.Should().Be("PENDIENTE");
        p.Cotizacion.Should().Be(1m);
        p.ComprobanteId.Should().BeNull();
    }

    [Fact]
    public void Crear_CotizacionCero_DebeUsarUno()
    {
        var p = Presupuesto.Crear(1, 1, Hoy, null, 1, 0m, null, null);

        p.Cotizacion.Should().Be(1m);
    }

    [Fact]
    public void Aprobar_PresupuestoPendiente_DebeMarcarAprobado()
    {
        var p = PresupuestoValido();

        p.Aprobar(userId: 1);

        p.Estado.Should().Be("APROBADO");
    }

    [Fact]
    public void Rechazar_PresupuestoPendiente_DebeMarcarRechazado()
    {
        var p = PresupuestoValido();

        p.Rechazar(userId: null);

        p.Estado.Should().Be("RECHAZADO");
    }

    [Fact]
    public void Convertir_PresupuestoAprobado_DebeAsignarComprobanteYEstado()
    {
        var p = PresupuestoValido();
        p.Aprobar(1);

        p.Convertir(comprobanteId: 42, userId: 1);

        p.ComprobanteId.Should().Be(42);
        p.Estado.Should().Be("CONVERTIDO");
    }

    [Fact]
    public void ActualizarTotal_DebeActualizarValor()
    {
        var p = PresupuestoValido();

        p.ActualizarTotal(9999m, userId: 1);

        p.Total.Should().Be(9999m);
    }

    [Fact]
    public void Eliminar_DebeMarcarComoEliminado()
    {
        var p = PresupuestoValido();

        p.Eliminar();

        p.IsDeleted.Should().BeTrue();
    }
}
