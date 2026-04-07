using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.UnitTests.Domain;

public class TesoreriaCierreTests
{
    [Fact]
    public void RegistrarApertura_DebeCrearEventoApertura()
    {
        var cierre = TesoreriaCierre.RegistrarApertura(1, 2, 3, DateOnly.FromDateTime(DateTime.Today), 500m, "Inicio", null);

        cierre.EsApertura.Should().BeTrue();
        cierre.SaldoInformado.Should().Be(500m);
        cierre.SaldoSistema.Should().Be(500m);
    }

    [Fact]
    public void RegistrarCierre_DebeGuardarDetalleOperativo()
    {
        var cierre = TesoreriaCierre.RegistrarCierre(1, 2, 3, DateOnly.FromDateTime(DateTime.Today), 1000m, 950m, 700m, 250m, 4, "Fin", null);

        cierre.EsApertura.Should().BeFalse();
        cierre.TotalIngresos.Should().Be(700m);
        cierre.TotalEgresos.Should().Be(250m);
        cierre.CantidadMovimientos.Should().Be(4);
    }
}
