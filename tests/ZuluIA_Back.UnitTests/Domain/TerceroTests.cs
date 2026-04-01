using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
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
            null,
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
    public void SetPais_AsignaValor()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.SetPais(1L);
        tercero.PaisId.Should().Be(1L);
    }

    [Fact]
    public void SetUsuario_AsignaValor()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        tercero.SetUsuario(9L, null);

        tercero.UsuarioId.Should().Be(9L);
    }

    [Fact]
    public void SetFechaRegistro_ConFechaValida_AsignaValor()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        var fechaRegistro = new DateOnly(2024, 12, 31);

        tercero.SetFechaRegistro(fechaRegistro, null);

        tercero.FechaRegistro.Should().Be(fechaRegistro);
    }

    [Fact]
    public void SetCobrador_SinAplicarComision_ReseteaPorcentaje()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        tercero.SetCobrador(5L, false, 12m);

        tercero.AplicaComisionCobrador.Should().BeFalse();
        tercero.PctComisionCobrador.Should().Be(0m);
    }

    [Fact]
    public void SetVigenciaCredito_RangoInvertido_LanzaExcepcion()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        var act = () => tercero.SetVigenciaCredito(new DateOnly(2025, 12, 31), new DateOnly(2025, 1, 1));

        act.Should().Throw<ArgumentException>();
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

    [Fact]
    public void ActualizarPersoneriaFiscal_CuandoEsFisicaSinNombre_LanzaExcepcion()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        var act = () => tercero.ActualizarPersoneriaFiscal(
            TipoPersoneriaTercero.Fisica,
            null,
            "Perez",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarPersoneriaFiscal_CuandoEsFisicaYGubernamental_LanzaExcepcion()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        var act = () => tercero.ActualizarPersoneriaFiscal(
            TipoPersoneriaTercero.Fisica,
            "Juan",
            "Perez",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true,
            null,
            null,
            null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarPersoneriaFiscal_CuandoSeInformaSoloClaveFiscal_LanzaExcepcion()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        var act = () => tercero.ActualizarPersoneriaFiscal(
            TipoPersoneriaTercero.Juridica,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            "AFIP",
            null,
            null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarPersoneriaFiscal_CuandoEsValida_NormalizaYActualiza()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        tercero.ActualizarPersoneriaFiscal(
            TipoPersoneriaTercero.Fisica,
            " Juan ",
            " Perez ",
            " Sr. ",
            " Comerciante ",
            null,
            " Soltero ",
            " Argentina ",
            "m",
            new DateOnly(1990, 1, 2),
            false,
            " AFIP ",
            " 3 ",
            null);

        tercero.TipoPersoneria.Should().Be(TipoPersoneriaTercero.Fisica);
        tercero.Nombre.Should().Be("Juan");
        tercero.Apellido.Should().Be("Perez");
        tercero.Tratamiento.Should().Be("Sr.");
        tercero.Profesion.Should().Be("Comerciante");
        tercero.EstadoCivil.Should().Be("Soltero");
        tercero.Nacionalidad.Should().Be("Argentina");
        tercero.Sexo.Should().Be("M");
        tercero.FechaNacimiento.Should().Be(new DateOnly(1990, 1, 2));
        tercero.ClaveFiscal.Should().Be("AFIP");
        tercero.ValorClaveFiscal.Should().Be("3");
    }

    [Fact]
    public void ActualizarRoles_CuandoNoQuedaNingunRol_LanzaExcepcion()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        var act = () => tercero.ActualizarRoles(false, false, false, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarRoles_CuandoEsValido_ActualizaYGeneraEvento()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.ClearDomainEvents();

        tercero.ActualizarRoles(true, true, false, null);

        tercero.EsCliente.Should().BeTrue();
        tercero.EsProveedor.Should().BeTrue();
        tercero.EsEmpleado.Should().BeFalse();
        tercero.DomainEvents.Should().ContainSingle(x => x is TerceroRolesActualizadosEvent);
    }

    [Fact]
    public void SetCatalogosPorRol_AsignaValoresEsperados()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, true, false, null, null);

        tercero.SetCategoriaCliente(11L);
        tercero.SetEstadoCliente(12L);
        tercero.SetCategoriaProveedor(21L);
        tercero.SetEstadoProveedor(22L);

        tercero.CategoriaClienteId.Should().Be(11L);
        tercero.EstadoClienteId.Should().Be(12L);
        tercero.CategoriaProveedorId.Should().Be(21L);
        tercero.EstadoProveedorId.Should().Be(22L);
    }
}