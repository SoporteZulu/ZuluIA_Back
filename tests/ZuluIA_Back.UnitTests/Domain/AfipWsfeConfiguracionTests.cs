using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.UnitTests.Domain;

public class AfipWsfeConfiguracionTests
{
    [Fact]
    public void Crear_DebePersistirDatosBasicos()
    {
        var config = AfipWsfeConfiguracion.Crear(1, 2, true, false, true, "20123456789", "cert", "obs", null);

        config.SucursalId.Should().Be(1);
        config.PuntoFacturacionId.Should().Be(2);
        config.UsaCaeaPorDefecto.Should().BeTrue();
    }
}
