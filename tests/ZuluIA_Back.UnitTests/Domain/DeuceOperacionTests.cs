using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class DeuceOperacionTests
{
    [Fact]
    public void Registrar_DebeConfirmarEstado()
    {
        var operacion = DeuceOperacion.Registrar(1, 1, 1, "ref-1", null, null, null, true, null);
        operacion.Estado.Should().Be(EstadoIntegracionFiscalAlternativa.Confirmada);
        operacion.ReferenciaExterna.Should().Be("REF-1");
    }

    [Fact]
    public void Rechazar_DebeCambiarEstado()
    {
        var operacion = DeuceOperacion.Registrar(1, 1, 1, "ref-1", null, null, null, false, null);

        operacion.Rechazar("error", null);

        operacion.Estado.Should().Be(EstadoIntegracionFiscalAlternativa.Rechazada);
    }
}
