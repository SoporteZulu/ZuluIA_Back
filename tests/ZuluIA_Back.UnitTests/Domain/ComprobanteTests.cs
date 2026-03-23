using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Comprobantes;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteTests
{
    private static Comprobante CrearComprobante() =>
        Comprobante.Crear(1, 1, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, string.Empty, null);

    private static ComprobanteItem CrearItem(long precio = 1000, long iva = 21) =>
        ComprobanteItem.Crear(0, 1, "Producto Test", 1m, 0, precio, 0m, 1, iva, null, (short)1);

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearEnEstadoBorrador()
    {
        var comp = CrearComprobante();

        comp.Estado.Should().Be(EstadoComprobante.Borrador);
        comp.Items.Should().BeEmpty();
        comp.Total.Should().Be(0m);
    }

    [Fact]
    public void AgregarItem_EnBorrador_DebeAgregarItem()
    {
        var comp = CrearComprobante();
        var item = CrearItem();

        comp.AgregarItem(item);

        comp.Items.Should().HaveCount(1);
        comp.NetoGravado.Should().BeGreaterThan(0);
        comp.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AgregarItem_DespuesDeEmitir_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);

        var act = () => comp.AgregarItem(CrearItem());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Emitir_ConItems_DebeEmitirYGenerarEvento()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.ClearDomainEvents();

        comp.Emitir(null);

        comp.Estado.Should().Be(EstadoComprobante.Emitido);
        comp.Saldo.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Emitir_SinItems_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();

        var act = () => comp.Emitir(null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*sin *tems*");
    }

    [Fact]
    public void Emitir_ComprobanteYaEmitido_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);

        var act = () => comp.Emitir(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Anular_ComprobanteEmitido_DebeAnularYGenerarEvento()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);
        comp.ClearDomainEvents();

        comp.Anular(null);

        comp.Estado.Should().Be(EstadoComprobante.Anulado);
        comp.Saldo.Should().Be(0m);
    }

    [Fact]
    public void Anular_ComprobanteYaAnulado_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);
        comp.Anular(null);

        var act = () => comp.Anular(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AplicarPago_PagoTotal_DebeDejarSaldoCeroYEstadoPagado()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        comp.Emitir(null);

        comp.AplicarPago(comp.Total);

        comp.Saldo.Should().Be(0m);
        comp.Estado.Should().Be(EstadoComprobante.Pagado);
    }

    [Fact]
    public void AplicarPago_PagoParcial_DebeDejarSaldoPositivoYEstadoParcial()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        comp.Emitir(null);

        comp.AplicarPago(comp.Total / 2);

        comp.Saldo.Should().BeGreaterThan(0);
        comp.Estado.Should().Be(EstadoComprobante.PagadoParcial);
    }

    // -- MarcarComoConvertido / SetComprobanteOrigen ---------------------------

    [Fact]
    public void MarcarComoConvertido_DesdeBorrador_DebePonerEstadoConvertido()
    {
        var comp = CrearComprobante();

        comp.MarcarComoConvertido(null);

        comp.Estado.Should().Be(EstadoComprobante.Convertido);
    }

    [Fact]
    public void MarcarComoConvertido_DesdeEmitido_DebePonerEstadoConvertido()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);

        comp.MarcarComoConvertido(null);

        comp.Estado.Should().Be(EstadoComprobante.Convertido);
    }

    [Fact]
    public void MarcarComoConvertido_DesdeAnulado_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.Anular(null);

        var act = () => comp.MarcarComoConvertido(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Borrador o Emitido*");
    }

    [Fact]
    public void SetComprobanteOrigen_DebeAsignarComprobanteOrigenId()
    {
        var comp = CrearComprobante();

        comp.SetComprobanteOrigen(42L);

        comp.ComprobanteOrigenId.Should().Be(42L);
    }

    // -- AsignarCae ------------------------------------------------------------

    [Fact]
    public void AsignarCae_CaeValido_AsignaValores()
    {
        var comp = CrearComprobante();
        var fechaVto = new DateOnly(2026, 6, 30);

        comp.AsignarCae("12345678901234", fechaVto, "qrdata", null);

        comp.Cae.Should().Be("12345678901234");
        comp.FechaVtoCae.Should().Be(fechaVto);
        comp.QrData.Should().Be("qrdata");
    }

    [Fact]
    public void AsignarCae_CaeVacio_LanzaExcepcion()
    {
        var comp = CrearComprobante();

        var act = () => comp.AsignarCae("   ", new DateOnly(2026, 6, 30), null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegistrarResultadoSifen_Aceptado_PersisteEstadoYTracking()
    {
        var comp = CrearComprobante();
        var fecha = new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero);

        comp.RegistrarResultadoSifen(EstadoSifenParaguay.Aceptado, null, null, "SIFEN-123", "CDC-001", "LOTE-88", fecha, null);

        comp.EstadoSifen.Should().Be(EstadoSifenParaguay.Aceptado);
        comp.SifenCodigoRespuesta.Should().BeNull();
        comp.SifenMensajeRespuesta.Should().BeNull();
        comp.SifenTrackingId.Should().Be("SIFEN-123");
        comp.SifenCdc.Should().Be("CDC-001");
        comp.SifenNumeroLote.Should().Be("LOTE-88");
        comp.SifenFechaRespuesta.Should().Be(fecha);
    }

    [Fact]
    public void RegistrarResultadoSifen_Error_SoportaTrackingNulo()
    {
        var comp = CrearComprobante();

        comp.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "Timeout", null, null, null, null, null);

        comp.EstadoSifen.Should().Be(EstadoSifenParaguay.Error);
        comp.SifenCodigoRespuesta.Should().Be("E001");
        comp.SifenMensajeRespuesta.Should().Be("Timeout");
        comp.SifenTrackingId.Should().BeNull();
        comp.SifenCdc.Should().BeNull();
        comp.SifenNumeroLote.Should().BeNull();
        comp.SifenFechaRespuesta.Should().NotBeNull();
    }

    // -- ActualizarSaldo -------------------------------------------------------

    [Fact]
    public void ActualizarSaldo_ImputacionTotal_DejaEstadoPagado()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        comp.Emitir(null);
        var total = comp.Total;

        comp.ActualizarSaldo(total, null);

        comp.Saldo.Should().Be(0);
        comp.Estado.Should().Be(EstadoComprobante.Pagado);
    }

    [Fact]
    public void ActualizarSaldo_ImputacionParcial_DejaEstadoPagadoParcial()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        comp.Emitir(null);

        comp.ActualizarSaldo(comp.Total / 2, null);

        comp.Saldo.Should().BeGreaterThan(0);
        comp.Estado.Should().Be(EstadoComprobante.PagadoParcial);
    }

    [Fact]
    public void ActualizarSaldo_ImputacionExcede_SaldoNoCaeNegativo()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        comp.Emitir(null);
        var total = comp.Total;

        comp.ActualizarSaldo(total * 2, null);

        comp.Saldo.Should().Be(0);
        comp.Estado.Should().Be(EstadoComprobante.Pagado);
    }

    // -- RemoverItem -----------------------------------------------------------

    [Fact]
    public void RemoverItem_EnBorrador_RemoveItemYRecalculaTotales()
    {
        var comp = CrearComprobante();
        var item = CrearItem(1000, 21);
        comp.AgregarItem(item);
        comp.Items.Should().HaveCount(1);
        var totalAntes = comp.Total;

        // Id es 0 para �tems creados en memoria (sin persistencia)
        comp.RemoverItem(0);

        comp.Items.Should().BeEmpty();
        comp.Total.Should().Be(0m);
        comp.Total.Should().BeLessThan(totalAntes);
    }

    [Fact]
    public void RemoverItem_ItemIdInexistente_NoFallaYMantieneTotales()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        var totalAntes = comp.Total;

        comp.RemoverItem(99999); // id que no existe

        comp.Items.Should().HaveCount(1);
        comp.Total.Should().Be(totalAntes);
    }

    [Fact]
    public void RemoverItem_SiNoEsBorrador_LanzaExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);

        var act = () => comp.RemoverItem(0);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*borrador*");
    }

    // -- SetPercepciones -------------------------------------------------------

    [Fact]
    public void SetPercepciones_EnBorrador_ActualizaPercepcionesYRecalculaTotal()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 0)); // sin IVA para simplificar
        var totalSinPercep = comp.Total;

        comp.SetPercepciones(100m, null);

        comp.Percepciones.Should().Be(100m);
        comp.Total.Should().Be(totalSinPercep + 100m);
    }

    [Fact]
    public void SetPercepciones_NoEsBorrador_LanzaExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);

        var act = () => comp.SetPercepciones(100m, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*borrador*");
    }

    // -- SetRetenciones --------------------------------------------------------

    [Fact]
    public void SetRetenciones_ActualizaRetencionesYDescuentaDelTotal()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 0)); // sin IVA para simplificar
        var totalSinRet = comp.Total;

        comp.SetRetenciones(50m, null);

        comp.Retenciones.Should().Be(50m);
        comp.Total.Should().Be(totalSinRet - 50m);
    }

    [Fact]
    public void SetRetenciones_NoValidaEstadoBorrador_FuncionaDesdeEmitido()
    {
        // SetRetenciones no valida estado; debe poder llamarse desde Emitido
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 0));
        comp.Emitir(null);

        var act = () => comp.SetRetenciones(50m, null);

        act.Should().NotThrow();
        comp.Retenciones.Should().Be(50m);
    }

    // -- SetFechaVencimiento ---------------------------------------------------

    [Fact]
    public void SetFechaVencimiento_AsignaFechaCorrecta()
    {
        var comp = CrearComprobante();
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        comp.SetFechaVencimiento(fecha, null);

        comp.FechaVencimiento.Should().Be(fecha);
    }

    // -- RecalcularTotales -----------------------------------------------------

    [Fact]
    public void RecalcularTotales_ConMultiplesItems_CalculaCorrectamente()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 0)); // neto 1000
        comp.AgregarItem(CrearItem(2000, 0)); // neto 2000

        comp.NetoGravado.Should().Be(3000m);
        comp.Total.Should().Be(3000m);
    }

    [Fact]
    public void RecalcularTotales_ConPercepcionesYRetenciones_TotalEsCorrecto()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 0)); // neto 1000
        comp.SetPercepciones(50m, null);
        comp.SetRetenciones(30m, null);

        // Total = NetoGravado(1000) + Percepciones(50) - Retenciones(30)
        comp.Total.Should().Be(1020m);
    }
}

public class ComprobanteDomainEventTests
{
    private static Comprobante CrearConItem()
    {
        var comp = Comprobante.Crear(1, 1, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        comp.AgregarItem(ComprobanteItem.Crear(0, 1, "Prod", 1m, 0, 1000L, 0m, 1, 21, null, 1));
        comp.ClearDomainEvents();
        return comp;
    }

    [Fact]
    public void Emitir_DebeRaisearComprobanteEmitidoEvent()
    {
        var comp = CrearConItem();

        comp.Emitir(null);

        comp.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ComprobanteEmitidoEvent>();
    }

    [Fact]
    public void Anular_DebeRaisearComprobanteAnuladoEvent()
    {
        var comp = CrearConItem();
        comp.Emitir(null);
        comp.ClearDomainEvents();

        comp.Anular(null);

        comp.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ComprobanteAnuladoEvent>();
    }
}