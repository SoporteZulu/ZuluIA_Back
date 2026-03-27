using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.UnitTests.Domain;

// ── ListaPrecioPersona ───────────────────────────────────────────────────────

public class ListaPrecioPersonaTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearAsignacion()
    {
        var lpp = ListaPrecioPersona.Crear(1, 10);

        lpp.ListaPreciosId.Should().Be(1);
        lpp.PersonaId.Should().Be(10);
    }

    [Fact]
    public void Crear_ListaPreciosIdCero_DebeLanzarArgumentException()
    {
        var act = () => ListaPrecioPersona.Crear(0, 10);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_PersonaIdCero_DebeLanzarArgumentException()
    {
        var act = () => ListaPrecioPersona.Crear(1, 0);

        act.Should().Throw<ArgumentException>();
    }
}

// ── UnidadManipulacion ───────────────────────────────────────────────────────

public class UnidadManipulacionTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearUnidad()
    {
        var um = UnidadManipulacion.Crear(5, "Caja x 12", 12m, 2);

        um.ItemId.Should().Be(5);
        um.Descripcion.Should().Be("Caja x 12");
        um.Cantidad.Should().Be(12m);
        um.UnidadMedidaId.Should().Be(2);
    }

    [Fact]
    public void Crear_DescripcionConEspacios_DebeRecortarse()
    {
        var um = UnidadManipulacion.Crear(1, "  Pallet  ", 10m, 1);

        um.Descripcion.Should().Be("Pallet");
    }

    [Fact]
    public void Crear_DescripcionVacia_DebeLanzarArgumentException()
    {
        var act = () => UnidadManipulacion.Crear(1, "", 10m, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CantidadCero_DebeLanzarArgumentException()
    {
        var act = () => UnidadManipulacion.Crear(1, "Caja", 0m, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_DebeActualizarCampos()
    {
        var um = UnidadManipulacion.Crear(1, "Caja", 12m, 2);

        um.Actualizar("Pallet x 20", 20m, 3, null);

        um.Descripcion.Should().Be("Pallet x 20");
        um.Cantidad.Should().Be(20m);
        um.UnidadMedidaId.Should().Be(3);
    }
}

// ── ConfiguracionFiscal ──────────────────────────────────────────────────────

public class ConfiguracionFiscalTests
{
    [Fact]
    public void Crear_ConDatosMinimos_DebeCrearConfiguracion()
    {
        var cfg = ConfiguracionFiscal.Crear(1, 2);

        cfg.PuntoFacturacionId.Should().Be(1);
        cfg.TipoComprobanteId.Should().Be(2);
        cfg.CopiasDefault.Should().Be(2);
        cfg.Online.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConTodosLosCampos_DebeAsignarCampos()
    {
        var cfg = ConfiguracionFiscal.Crear(
            1, 2, 3, 4, 0, "COM1", 3, "ABC123",
            @"C:\fiscal", @"C:\fiscal\backup", 5000, 60000, true);

        cfg.TecnologiaId.Should().Be(3);
        cfg.InterfazFiscalId.Should().Be(4);
        cfg.Puerto.Should().Be("COM1");
        cfg.CopiasDefault.Should().Be(3);
        cfg.ClaveActivacion.Should().Be("ABC123");
        cfg.Online.Should().BeTrue();
    }

    [Fact]
    public void Crear_PuertoConEspacios_DebeRecortarse()
    {
        var cfg = ConfiguracionFiscal.Crear(1, 2, puerto: "  COM2  ");

        cfg.Puerto.Should().Be("COM2");
    }

    [Fact]
    public void Actualizar_DebeActualizarTodosLosCampos()
    {
        var cfg = ConfiguracionFiscal.Crear(1, 2);

        cfg.Actualizar(5, 6, 1, "COM3", 2, "KEY", @"C:\new", null, 3000, 30000, true);

        cfg.TecnologiaId.Should().Be(5);
        cfg.InterfazFiscalId.Should().Be(6);
        cfg.Puerto.Should().Be("COM3");
        cfg.Online.Should().BeTrue();
    }
}

// ── ImpuestoPorTipoComprobante ────────────────────────────────────────────────

public class ImpuestoPorTipoComprobanteTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearAsignacion()
    {
        var asig = ImpuestoPorTipoComprobante.Crear(10, 2, 6);

        asig.ImpuestoId.Should().Be(10);
        asig.TipoComprobanteId.Should().Be(2);
        asig.Orden.Should().Be(6);
    }

    [Fact]
    public void Crear_ImpuestoIdCero_DebeLanzarArgumentException()
    {
        var act = () => ImpuestoPorTipoComprobante.Crear(0, 2);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_TipoComprobanteIdCero_DebeLanzarArgumentException()
    {
        var act = () => ImpuestoPorTipoComprobante.Crear(10, 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarOrden_DebeActualizarOrden()
    {
        var asig = ImpuestoPorTipoComprobante.Crear(10, 2, 6);

        asig.ActualizarOrden(99);

        asig.Orden.Should().Be(99);
    }
}

// ── TipoComprobantePuntoFacturacion ───────────────────────────────────────────

public class TipoComprobantePuntoFacturacionTests
{
    [Fact]
    public void Crear_ConDatosMinimos_DebeCrearAsignacion()
    {
        var tc = TipoComprobantePuntoFacturacion.Crear(1, 5);

        tc.PuntoFacturacionId.Should().Be(1);
        tc.TipoComprobanteId.Should().Be(5);
        tc.NumeroComprobanteProximo.Should().Be(1);
        tc.Editable.Should().BeTrue();
        tc.CantidadCopias.Should().Be(1);
    }

    [Fact]
    public void Crear_PuntoFacturacionIdCero_DebeLanzarArgumentException()
    {
        var act = () => TipoComprobantePuntoFacturacion.Crear(0, 5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_TipoComprobanteIdCero_DebeLanzarArgumentException()
    {
        var act = () => TipoComprobantePuntoFacturacion.Crear(1, 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_MascaraMonedaConEspacios_DebeRecortarse()
    {
        var tc = TipoComprobantePuntoFacturacion.Crear(1, 5, mascaraMoneda: "  ##.##0,00  ");

        tc.MascaraMoneda.Should().Be("##.##0,00");
    }

    [Fact]
    public void Actualizar_DebeActualizarCampos()
    {
        var tc = TipoComprobantePuntoFacturacion.Crear(1, 5);

        tc.Actualizar(100, false, 5, 20, null, 2, true, true, true, null, "$#.##0,00");

        tc.NumeroComprobanteProximo.Should().Be(100);
        tc.Editable.Should().BeFalse();
        tc.CantidadCopias.Should().Be(2);
        tc.VistaPrevia.Should().BeTrue();
        tc.ImprimirControladorFiscal.Should().BeTrue();
        tc.MascaraMoneda.Should().Be("$#.##0,00");
    }
}
