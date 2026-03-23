using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ComprobanteFormaPago
// ─────────────────────────────────────────────────────────────────────────────
public class ComprobanteFormaPagoTests
{
    private static readonly DateOnly _fechaCfp = new(2025, 1, 15);

    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var cfp = ComprobanteFormaPago.Crear(1, 2, _fechaCfp, 100m);
        cfp.Should().NotBeNull();
        cfp.ComprobanteId.Should().Be(1);
        cfp.FormaPagoId.Should().Be(2);
        cfp.Importe.Should().Be(100m);
        cfp.Cotizacion.Should().Be(1m);
    }

    [Fact]
    public void Crear_ComprobanteIdCero_LanzaExcepcion()
    {
        var act = () => ComprobanteFormaPago.Crear(0, 1, _fechaCfp, 100m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_FormaPagoIdCero_LanzaExcepcion()
    {
        var act = () => ComprobanteFormaPago.Crear(1, 0, _fechaCfp, 100m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ImporteCero_LanzaExcepcion()
    {
        var act = () => ComprobanteFormaPago.Crear(1, 1, _fechaCfp, 0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CotizacionCero_LanzaExcepcion()
    {
        var act = () => ComprobanteFormaPago.Crear(1, 1, _fechaCfp, 100m, cotizacion: 0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Anular_EstableceValidoEnFalso()
    {
        var cfp = ComprobanteFormaPago.Crear(1, 2, _fechaCfp, 100m);
        cfp.Anular();
        cfp.Valido.Should().BeFalse();
    }

    [Fact]
    public void ActualizarImporte_ValorValido_ActualizaCorrectamente()
    {
        var cfp = ComprobanteFormaPago.Crear(1, 2, _fechaCfp, 100m);
        cfp.ActualizarImporte(200m, 1.5m);
        cfp.Importe.Should().Be(200m);
        cfp.Cotizacion.Should().Be(1.5m);
    }

    [Fact]
    public void ActualizarImporte_ImporteCero_LanzaExcepcion()
    {
        var cfp = ComprobanteFormaPago.Crear(1, 2, _fechaCfp, 100m);
        var act = () => cfp.ActualizarImporte(0m, 1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarImporte_CotizacionCero_LanzaExcepcion()
    {
        var cfp = ComprobanteFormaPago.Crear(1, 2, _fechaCfp, 100m);
        var act = () => cfp.ActualizarImporte(100m, 0m);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TipoComprobanteSucursal
// ─────────────────────────────────────────────────────────────────────────────
public class TipoComprobanteSucursalTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var tcs = TipoComprobanteSucursal.Crear(1, null, 1L);
        tcs.Should().NotBeNull();
        tcs.TipoComprobanteId.Should().Be(1);
        tcs.NumeroComprobanteProximo.Should().Be(1);
    }

    [Fact]
    public void Crear_TipoIdCero_LanzaExcepcion()
    {
        var act = () => TipoComprobanteSucursal.Crear(0, null, 1L);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_NumeroProximoCero_LanzaExcepcion()
    {
        var act = () => TipoComprobanteSucursal.Crear(1, null, 0L);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IncrementarNumero_IncrementaEnUno()
    {
        var tcs = TipoComprobanteSucursal.Crear(1, null, 5L);
        tcs.IncrementarNumero();
        tcs.NumeroComprobanteProximo.Should().Be(6);
    }

    [Fact]
    public void ActualizarConfiguracion_ActualizaValores()
    {
        var tcs = TipoComprobanteSucursal.Crear(1, null, 1L);
        tcs.ActualizarConfiguracion(50, 80, 2, true, false, false, null, true, false, false, null, null);
        tcs.FilasCantidad.Should().Be(50);
        tcs.FilasAnchoMaximo.Should().Be(80);
        tcs.CantidadCopias.Should().Be(2);
        tcs.ImprimirControladorFiscal.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ItemComponente
// ─────────────────────────────────────────────────────────────────────────────
public class ItemComponenteTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var ic = ItemComponente.Crear(1, 2, 3m);
        ic.Should().NotBeNull();
        ic.ItemPadreId.Should().Be(1);
        ic.ComponenteId.Should().Be(2);
        ic.Cantidad.Should().Be(3m);
    }

    [Fact]
    public void Crear_ItemPadreIdCero_LanzaExcepcion()
    {
        var act = () => ItemComponente.Crear(0, 2, 1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ComponenteIdCero_LanzaExcepcion()
    {
        var act = () => ItemComponente.Crear(1, 0, 1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_AutoReferencia_LanzaExcepcion()
    {
        var act = () => ItemComponente.Crear(5, 5, 1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CantidadCero_LanzaExcepcion()
    {
        var act = () => ItemComponente.Crear(1, 2, 0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarCantidad_ValorValido_ActualizaCorrectamente()
    {
        var ic = ItemComponente.Crear(1, 2, 1m);
        ic.ActualizarCantidad(5m);
        ic.Cantidad.Should().Be(5m);
    }

    [Fact]
    public void ActualizarCantidad_CantidadCero_LanzaExcepcion()
    {
        var ic = ItemComponente.Crear(1, 2, 1m);
        var act = () => ic.ActualizarCantidad(0m);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CierreCajaDetalle
// ─────────────────────────────────────────────────────────────────────────────
public class CierreCajaDetalleTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var detalle = CierreCajaDetalle.Crear(1, 2, 500m);
        detalle.Should().NotBeNull();
        detalle.CierreId.Should().Be(1);
        detalle.CajaCuentaBancariaId.Should().Be(2);
        detalle.Diferencia.Should().Be(500m);
    }

    [Fact]
    public void Crear_CierreIdCero_LanzaExcepcion()
    {
        var act = () => CierreCajaDetalle.Crear(0, 1, 0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CajaCuentaIdCero_LanzaExcepcion()
    {
        var act = () => CierreCajaDetalle.Crear(1, 0, 0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarDiferencia_ActualizaValor()
    {
        var detalle = CierreCajaDetalle.Crear(1, 2, 0m);
        detalle.ActualizarDiferencia(-100m);
        detalle.Diferencia.Should().Be(-100m);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ComprobanteEntrega
// ─────────────────────────────────────────────────────────────────────────────
public class ComprobanteEntregaTests
{
    private static readonly DateOnly _fecha = new(2025, 6, 1);

    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var ce = ComprobanteEntrega.Crear(1, _fecha);
        ce.Should().NotBeNull();
        ce.ComprobanteId.Should().Be(1);
    }

    [Fact]
    public void Crear_ComprobanteIdCero_LanzaExcepcion()
    {
        var act = () => ComprobanteEntrega.Crear(0, _fecha);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_EmailSeMayuscula_SeNormaliza()
    {
        var ce = ComprobanteEntrega.Crear(1, _fecha, email: "Test@Example.COM");
        ce.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Actualizar_EmailSeMayuscula_SeNormaliza()
    {
        var ce = ComprobanteEntrega.Crear(1, _fecha);
        ce.Actualizar(null, null, null, null, null, null, null, null, null, "USER@DOMAIN.ORG", null, null, null, null);
        ce.Email.Should().Be("user@domain.org");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ImpuestoPorItem
// ─────────────────────────────────────────────────────────────────────────────
public class ImpuestoPorItemTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var ipi = ImpuestoPorItem.Crear(1, 2);
        ipi.Should().NotBeNull();
        ipi.ImpuestoId.Should().Be(1);
        ipi.ItemId.Should().Be(2);
    }

    [Fact]
    public void Crear_ImpuestoIdCero_LanzaExcepcion()
    {
        var act = () => ImpuestoPorItem.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ItemIdCero_LanzaExcepcion()
    {
        var act = () => ImpuestoPorItem.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ActualizaDescripcionYObservacion()
    {
        var ipi = ImpuestoPorItem.Crear(1, 2, "original", null);
        ipi.Actualizar("  nueva desc  ", "  obs  ");
        ipi.Descripcion.Should().Be("nueva desc");
        ipi.Observacion.Should().Be("obs");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ImpuestoPorPersona
// ─────────────────────────────────────────────────────────────────────────────
public class ImpuestoPorPersonaTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var ipp = ImpuestoPorPersona.Crear(1, 2);
        ipp.Should().NotBeNull();
        ipp.ImpuestoId.Should().Be(1);
        ipp.TerceroId.Should().Be(2);
    }

    [Fact]
    public void Crear_ImpuestoIdCero_LanzaExcepcion()
    {
        var act = () => ImpuestoPorPersona.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_TerceroIdCero_LanzaExcepcion()
    {
        var act = () => ImpuestoPorPersona.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ActualizaDescripcionYObservacion()
    {
        var ipp = ImpuestoPorPersona.Crear(1, 2, "original", null);
        ipp.Actualizar("  nueva desc  ", "  obs  ");
        ipp.Descripcion.Should().Be("nueva desc");
        ipp.Observacion.Should().Be("obs");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ImpuestoPorSucursal
// ─────────────────────────────────────────────────────────────────────────────
public class ImpuestoPorSucursalTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var ips = ImpuestoPorSucursal.Crear(1, 2);
        ips.Should().NotBeNull();
        ips.ImpuestoId.Should().Be(1);
        ips.SucursalId.Should().Be(2);
    }

    [Fact]
    public void Crear_ImpuestoIdCero_LanzaExcepcion()
    {
        var act = () => ImpuestoPorSucursal.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SucursalIdCero_LanzaExcepcion()
    {
        var act = () => ImpuestoPorSucursal.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ActualizaDescripcionYObservacion()
    {
        var ips = ImpuestoPorSucursal.Crear(1, 2, "original", null);
        ips.Actualizar("  nueva desc  ", "  obs  ");
        ips.Descripcion.Should().Be("nueva desc");
        ips.Observacion.Should().Be("obs");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Contacto
// ─────────────────────────────────────────────────────────────────────────────
public class ContactoTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var c = Contacto.Crear(1, 2);
        c.Should().NotBeNull();
        c.PersonaId.Should().Be(1);
        c.PersonaContactoId.Should().Be(2);
    }

    [Fact]
    public void Crear_PersonaIdCero_LanzaExcepcion()
    {
        var act = () => Contacto.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_PersonaContactoIdCero_LanzaExcepcion()
    {
        var act = () => Contacto.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarTipoRelacion_AsignaNuevoValor()
    {
        var c = Contacto.Crear(1, 2, null);
        c.ActualizarTipoRelacion(5L);
        c.TipoRelacionId.Should().Be(5L);
    }

    [Fact]
    public void ActualizarTipoRelacion_ConNull_LimpiaTipoRelacion()
    {
        var c = Contacto.Crear(1, 2, 5L);
        c.ActualizarTipoRelacion(null);
        c.TipoRelacionId.Should().BeNull();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MedioContacto
// ─────────────────────────────────────────────────────────────────────────────
public class MedioContactoTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var mc = MedioContacto.Crear(1, "test@email.com");
        mc.Should().NotBeNull();
        mc.PersonaId.Should().Be(1);
        mc.Valor.Should().Be("test@email.com");
    }

    [Fact]
    public void Crear_PersonaIdCero_LanzaExcepcion()
    {
        var act = () => MedioContacto.Crear(0, "valor");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ValorVacio_LanzaExcepcion()
    {
        var act = () => MedioContacto.Crear(1, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ValorValido_ActualizaCorrectamente()
    {
        var mc = MedioContacto.Crear(1, "original");
        mc.Actualizar("nuevo-valor", null, 0, false, null);
        mc.Valor.Should().Be("nuevo-valor");
    }

    [Fact]
    public void Actualizar_ValorVacio_LanzaExcepcion()
    {
        var mc = MedioContacto.Crear(1, "original");
        var act = () => mc.Actualizar("", null, 0, false, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// VinculacionPersona
// ─────────────────────────────────────────────────────────────────────────────
public class VinculacionPersonaTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var vp = VinculacionPersona.Crear(1, 2);
        vp.Should().NotBeNull();
        vp.ClienteId.Should().Be(1);
        vp.ProveedorId.Should().Be(2);
    }

    [Fact]
    public void Crear_ClienteIdCero_LanzaExcepcion()
    {
        var act = () => VinculacionPersona.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ProveedorIdCero_LanzaExcepcion()
    {
        var act = () => VinculacionPersona.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ActualizaEsPredeterminadoYTipoRelacion()
    {
        var vp = VinculacionPersona.Crear(1, 2, false, null);
        vp.Actualizar(true, 5L);
        vp.EsPredeterminado.Should().BeTrue();
        vp.TipoRelacionId.Should().Be(5L);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// EmpleadoXArea
// ─────────────────────────────────────────────────────────────────────────────
public class EmpleadoXAreaTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var exa = EmpleadoXArea.Crear(1, 2, 3);
        exa.Should().NotBeNull();
        exa.EmpleadoId.Should().Be(1);
        exa.AreaId.Should().Be(2);
        exa.Orden.Should().Be(3);
    }

    [Fact]
    public void Crear_EmpleadoIdCero_LanzaExcepcion()
    {
        var act = () => EmpleadoXArea.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_AreaIdCero_LanzaExcepcion()
    {
        var act = () => EmpleadoXArea.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarOrden_ActualizaValor()
    {
        var exa = EmpleadoXArea.Crear(1, 2);
        exa.ActualizarOrden(10);
        exa.Orden.Should().Be(10);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// EmpleadoXPerfil
// ─────────────────────────────────────────────────────────────────────────────
public class EmpleadoXPerfilTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var exp = EmpleadoXPerfil.Crear(1, 2, 5);
        exp.Should().NotBeNull();
        exp.EmpleadoXAreaId.Should().Be(1);
        exp.PerfilId.Should().Be(2);
        exp.Orden.Should().Be(5);
    }

    [Fact]
    public void Crear_EmpleadoXAreaIdCero_LanzaExcepcion()
    {
        var act = () => EmpleadoXPerfil.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_PerfilIdCero_LanzaExcepcion()
    {
        var act = () => EmpleadoXPerfil.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActualizarOrden_ActualizaValor()
    {
        var exp = EmpleadoXPerfil.Crear(1, 2);
        exp.ActualizarOrden(7);
        exp.Orden.Should().Be(7);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// AtributoItem
// ─────────────────────────────────────────────────────────────────────────────
public class AtributoItemTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var ai = AtributoItem.Crear(1, 2, "Rojo");
        ai.Should().NotBeNull();
        ai.ItemId.Should().Be(1);
        ai.AtributoId.Should().Be(2);
        ai.Valor.Should().Be("Rojo");
    }

    [Fact]
    public void Crear_ItemIdCero_LanzaExcepcion()
    {
        var act = () => AtributoItem.Crear(0, 1, "x");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_AtributoIdCero_LanzaExcepcion()
    {
        var act = () => AtributoItem.Crear(1, 0, "x");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ValorConEspacios_SeRecorta()
    {
        var ai = AtributoItem.Crear(1, 2, "  Azul  ");
        ai.Valor.Should().Be("Azul");
    }

    [Fact]
    public void ActualizarValor_ActualizaYRecortaValor()
    {
        var ai = AtributoItem.Crear(1, 2, "original");
        ai.ActualizarValor("  nuevo  ");
        ai.Valor.Should().Be("nuevo");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CuboFiltro
// ─────────────────────────────────────────────────────────────────────────────
public class CuboFiltroTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var cf = CuboFiltro.Crear(1, "campo > 0", 1, 0);
        cf.Should().NotBeNull();
        cf.CuboId.Should().Be(1);
        cf.Filtro.Should().Be("campo > 0");
        cf.Operador.Should().Be(1);
    }

    [Fact]
    public void Crear_CuboIdCero_LanzaExcepcion()
    {
        var act = () => CuboFiltro.Crear(0, "filtro");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_FiltroVacio_LanzaExcepcion()
    {
        var act = () => CuboFiltro.Crear(1, "  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_FiltroConEspacios_SeRecorta()
    {
        var cf = CuboFiltro.Crear(1, "  x=1  ");
        cf.Filtro.Should().Be("x=1");
    }

    [Fact]
    public void Actualizar_FiltroValido_ActualizaCorrectamente()
    {
        var cf = CuboFiltro.Crear(1, "original");
        cf.Actualizar("nuevo", 2, 5);
        cf.Filtro.Should().Be("nuevo");
        cf.Operador.Should().Be(2);
        cf.Orden.Should().Be(5);
    }

    [Fact]
    public void Actualizar_FiltroVacio_LanzaExcepcion()
    {
        var cf = CuboFiltro.Crear(1, "original");
        var act = () => cf.Actualizar("", 1, 0);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MovimientoStockAtributo
// ─────────────────────────────────────────────────────────────────────────────
public class MovimientoStockAtributoTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var msa = MovimientoStockAtributo.Crear(1, 2, "LOTE-2024");
        msa.Should().NotBeNull();
        msa.MovimientoStockId.Should().Be(1);
        msa.AtributoId.Should().Be(2);
        msa.Valor.Should().Be("LOTE-2024");
    }

    [Fact]
    public void Crear_MovimientoStockIdCero_LanzaExcepcion()
    {
        var act = () => MovimientoStockAtributo.Crear(0, 1, "val");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_AtributoIdCero_LanzaExcepcion()
    {
        var act = () => MovimientoStockAtributo.Crear(1, 0, "val");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ValorVacio_LanzaExcepcion()
    {
        var act = () => MovimientoStockAtributo.Crear(1, 1, "   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ValorConEspacios_SeRecorta()
    {
        var msa = MovimientoStockAtributo.Crear(1, 2, "  SN-001  ");
        msa.Valor.Should().Be("SN-001");
    }

    [Fact]
    public void ActualizarValor_ValorValido_ActualizaCorrectamente()
    {
        var msa = MovimientoStockAtributo.Crear(1, 2, "original");
        msa.ActualizarValor("  nuevo  ");
        msa.Valor.Should().Be("nuevo");
    }

    [Fact]
    public void ActualizarValor_ValorVacio_LanzaExcepcion()
    {
        var msa = MovimientoStockAtributo.Crear(1, 2, "original");
        var act = () => msa.ActualizarValor("");
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PersonaXTipoPersona
// ─────────────────────────────────────────────────────────────────────────────
public class PersonaXTipoPersonaTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var pxtp = PersonaXTipoPersona.Crear(1, 2, "LEG-001", 1);
        pxtp.Should().NotBeNull();
        pxtp.PersonaId.Should().Be(1);
        pxtp.TipoPersonaId.Should().Be(2);
        pxtp.Legajo.Should().Be("LEG-001");
        pxtp.LegajoOrden.Should().Be(1);
    }

    [Fact]
    public void Crear_PersonaIdCero_LanzaExcepcion()
    {
        var act = () => PersonaXTipoPersona.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_TipoPersonaIdCero_LanzaExcepcion()
    {
        var act = () => PersonaXTipoPersona.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SinLegajo_LegajoEsNulo()
    {
        var pxtp = PersonaXTipoPersona.Crear(1, 2);
        pxtp.Legajo.Should().BeNull();
        pxtp.LegajoOrden.Should().BeNull();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SucursalDomicilio
// ─────────────────────────────────────────────────────────────────────────────
public class SucursalDomicilioTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var sd = SucursalDomicilio.Crear(1, null, null, null, "Av. Principal 100");
        sd.Should().NotBeNull();
        sd.SucursalId.Should().Be(1);
        sd.Calle.Should().Be("Av. Principal 100");
        sd.EsDefecto.Should().BeFalse();
    }

    [Fact]
    public void Crear_SucursalIdCero_LanzaExcepcion()
    {
        var act = () => SucursalDomicilio.Crear(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_EsDefectoTrue_SeEstablece()
    {
        var sd = SucursalDomicilio.Crear(1, esDefecto: true);
        sd.EsDefecto.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_ActualizaValores()
    {
        var sd = SucursalDomicilio.Crear(1);
        sd.Actualizar(1L, 2L, 3L, "Nueva Calle", null, "5000", null, 1, true);
        sd.TipoDomicilioId.Should().Be(1);
        sd.Calle.Should().Be("Nueva Calle");
        sd.CodigoPostal.Should().Be("5000");
        sd.EsDefecto.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PersonaDomicilio
// ─────────────────────────────────────────────────────────────────────────────
public class PersonaDomicilioTests
{
    [Fact]
    public void Crear_ConPersonaIdValido_RetornaInstancia()
    {
        var pd = PersonaDomicilio.Crear(1, null, null, null, "Calle 123");
        pd.Should().NotBeNull();
        pd.PersonaId.Should().Be(1);
        pd.Calle.Should().Be("Calle 123");
        pd.EsDefecto.Should().BeFalse();
    }

    [Fact]
    public void Crear_PersonaIdCero_LanzaExcepcion()
    {
        var act = () => PersonaDomicilio.Crear(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_EsDefectoTrue_SeEstablece()
    {
        var pd = PersonaDomicilio.Crear(1, esDefecto: true);
        pd.EsDefecto.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_ActualizaValores()
    {
        var pd = PersonaDomicilio.Crear(1);
        pd.Actualizar(1L, 2L, 3L, "Nueva Calle", null, "5000", null, 1, true);
        pd.TipoDomicilioId.Should().Be(1L);
        pd.Calle.Should().Be("Nueva Calle");
        pd.CodigoPostal.Should().Be("5000");
        pd.EsDefecto.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UsuarioXUsuario
// ─────────────────────────────────────────────────────────────────────────────
public class UsuarioXUsuarioTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var uxu = UsuarioXUsuario.Crear(1, 2);
        uxu.Should().NotBeNull();
        uxu.UsuarioMiembroId.Should().Be(1);
        uxu.UsuarioGrupoId.Should().Be(2);
    }

    [Fact]
    public void Crear_MiembroIdCero_LanzaExcepcion()
    {
        var act = () => UsuarioXUsuario.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_GrupoIdCero_LanzaExcepcion()
    {
        var act = () => UsuarioXUsuario.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_AutoReferencia_LanzaExcepcion()
    {
        var act = () => UsuarioXUsuario.Crear(5, 5);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// VariableDetalle
// ─────────────────────────────────────────────────────────────────────────────
public class VariableDetalleTests
{
    [Fact]
    public void Crear_Valido_RetornaInstancia()
    {
        var vd = VariableDetalle.Crear(1, 2, true, false, 50m, 100m);
        vd.Should().NotBeNull();
        vd.VariableId.Should().Be(1);
        vd.OpcionVariableId.Should().Be(2);
        vd.AplicaPuntajePenalizacion.Should().BeTrue();
        vd.VisualizarOpcion.Should().BeFalse();
        vd.PorcentajeIncidencia.Should().Be(50m);
        vd.ValorObjetivo.Should().Be(100m);
    }

    [Fact]
    public void Crear_VariableIdCero_LanzaExcepcion()
    {
        var act = () => VariableDetalle.Crear(0, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SinOpcional_OpcionEsNula()
    {
        var vd = VariableDetalle.Crear(1, null);
        vd.OpcionVariableId.Should().BeNull();
        vd.PorcentajeIncidencia.Should().BeNull();
        vd.ValorObjetivo.Should().BeNull();
    }

    [Fact]
    public void Actualizar_ActualizaValores()
    {
        var vd = VariableDetalle.Crear(1, null);
        vd.Actualizar(5L, true, true, 75m, 200m);
        vd.OpcionVariableId.Should().Be(5);
        vd.AplicaPuntajePenalizacion.Should().BeTrue();
        vd.VisualizarOpcion.Should().BeTrue();
        vd.PorcentajeIncidencia.Should().Be(75m);
        vd.ValorObjetivo.Should().Be(200m);
    }
}
