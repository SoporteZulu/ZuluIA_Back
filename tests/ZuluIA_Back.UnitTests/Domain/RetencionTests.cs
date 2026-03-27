using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.UnitTests.Domain;

public class RetencionTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public void CrearEnPago_ConDatosValidos_DebeCrearRetencion()
    {
        var retencion = Retencion.CrearEnPago(
            pagoId: 1,
            tipo: "ganancias",
            importe: 150m,
            nroCertificado: "CERT-001",
            fecha: Hoy);

        retencion.PagoId.Should().Be(1);
        retencion.CobroId.Should().BeNull();
        retencion.Importe.Should().Be(150m);
        retencion.NroCertificado.Should().Be("CERT-001");
        retencion.Fecha.Should().Be(Hoy);
    }

    [Fact]
    public void CrearEnPago_DebeNormalizarTipoAMayusculas()
    {
        var retencion = Retencion.CrearEnPago(1, "ganancias", 100m, null, Hoy);

        retencion.Tipo.Should().Be("GANANCIAS");
    }

    [Fact]
    public void CrearEnCobro_ConDatosValidos_DebeCrearRetencion()
    {
        var retencion = Retencion.CrearEnCobro(
            cobroId: 5,
            tipo: "IVA",
            importe: 200m,
            nroCertificado: null,
            fecha: Hoy);

        retencion.CobroId.Should().Be(5);
        retencion.PagoId.Should().BeNull();
        retencion.Importe.Should().Be(200m);
    }

    [Fact]
    public void CrearEnPago_TipoVacio_DebeArrojarExcepcion()
    {
        var act = () => Retencion.CrearEnPago(1, "", 100m, null, Hoy);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CrearEnPago_TipoBlanco_DebeArrojarExcepcion()
    {
        var act = () => Retencion.CrearEnPago(1, "   ", 100m, null, Hoy);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CrearEnPago_ImporteCero_DebeArrojarExcepcion()
    {
        var act = () => Retencion.CrearEnPago(1, "GANANCIAS", 0m, null, Hoy);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void CrearEnPago_ImporteNegativo_DebeArrojarExcepcion()
    {
        var act = () => Retencion.CrearEnPago(1, "GANANCIAS", -50m, null, Hoy);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void CrearEnCobro_TipoVacio_DebeArrojarExcepcion()
    {
        var act = () => Retencion.CrearEnCobro(1, "", 100m, null, Hoy);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CrearEnPago_NroCertificadoDebe_SerTrimmed()
    {
        var retencion = Retencion.CrearEnPago(1, "IVA", 100m, "  CERT-001  ", Hoy);

        retencion.NroCertificado.Should().Be("CERT-001");
    }
}

public class RetencionRegimenTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearRegimen()
    {
        var regimen = RetencionRegimen.Crear(
            codigo: "RET-IVA",
            descripcion: "Retención de IVA",
            retencionId: 1,
            observacion: "Obs test");

        regimen.Should().NotBeNull();
        regimen.Descripcion.Should().Be("Retención de IVA");
        regimen.RetencionId.Should().Be(1);
        regimen.Observacion.Should().Be("Obs test");
    }

    [Fact]
    public void Crear_DebeNormalizarCodigoAMayusculas()
    {
        var regimen = RetencionRegimen.Crear("ret-iva", "Descripción", 1);

        regimen.Codigo.Should().Be("RET-IVA");
    }

    [Fact]
    public void Crear_CodigoVacio_DebeArrojarExcepcion()
    {
        var act = () => RetencionRegimen.Crear("", "Descripción", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_DebeArrojarExcepcion()
    {
        var act = () => RetencionRegimen.Crear("COD", "", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_RetencionIdCero_DebeArrojarExcepcion()
    {
        var act = () => RetencionRegimen.Crear("COD", "Descripcion", 0);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*retención*");
    }

    [Fact]
    public void ActualizarParametros_DebeActualizarTodosLosValores()
    {
        var regimen = RetencionRegimen.Crear("COD", "Desc", 1);

        regimen.ActualizarParametros(
            controlTipoComprobante: true,
            controlTipoComprobanteAplica: true,
            baseImponibleComposicion: "NETO",
            noImponible: 1000m,
            noImponibleAplica: true,
            baseImponiblePorcentaje: 100m,
            baseImponiblePorcentajeAplica: false,
            baseImponibleMinimo: 500m,
            baseImponibleMinimoAplica: true,
            baseImponibleMaximo: 999999m,
            baseImponibleMaximoAplica: false,
            retencionComposicion: "ALICUOTA",
            retencionMinimo: 10m,
            retencionMinimoAplica: true,
            retencionMaximo: 50000m,
            retencionMaximoAplica: false,
            alicuota: 6m,
            alicuotaAplica: true,
            alicuotaEscalaAplica: false,
            alicuotaConvenio: 3m,
            alicuotaConvenioAplica: false,
            observacion: "Nueva observación");

        regimen.ControlTipoComprobante.Should().BeTrue();
        regimen.NoImponible.Should().Be(1000m);
        regimen.Alicuota.Should().Be(6m);
        regimen.Observacion.Should().Be("Nueva observación");
    }
}
