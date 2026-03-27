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
            true, false, false, 1, null);

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
            true, false, false, null, null);

        tercero.Legajo.Should().Be("CLI001");
    }

    [Fact]
    public void Crear_LegajoVacio_DebeLanzarExcepcion()
    {
        var act = () => Tercero.Crear(
            "", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_RazonSocialVacia_DebeLanzarExcepcion()
    {
        var act = () => Tercero.Crear(
            "CLI001", "",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DebeGenerarDomainEventTerceroCreado()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        tercero.DomainEvents.Should().ContainSingle();
        tercero.DomainEvents.First().Should().BeOfType<TerceroCreadoEvent>();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_DebeActualizarPropiedades()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        tercero.Actualizar(
            "Empresa SA Actualizada",
            "Fantasia",
            "351-1234567",
            "351-9999999",
            "nuevo@email.com",
            "www.empresa.com",
            new Domicilio("San Martín", "123", null, null, "X5000", 1, null),
            null,
            null,
            500000m,
            true,
            null,
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
            true, false, false, null, null);

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
            true, false, false, null, null);

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
            true, false, false, null, null);

        tercero.Desactivar(null);
        tercero.Activar(null);

        tercero.Activo.Should().BeTrue();
    }

    // ── ActualizarNroDocumento ────────────────────────────────────────────────

    [Fact]
    public void ActualizarNroDocumento_NroValido_ActualizaPropiedad()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        tercero.ActualizarNroDocumento("20-22222222-2", null);

        tercero.NroDocumento.Should().Be("20-22222222-2");
    }

    [Fact]
    public void ActualizarNroDocumento_NroVacio_LanzaExcepcion()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        var act = () => tercero.ActualizarNroDocumento("   ", null);

        act.Should().Throw<ArgumentException>();
    }

    // ── SetCobrador ───────────────────────────────────────────────────────────

    [Fact]
    public void SetCobrador_PorcentajeValido_AsignaValores()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        tercero.SetCobrador(5L, 10m);

        tercero.CobradorId.Should().Be(5L);
        tercero.PctComisionCobrador.Should().Be(10m);
    }

    [Fact]
    public void SetCobrador_PorcentajeMayorA100_LanzaExcepcion()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        var act = () => tercero.SetCobrador(5L, 101m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetCobrador_PorcentajeNegativo_LanzaExcepcion()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        var act = () => tercero.SetCobrador(5L, -1m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── SetVendedor ───────────────────────────────────────────────────────────

    [Fact]
    public void SetVendedor_PorcentajeValido_AsignaValores()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        tercero.SetVendedor(7L, 5m);

        tercero.VendedorId.Should().Be(7L);
        tercero.PctComisionVendedor.Should().Be(5m);
    }

    [Fact]
    public void SetVendedor_PorcentajeMayorA100_LanzaExcepcion()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        var act = () => tercero.SetVendedor(7L, 150m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetVendedor_PorcentajeNegativo_LanzaExcepcion()
    {
        var tercero = Tercero.Crear(
            "CLI001", "Empresa SA",
            1, "30-11111111-1", 1,
            true, false, false, null, null);

        var act = () => tercero.SetVendedor(7L, -1m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetMoneda_AsignaValor()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.SetMoneda(3L);
        tercero.MonedaId.Should().Be(3L);
    }

    [Fact]
    public void SetMoneda_Null_LimpiaMoned()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.SetMoneda(null);
        tercero.MonedaId.Should().BeNull();
    }

    [Fact]
    public void SetCategoria_AsignaValor()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.SetCategoria(5L);
        tercero.CategoriaId.Should().Be(5L);
    }

    [Fact]
    public void SetSucursal_AsignaValor()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.SetSucursal(2L);
        tercero.SucursalId.Should().Be(2L);
    }

    [Fact]
    public void SetDomicilio_AsignaDomicilio()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        var dom = new ZuluIA_Back.Domain.ValueObjects.Domicilio("San Martín", "123", null, null, "5000", 1L, null);
        tercero.SetDomicilio(dom);
        tercero.Domicilio.Calle.Should().Be("San Martín");
    }

    [Fact]
    public void SetNroIngresosBrutos_ConEspacios_Recorta()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.SetNroIngresosBrutos("  IB-001  ");
        tercero.NroIngresosBrutos.Should().Be("IB-001");
    }
}