using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteAfipTests
{
    private static Comprobante CrearComprobante() =>
        Comprobante.Crear(1, 2, 3, 1, 10, DateOnly.FromDateTime(DateTime.Today), null, 4, 1, 1m, null, null);

    [Fact]
    public void AsignarCae_DebeActualizarEstadoAfipYQr()
    {
        var comprobante = CrearComprobante();

        comprobante.AsignarCae("12345678901234", DateOnly.FromDateTime(DateTime.Today.AddDays(10)), "qr", null);

        comprobante.Cae.Should().Be("12345678901234");
        comprobante.EstadoAfip.Should().Be(EstadoAfipWsfe.AutorizadoCae);
        comprobante.QrData.Should().Be("qr");
    }

    [Fact]
    public void AsignarCaea_DebeActualizarEstadoAfip()
    {
        var comprobante = CrearComprobante();

        comprobante.AsignarCaea("2025010001", DateOnly.FromDateTime(DateTime.Today.AddDays(15)), null);

        comprobante.Caea.Should().Be("2025010001");
        comprobante.EstadoAfip.Should().Be(EstadoAfipWsfe.AutorizadoCaea);
    }

    [Fact]
    public void RegistrarEstadoAfip_DebeGuardarEstadoYError()
    {
        var comprobante = CrearComprobante();

        comprobante.RegistrarEstadoAfip(EstadoAfipWsfe.Error, "AFIP down", DateTimeOffset.UtcNow, null);

        comprobante.EstadoAfip.Should().Be(EstadoAfipWsfe.Error);
        comprobante.UltimoErrorAfip.Should().Be("AFIP down");
        comprobante.FechaUltimaConsultaAfip.Should().NotBeNull();
    }
}
