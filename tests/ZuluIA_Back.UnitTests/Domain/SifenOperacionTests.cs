using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class SifenOperacionTests
{
    [Fact]
    public void Confirmar_DebeCambiarEstado()
    {
        var operacion = SifenOperacion.Registrar(1, 1, 1, 1, null, null, null, null, false, null);

        operacion.Confirmar("ok", null);

        operacion.Estado.Should().Be(EstadoIntegracionFiscalAlternativa.Confirmada);
    }
}
