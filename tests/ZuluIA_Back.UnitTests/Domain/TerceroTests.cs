using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Events.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.UnitTests.Domain;

public class TerceroTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearTercero()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Distribuidora El Sur SRL",
            1, "30-22222222-2", 1,
            true, false, 1, null);

        tercero.Legajo.Should().Be("CLI001");
        tercero.RazonSocial.Should().Be("Distribuidora El Sur SRL");
        tercero.EsCliente.Should().BeTrue();
        tercero.EsProveedor.Should().BeFalse();
        tercero.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_LegajoConEspacios_DebeNormalizarse()
    {
        var tercero = Tercero.Crear(
            "  cli001  ", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, null, null);

        tercero.Legajo.Should().Be("CLI001");
    }

    [Fact]
    public void Crear_LegajoVacio_DebeLanzarExcepcion()
    {
        var act = () => Tercero.Crear(
            "", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_RazonSocialVacia_DebeLanzarExcepcion()
    {
        var act = () => Tercero.Crear(
            "CLI001", "",
            1, "30-11111111-1", 1,
            true, false, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DebeGenerarDomainEventTerceroCreado()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, null, null);

        tercero.DomainEvents.Should().ContainSingle();
        tercero.DomainEvents.First().Should().BeOfType<TerceroCreadoEvent>();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_DebeActualizarPropiedades()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, null, null);

        tercero.Actualizar(
            "Empresa SA Actualizada",
            "Fantasia",
            "351-1234567",
            "351-9999999",
            "nuevo@email.com",
            "www.empresa.com",
            new Domicilio("San Martín", "123", null, null, "X5000", 1, null),
            500000m,
            null);

        tercero.RazonSocial.Should().Be("Empresa SA Actualizada");
        tercero.NombreFantasia.Should().Be("Fantasia");
        tercero.Email.Should().Be("nuevo@email.com");
        tercero.LimiteCredito.Should().Be(500000m);
        tercero.Domicilio.Calle.Should().Be("San Martín");
    }

    [Fact]
    public void Desactivar_TerceroActivo_DebeDesactivarlo()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, null, null);

        tercero.Desactivar(null);

        tercero.Activo.Should().BeFalse();
        tercero.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_DebeGenerarDomainEventTerceroDesactivado()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, null, null);

        tercero.ClearDomainEvents();
        tercero.Desactivar(null);

        tercero.DomainEvents.Should().ContainSingle();
        tercero.DomainEvents.First().Should().BeOfType<TerceroDesactivadoEvent>();
    }

    [Fact]
    public void Activar_TerceroDesactivado_DebeActivarlo()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, null, null);

        tercero.Desactivar(null);
        tercero.Activar(null);

        tercero.Activo.Should().BeTrue();
    }
}