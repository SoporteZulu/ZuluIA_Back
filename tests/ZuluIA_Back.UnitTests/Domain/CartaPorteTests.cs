using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class CartaPorteTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static CartaPorte CrearCarta() =>
        CartaPorte.Crear(1, "20304050607", "20999999991", null, Hoy, null, null);

    [Fact]
    public void Crear_DebeIniciarPendiente()
    {
        var carta = CrearCarta();

        carta.Estado.Should().Be(EstadoCartaPorte.Pendiente);
        carta.IntentosCtg.Should().Be(0);
    }

    [Fact]
    public void AsignarOrdenCarga_DebePasarAOrdenCargaAsignada()
    {
        var carta = CrearCarta();

        carta.AsignarOrdenCarga(10, 20, "20303030303", "Orden creada", null);

        carta.Estado.Should().Be(EstadoCartaPorte.OrdenCargaAsignada);
        carta.OrdenCargaId.Should().Be(10);
        carta.TransportistaId.Should().Be(20);
    }

    [Fact]
    public void SolicitarCtg_DebeIncrementarIntentosYPasarASolicitado()
    {
        var carta = CrearCarta();
        carta.AsignarOrdenCarga(10, 20, null, null, null);

        carta.SolicitarCtg(Hoy, "Primer intento", null);

        carta.Estado.Should().Be(EstadoCartaPorte.CtgSolicitado);
        carta.IntentosCtg.Should().Be(1);
    }

    [Fact]
    public void RegistrarErrorCtg_DebePasarAError()
    {
        var carta = CrearCarta();
        carta.AsignarOrdenCarga(10, 20, null, null, null);
        carta.SolicitarCtg(Hoy, null, null);

        carta.RegistrarErrorCtg("Timeout AFIP", null, null);

        carta.Estado.Should().Be(EstadoCartaPorte.CtgError);
        carta.UltimoErrorCtg.Should().Be("Timeout AFIP");
    }

    [Fact]
    public void AsignarCtg_DespuesDeSolicitud_DebeQuedarActiva()
    {
        var carta = CrearCarta();
        carta.AsignarOrdenCarga(10, 20, null, null, null);
        carta.SolicitarCtg(Hoy, null, null);

        carta.AsignarCtg("123456", null);

        carta.Estado.Should().Be(EstadoCartaPorte.Activa);
        carta.NroCtg.Should().Be("123456");
    }
}
