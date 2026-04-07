using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Enums;
using LogisticaOrdenEmpaque = ZuluIA_Back.Domain.Entities.Logistica.OrdenEmpaque;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// Cedulon
// ─────────────────────────────────────────────────────────────────────────────

public class CedulonTests
{
    private static readonly DateOnly _emision     = new(2025, 1, 1);
    private static readonly DateOnly _vencimiento = new(2025, 3, 31);

    private static Cedulon CedulonValido() =>
        Cedulon.Crear(1, 1, null, "CED-001", _emision, _vencimiento, 1000m, null);

    [Fact]
    public void Crear_ConDatosValidos_EstadoPendiente()
    {
        var c = CedulonValido();
        c.Estado.Should().Be(EstadoCedulon.Pendiente);
        c.ImportePagado.Should().Be(0m);
    }

    [Fact]
    public void Crear_ImporteCero_LanzaExcepcion()
    {
        var act = () => Cedulon.Crear(1, 1, null, "CED-001", _emision, _vencimiento, 0m, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_VencimientoAnteriorAlEmision_LanzaExcepcion()
    {
        var act = () => Cedulon.Crear(1, 1, null, "CED-001", _vencimiento, _emision, 1000m, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_NroCedulonVacio_LanzaExcepcion()
    {
        var act = () => Cedulon.Crear(1, 1, null, "", _emision, _vencimiento, 1000m, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegistrarPago_ParcialActualiza_EstadoPagadoParcial()
    {
        var c = CedulonValido();
        c.RegistrarPago(400m, null);
        c.ImportePagado.Should().Be(400m);
        c.Estado.Should().Be(EstadoCedulon.PagadoParcial);
    }

    [Fact]
    public void RegistrarPago_Completo_CambiaEstadoAPagado()
    {
        var c = CedulonValido();
        c.RegistrarPago(1000m, null);
        c.Estado.Should().Be(EstadoCedulon.Pagado);
    }

    [Fact]
    public void RegistrarPago_Excede_LanzaExcepcion()
    {
        var c = CedulonValido();
        var act = () => c.RegistrarPago(1500m, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RegistrarPago_ImporteCero_LanzaExcepcion()
    {
        var c = CedulonValido();
        var act = () => c.RegistrarPago(0m, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Vencer_CedulonPendiente_CambiaAVencido()
    {
        var c = CedulonValido();
        c.Vencer(null);
        c.Estado.Should().Be(EstadoCedulon.Vencido);
    }

    [Fact]
    public void Vencer_CedulonPagado_NoModificaEstado()
    {
        var c = CedulonValido();
        c.RegistrarPago(1000m, null);
        c.Vencer(null);
        c.Estado.Should().Be(EstadoCedulon.Pagado);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ComprobanteItem
// ─────────────────────────────────────────────────────────────────────────────

public class ComprobanteItemTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var item = ComprobanteItem.Crear(1, 1, "Producto A", 10m, 0, 100, 0m, 1, 0, null, 1);
        item.Cantidad.Should().Be(10m);
        item.Descripcion.Should().Be("Producto A");
    }

    [Fact]
    public void Crear_CantidadCero_LanzaExcepcion()
    {
        var act = () => ComprobanteItem.Crear(1, 1, "Producto A", 0m, 0, 100, 0m, 1, 0, null, 1);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_PrecioNegativo_LanzaExcepcion()
    {
        var act = () => ComprobanteItem.Crear(1, 1, "Producto A", 5m, 0, -10, 0m, 1, 0, null, 1);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_ConIva_CalculaImporteIvaYTotalConDivisionDecimal()
    {
        var item = ComprobanteItem.Crear(1, 1, "Producto A", 1m, 0, 1000, 0m, 1, 21, null, 1);

        item.SubtotalNeto.Should().Be(1000m);
        item.IvaImporte.Should().Be(210m);
        item.TotalLinea.Should().Be(1210m);
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => ComprobanteItem.Crear(1, 1, "", 5m, 0, 100, 0m, 1, 0, null, 1);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CotizacionMoneda
// ─────────────────────────────────────────────────────────────────────────────

public class CotizacionMonedaTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var c = CotizacionMoneda.Crear(1, new DateOnly(2025, 3, 1), 900m);
        c.Cotizacion.Should().Be(900m);
    }

    [Fact]
    public void Crear_CotizacionCero_LanzaExcepcion()
    {
        var act = () => CotizacionMoneda.Crear(1, new DateOnly(2025, 3, 1), 0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ActualizarCotizacion_ValorPositivo_Actualiza()
    {
        var c = CotizacionMoneda.Crear(1, new DateOnly(2025, 3, 1), 900m);
        c.ActualizarCotizacion(950m);
        c.Cotizacion.Should().Be(950m);
    }

    [Fact]
    public void ActualizarCotizacion_Cero_LanzaExcepcion()
    {
        var c = CotizacionMoneda.Crear(1, new DateOnly(2025, 3, 1), 900m);
        var act = () => c.ActualizarCotizacion(0m);
        act.Should().Throw<InvalidOperationException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FormulaIngrediente
// ─────────────────────────────────────────────────────────────────────────────

public class FormulaIngredienteTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var fi = FormulaIngrediente.Crear(1, 2, 5m, null, false, 0);
        fi.Cantidad.Should().Be(5m);
        fi.EsOpcional.Should().BeFalse();
    }

    [Fact]
    public void Crear_CantidadCero_LanzaExcepcion()
    {
        var act = () => FormulaIngrediente.Crear(1, 2, 0m, null, false, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_CantidadNegativa_LanzaExcepcion()
    {
        var act = () => FormulaIngrediente.Crear(1, 2, -1m, null, false, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ActualizarCantidad_ValorPositivo_Actualiza()
    {
        var fi = FormulaIngrediente.Crear(1, 2, 5m, null, false, 0);
        fi.ActualizarCantidad(10m);
        fi.Cantidad.Should().Be(10m);
    }

    [Fact]
    public void ActualizarCantidad_Cero_LanzaExcepcion()
    {
        var fi = FormulaIngrediente.Crear(1, 2, 5m, null, false, 0);
        var act = () => fi.ActualizarCantidad(0m);
        act.Should().Throw<InvalidOperationException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// InventarioConteo
// ─────────────────────────────────────────────────────────────────────────────

public class InventarioConteoTests
{
    [Fact]
    public void Crear_ConDatosValidos_FechaCierreSinAsignar()
    {
        var inv = InventarioConteo.Crear(1, DateTimeOffset.UtcNow, 1);
        inv.FechaCierre.Should().BeNull();
        inv.NroAuditoria.Should().Be(1);
    }

    [Fact]
    public void Crear_UsuarioIdCero_LanzaExcepcion()
    {
        var act = () => InventarioConteo.Crear(0, DateTimeOffset.UtcNow, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_NroAuditoriaCero_LanzaExcepcion()
    {
        var act = () => InventarioConteo.Crear(1, DateTimeOffset.UtcNow, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cerrar_AsignaFechaCierre()
    {
        var inv = InventarioConteo.Crear(1, DateTimeOffset.UtcNow, 1);
        var fecha = DateTimeOffset.UtcNow;
        inv.Cerrar(fecha);
        inv.FechaCierre.Should().Be(fecha);
    }

    [Fact]
    public void Cerrar_YaCerrado_LanzaExcepcion()
    {
        var inv = InventarioConteo.Crear(1, DateTimeOffset.UtcNow, 1);
        inv.Cerrar(DateTimeOffset.UtcNow);
        var act = () => inv.Cerrar(DateTimeOffset.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// OrdenCompraMeta
// ─────────────────────────────────────────────────────────────────────────────

public class OrdenCompraMetaBusinessLogicTests
{
    private static OrdenCompraMeta OrdenValida() =>
        OrdenCompraMeta.Crear(1, 1, null, null, 10m);

    [Fact]
    public void Crear_EstadoInicialPendiente()
    {
        var o = OrdenValida();
        o.EstadoOc.Should().Be(EstadoOrdenCompra.Pendiente);
        o.Habilitada.Should().BeTrue();
    }

    [Fact]
    public void Recibir_DesdeEstadoPendiente_CambiaARecibida()
    {
        var o = OrdenValida();
        o.Recibir();
        o.EstadoOc.Should().Be(EstadoOrdenCompra.Recibida);
    }

    [Fact]
    public void Recibir_YaRecibida_LanzaExcepcion()
    {
        var o = OrdenValida();
        o.Recibir();
        var act = () => o.Recibir();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancelar_DesdeEstadoPendiente_CambiaACancelada()
    {
        var o = OrdenValida();
        o.Cancelar();
        o.EstadoOc.Should().Be(EstadoOrdenCompra.Cancelada);
        o.Habilitada.Should().BeFalse();
    }

    [Fact]
    public void Cancelar_YaCancelada_LanzaExcepcion()
    {
        var o = OrdenValida();
        o.Cancelar();
        var act = () => o.Cancelar();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetFechaEntrega_AsignaFechaEntregaReq()
    {
        var o = OrdenValida();
        var fecha = new DateOnly(2026, 6, 30);

        o.SetFechaEntrega(fecha);

        o.FechaEntregaReq.Should().Be(fecha);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// OrdenPreparacionDetalle (acceso via OrdenPreparacion.AgregarDetalle)
// ─────────────────────────────────────────────────────────────────────────────

public class OrdenPreparacionDetalleTests
{
    private static readonly DateOnly _hoy = DateOnly.FromDateTime(DateTime.Today);

    private static OrdenPreparacionDetalle CrearDetalle(decimal cantidad = 10m)
    {
        var orden = OrdenPreparacion.Crear(1L, null, null, _hoy, null, null);
        orden.AgregarDetalle(1L, 1L, cantidad);
        return orden.Detalles.First();
    }

    [Fact]
    public void AgregarDetalle_CantidadCero_LanzaExcepcion()
    {
        var orden = OrdenPreparacion.Crear(1L, null, null, _hoy, null, null);
        var act = () => orden.AgregarDetalle(1L, 1L, 0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RegistrarEntrega_CantidadValida_Actualiza()
    {
        var detalle = CrearDetalle(10m);
        detalle.RegistrarEntrega(5m);
        detalle.CantidadEntregada.Should().Be(5m);
    }

    [Fact]
    public void RegistrarEntrega_CantidadNegativa_LanzaExcepcion()
    {
        var detalle = CrearDetalle(10m);
        var act = () => detalle.RegistrarEntrega(-1m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RegistrarEntrega_ExcedeCantidad_LanzaExcepcion()
    {
        var detalle = CrearDetalle(10m);
        var act = () => detalle.RegistrarEntrega(15m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EstaCompleto_CantidadEntregadaIgualACantidad_RetornaTrue()
    {
        var detalle = CrearDetalle(10m);
        detalle.RegistrarEntrega(10m);
        detalle.EstaCompleto.Should().BeTrue();
    }

    [Fact]
    public void EstaCompleto_EntregaParcial_RetornaFalse()
    {
        var detalle = CrearDetalle(10m);
        detalle.RegistrarEntrega(5m);
        detalle.EstaCompleto.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PuntoFacturacion
// ─────────────────────────────────────────────────────────────────────────────

public class PuntoFacturacionTests
{
    [Fact]
    public void Crear_ConDatosValidos_EsActivo()
    {
        var p = PuntoFacturacion.Crear(1, 1, 1, null, null);
        p.Activo.Should().BeTrue();
        p.Numero.Should().Be(1);
    }

    [Fact]
    public void Crear_NumeroCero_LanzaExcepcion()
    {
        var act = () => PuntoFacturacion.Crear(1, 1, 0, null, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Desactivar_PoneActivo_EnFalse()
    {
        var p = PuntoFacturacion.Crear(1, 1, 1, null, null);
        p.Desactivar(null);
        p.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_Tras_Desactivar_RestaureaActivo()
    {
        var p = PuntoFacturacion.Crear(1, 1, 1, null, null);
        p.Desactivar(null);
        p.Activar(null);
        p.Activo.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// LiquidacionSueldo
// ─────────────────────────────────────────────────────────────────────────────

public class LiquidacionSueldoTests
{
    private static LiquidacionSueldo Crear(int anio = 2025, int mes = 6) =>
        LiquidacionSueldo.Crear(1, 1, anio, mes, 100000m, 120000m, 20000m, 1, null);

    [Fact]
    public void Crear_NetoCalculadoComoHabelesMinusDescuentos()
    {
        var l = Crear();
        l.Neto.Should().Be(100000m); // 120000 - 20000
    }

    [Fact]
    public void Crear_NoPagadaInicialmente()
    {
        var l = Crear();
        l.Pagada.Should().BeFalse();
    }

    [Fact]
    public void Crear_AnioInvalido_LanzaExcepcion()
    {
        var act = () => Crear(anio: 1990);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_AnioFuturoExtremo_LanzaExcepcion()
    {
        var act = () => Crear(anio: 2101);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_MesCero_LanzaExcepcion()
    {
        var act = () => Crear(mes: 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_Mes13_LanzaExcepcion()
    {
        var act = () => Crear(mes: 13);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarcarComoPagada_CambiaPagadaATrue()
    {
        var l = Crear();
        l.MarcarComoPagada();
        l.Pagada.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Area (Sucursales)
// ─────────────────────────────────────────────────────────────────────────────

public class AreaTests
{
    [Fact]
    public void Crear_DescripcionValida_RetornaInstancia()
    {
        var a = Area.Crear("Producción");
        a.Descripcion.Should().Be("Producción");
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Area.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CodigoSeNormalizaAMayusculas()
    {
        var a = Area.Crear("Producción", "prod");
        a.Codigo.Should().Be("PROD");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var a = Area.Crear("Producción");
        var act = () => a.Actualizar("", null, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Perfil (Sucursales)
// ─────────────────────────────────────────────────────────────────────────────

public class PerfilTests
{
    [Fact]
    public void Crear_CodigoValido_SeNormalizaAMayusculas()
    {
        var p = Perfil.Crear("admin");
        p.Codigo.Should().Be("ADMIN");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => Perfil.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CodigoVacio_LanzaExcepcion()
    {
        var p = Perfil.Crear("ADM");
        var act = () => p.Actualizar("", null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// OrdenEmpaque
// ─────────────────────────────────────────────────────────────────────────────

public class OrdenEmpaqueBusinessLogicTests
{
    private static LogisticaOrdenEmpaque OrdenValida() =>
        LogisticaOrdenEmpaque.Crear(1, null, null, null, null, null, null, null, null,
                                    1m, new DateOnly(2025, 3, 1), null, null,
                                    null, null, 5000m, null);

    [Fact]
    public void Crear_EstadoInicialPendiente()
    {
        var o = OrdenValida();
        o.Estado.Should().Be("PENDIENTE");
        o.Anulada.Should().BeFalse();
    }

    [Fact]
    public void Crear_TerceroIdCero_LanzaExcepcion()
    {
        var act = () => LogisticaOrdenEmpaque.Crear(0, null, null, null, null, null, null, null, null,
                                                    1m, new DateOnly(2025, 3, 1), null, null,
                                                    null, null, 0m, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CotizacionCero_DefaultsToUno()
    {
        var o = LogisticaOrdenEmpaque.Crear(1, null, null, null, null, null, null, null, null,
                                            0m, new DateOnly(2025, 3, 1), null, null,
                                            null, null, 0m, null);
        o.Cotizacion.Should().Be(1m);
    }

    [Fact]
    public void Confirmar_CambiaEstado()
    {
        var o = OrdenValida();
        o.Confirmar();
        o.Estado.Should().Be("CONFIRMADO");
    }

    [Fact]
    public void Confirmar_OrdenAnulada_LanzaExcepcion()
    {
        var o = OrdenValida();
        o.Anular();
        var act = () => o.Confirmar();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Anular_CambiaEstadoYDesactiva()
    {
        var o = OrdenValida();
        o.Anular();
        o.Anulada.Should().BeTrue();
        o.Estado.Should().Be("ANULADO");
    }

    [Fact]
    public void Anular_YaAnulada_LanzaExcepcion()
    {
        var o = OrdenValida();
        o.Anular();
        var act = () => o.Anular();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AgregarDetalle_ConDatosValidos_AgregaDetalleALaOrden()
    {
        var o = OrdenValida();
        o.AgregarDetalle(1L, "Producto A", 5m, 200m, null, null);
        o.Detalles.Should().HaveCount(1);
        o.Detalles.First().Descripcion.Should().Be("Producto A");
        o.Detalles.First().Total.Should().Be(1000m);
    }

    [Fact]
    public void AgregarDetalle_DescripcionVacia_LanzaExcepcion()
    {
        var o = OrdenValida();
        var act = () => o.AgregarDetalle(null, "", 5m, 200m, null, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// OrdenEmpaqueDetalle
// ─────────────────────────────────────────────────────────────────────────────

public class OrdenEmpaqueDetalleTests
{
    [Fact]
    public void Crear_ConDatosValidos_TotalCalculado()
    {
        var det = OrdenEmpaqueDetalle.Crear(1, null, "Item A", 5m, 200m, null, null);
        det.Total.Should().Be(1000m); // 5 * 200
        det.Descripcion.Should().Be("Item A");
    }

    [Fact]
    public void Crear_CantidadCero_LanzaExcepcion()
    {
        var act = () => OrdenEmpaqueDetalle.Crear(1, null, "Item A", 0m, 200m, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => OrdenEmpaqueDetalle.Crear(1, null, "", 5m, 200m, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionSeTrimea()
    {
        var det = OrdenEmpaqueDetalle.Crear(1, null, "  Item  ", 1m, 100m, null, null);
        det.Descripcion.Should().Be("Item");
    }
}
