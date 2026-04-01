using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────
// ComprobanteService
// ─────────────────────────────────────────────
public class ComprobanteServiceTests
{
    private static readonly DateOnly _fecha = new(2025, 6, 1);

    private static Comprobante CrearComprobante(long id = 1) =>
        Comprobante.Crear(1, null, 1, 1, id, _fecha, null, 1, 1, 1m, null, null);

    [Fact]
    public async Task ImputarAsync_OrigenNoEncontrado_LanzaExcepcion()
    {
        var comprobanteRepo = Substitute.For<IComprobanteRepository>();
        var imputacionRepo  = Substitute.For<IImputacionRepository>();

        comprobanteRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Comprobante?)null);

        var svc = new ComprobanteService(comprobanteRepo, imputacionRepo);

        var act = async () => await svc.ImputarAsync(1, 2, 100m, _fecha, null, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*origen*");
    }

    [Fact]
    public async Task ImputarAsync_DestinoNoEncontrado_LanzaExcepcion()
    {
        var comprobanteRepo = Substitute.For<IComprobanteRepository>();
        var imputacionRepo  = Substitute.For<IImputacionRepository>();

        var origen = CrearComprobante(1);
        comprobanteRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(origen);
        comprobanteRepo.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns((Comprobante?)null);

        var svc = new ComprobanteService(comprobanteRepo, imputacionRepo);

        var act = async () => await svc.ImputarAsync(1, 2, 100m, _fecha, null, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*destino*");
    }

    [Fact]
    public async Task ImputarAsync_ImporteMayorSaldoOrigen_LanzaExcepcion()
    {
        var comprobanteRepo = Substitute.For<IComprobanteRepository>();
        var imputacionRepo  = Substitute.For<IImputacionRepository>();

        // Saldo de origen = 0 (default en Comprobante sin ítems)
        var origen  = CrearComprobante(1);
        var destino = CrearComprobante(2);

        comprobanteRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(origen);
        comprobanteRepo.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(destino);

        var svc = new ComprobanteService(comprobanteRepo, imputacionRepo);

        // importe 500 > origen.Saldo (0) → lanza
        var act = async () => await svc.ImputarAsync(1, 2, 500m, _fecha, null, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*saldo*");
    }

    [Fact]
    public async Task ImputarAsync_ImporteMayorSaldoDestino_LanzaExcepcion()
    {
        var comprobanteRepo = Substitute.For<IComprobanteRepository>();
        var imputacionRepo  = Substitute.For<IImputacionRepository>();

        var origen  = CrearComprobante(1);
        var destino = CrearComprobante(2);

        // Dar saldo al origen via percepciones (que llama RecalcularTotales → Saldo = percepciones)
        origen.SetPercepciones(1000m, null);

        comprobanteRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(origen);
        comprobanteRepo.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(destino);

        var svc = new ComprobanteService(comprobanteRepo, imputacionRepo);

        // importe 500 ≤ origen.Saldo(1000) pero > destino.Saldo(0)
        var act = async () => await svc.ImputarAsync(1, 2, 500m, _fecha, null, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*saldo*");
    }
}

// ─────────────────────────────────────────────
// ContabilidadService
// ─────────────────────────────────────────────
public class ContabilidadServiceTests
{
    private static readonly DateOnly _fechaDentro  = new(2025, 6, 15);
    private static readonly DateOnly _fechaFuera   = new(2030, 1, 1);

    private static Ejercicio CrearEjercicioAbierto() =>
        Ejercicio.Crear("Ej 2025", new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

    private static IReadOnlyList<(long, decimal, decimal, string?, long?)> LineasBalanceadas() =>
        [(1, 1000m, 0m, null, null), (2, 0m, 1000m, null, null)];

    private static IReadOnlyList<(long, decimal, decimal, string?, long?)> LineasDesbalanceadas() =>
        [(1, 1000m, 0m, null, null), (2, 0m, 999m, null, null)];

    [Fact]
    public async Task RegistrarAsientoAsync_EjercicioNoEncontrado_LanzaExcepcion()
    {
        var asientoRepo   = Substitute.For<IAsientoRepository>();
        var ejercicioRepo = Substitute.For<IEjercicioRepository>();

        ejercicioRepo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Ejercicio?)null);

        var svc = new ContabilidadService(asientoRepo, ejercicioRepo);

        var act = async () => await svc.RegistrarAsientoAsync(
            99, 1, _fechaDentro, "Test", null, null, LineasBalanceadas(), null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ejercicio*");
    }

    [Fact]
    public async Task RegistrarAsientoAsync_EjercicioCerrado_LanzaExcepcion()
    {
        var asientoRepo   = Substitute.For<IAsientoRepository>();
        var ejercicioRepo = Substitute.For<IEjercicioRepository>();

        var ejercicio = CrearEjercicioAbierto();
        ejercicio.Cerrar();

        ejercicioRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ejercicio);

        var svc = new ContabilidadService(asientoRepo, ejercicioRepo);

        var act = async () => await svc.RegistrarAsientoAsync(
            1, 1, _fechaDentro, "Test", null, null, LineasBalanceadas(), null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cerrado*");
    }

    [Fact]
    public async Task RegistrarAsientoAsync_FechaFueraDeEjercicio_LanzaExcepcion()
    {
        var asientoRepo   = Substitute.For<IAsientoRepository>();
        var ejercicioRepo = Substitute.For<IEjercicioRepository>();

        var ejercicio = CrearEjercicioAbierto();
        ejercicioRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ejercicio);

        var svc = new ContabilidadService(asientoRepo, ejercicioRepo);

        var act = async () => await svc.RegistrarAsientoAsync(
            1, 1, _fechaFuera, "Test", null, null, LineasBalanceadas(), null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ejercicio*");
    }

    [Fact]
    public async Task RegistrarAsientoAsync_AsientoNoBalancea_LanzaExcepcion()
    {
        var asientoRepo   = Substitute.For<IAsientoRepository>();
        var ejercicioRepo = Substitute.For<IEjercicioRepository>();

        var ejercicio = CrearEjercicioAbierto();
        ejercicioRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ejercicio);
        asientoRepo.GetProximoNumeroAsync(1, 1, Arg.Any<CancellationToken>()).Returns(1L);

        var svc = new ContabilidadService(asientoRepo, ejercicioRepo);

        var act = async () => await svc.RegistrarAsientoAsync(
            1, 1, _fechaDentro, "Test", null, null, LineasDesbalanceadas(), null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*balancea*");
    }

    [Fact]
    public async Task RegistrarAsientoAsync_DatosValidos_RetornaAsiento()
    {
        var asientoRepo   = Substitute.For<IAsientoRepository>();
        var ejercicioRepo = Substitute.For<IEjercicioRepository>();

        var ejercicio = CrearEjercicioAbierto();
        ejercicioRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ejercicio);
        asientoRepo.GetProximoNumeroAsync(1, 1, Arg.Any<CancellationToken>()).Returns(1L);

        var svc = new ContabilidadService(asientoRepo, ejercicioRepo);

        var asiento = await svc.RegistrarAsientoAsync(
            1, 1, _fechaDentro, "Asiento test", null, null, LineasBalanceadas(), null);

        asiento.Should().NotBeNull();
        asiento.Balancea.Should().BeTrue();
        await asientoRepo.Received(1).AddAsync(Arg.Any<Asiento>(), Arg.Any<CancellationToken>());
    }
}

// ─────────────────────────────────────────────
// CuentaCorrienteService
// ─────────────────────────────────────────────
public class CuentaCorrienteServiceTests
{
    private static readonly DateOnly _fecha = new(2025, 6, 1);

    [Fact]
    public async Task DebitarAsync_DatosValidos_CreaMovimientoDebito()
    {
        var ctaCteRepo    = Substitute.For<ICuentaCorrienteRepository>();
        var movimientoRepo = Substitute.For<IMovimientoCtaCteRepository>();

        var cta = CuentaCorriente.Crear(1, null, 1);
        ctaCteRepo.GetOrCreateAsync(1, 1, null, Arg.Any<CancellationToken>()).Returns(cta);

        var svc = new CuentaCorrienteService(ctaCteRepo, movimientoRepo);

        var mov = await svc.DebitarAsync(1, null, 1, 500m, null, _fecha, "Factura");

        mov.Should().NotBeNull();
        mov.Debe.Should().Be(500m);
        cta.Saldo.Should().Be(500m);
        ctaCteRepo.Received(1).Update(cta);
        await movimientoRepo.Received(1).AddAsync(Arg.Any<MovimientoCtaCte>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AcreditarAsync_DatosValidos_CreaMovimientoCredito()
    {
        var ctaCteRepo    = Substitute.For<ICuentaCorrienteRepository>();
        var movimientoRepo = Substitute.For<IMovimientoCtaCteRepository>();

        var cta = CuentaCorriente.Crear(1, null, 1);
        cta.Debitar(1000m); // dar saldo positivo antes de acreditar
        ctaCteRepo.GetOrCreateAsync(1, 1, null, Arg.Any<CancellationToken>()).Returns(cta);

        var svc = new CuentaCorrienteService(ctaCteRepo, movimientoRepo);

        var mov = await svc.AcreditarAsync(1, null, 1, 300m, null, _fecha, "Pago");

        mov.Should().NotBeNull();
        mov.Haber.Should().Be(300m);
        cta.Saldo.Should().Be(700m);
        ctaCteRepo.Received(1).Update(cta);
        await movimientoRepo.Received(1).AddAsync(Arg.Any<MovimientoCtaCte>(), Arg.Any<CancellationToken>());
    }
}

// ─────────────────────────────────────────────
// NumeracionComprobanteService
// ─────────────────────────────────────────────
public class NumeracionComprobanteServiceTests
{
    [Fact]
    public async Task ObtenerProximoNumeroAsync_PuntoNoEncontrado_LanzaExcepcion()
    {
        var puntoRepo = Substitute.For<IPuntoFacturacionRepository>();
        puntoRepo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((PuntoFacturacion?)null);

        var svc = new NumeracionComprobanteService(puntoRepo);

        var act = async () => await svc.ObtenerProximoNumeroAsync(99, 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*punto de facturación*");
    }

    [Fact]
    public async Task ObtenerProximoNumeroAsync_PuntoNoActivo_LanzaExcepcion()
    {
        var puntoRepo = Substitute.For<IPuntoFacturacionRepository>();

        var punto = PuntoFacturacion.Crear(1, 1, 1, null, null);
        punto.Desactivar(null);

        puntoRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(punto);

        var svc = new NumeracionComprobanteService(puntoRepo);

        var act = async () => await svc.ObtenerProximoNumeroAsync(1, 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*activo*");
    }

    [Fact]
    public async Task ObtenerProximoNumeroAsync_PuntoActivo_RetornaPrefijoyNumero()
    {
        var puntoRepo = Substitute.For<IPuntoFacturacionRepository>();

        var punto = PuntoFacturacion.Crear(1, 1, 5, null, null); // Numero = 5
        puntoRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(punto);
        puntoRepo.GetProximoNumeroComprobanteAsync(1, 1, Arg.Any<CancellationToken>()).Returns(42L);

        var svc = new NumeracionComprobanteService(puntoRepo);

        var (prefijo, numero) = await svc.ObtenerProximoNumeroAsync(1, 1);

        prefijo.Should().Be((short)5);
        numero.Should().Be(42L);
    }
}

// ─────────────────────────────────────────────
// PermisoService
// ─────────────────────────────────────────────
public class PermisoServiceTests
{
    [Fact]
    public async Task TienePermisoAsync_SeguridadNoEncontrada_RetornaFalse()
    {
        var seguridadRepo = Substitute.For<ISeguridadRepository>();
        seguridadRepo.GetByIdentificadorAsync("PERMISO.X", Arg.Any<CancellationToken>())
            .Returns((Seguridad?)null);

        var svc = new PermisoService(seguridadRepo);

        var result = await svc.TienePermisoAsync(1, "PERMISO.X");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TienePermisoAsync_SinAplicarSeguridad_RetornaTrue()
    {
        var seguridadRepo = Substitute.For<ISeguridadRepository>();

        var seg = Seguridad.Crear("PERMISO.Y", null, aplicaSeguridadPorUsuario: false);
        seguridadRepo.GetByIdentificadorAsync("PERMISO.Y", Arg.Any<CancellationToken>())
            .Returns(seg);

        var svc = new PermisoService(seguridadRepo);

        var result = await svc.TienePermisoAsync(1, "PERMISO.Y");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TienePermisoAsync_ConAplicarSeguridad_DelegaARepositorio()
    {
        var seguridadRepo = Substitute.For<ISeguridadRepository>();

        var seg = Seguridad.Crear("PERMISO.Z", null, aplicaSeguridadPorUsuario: true);
        seguridadRepo.GetByIdentificadorAsync("PERMISO.Z", Arg.Any<CancellationToken>())
            .Returns(seg);
        seguridadRepo.TienePermisoAsync(5, "PERMISO.Z", Arg.Any<CancellationToken>())
            .Returns(true);

        var svc = new PermisoService(seguridadRepo);

        var result = await svc.TienePermisoAsync(5, "PERMISO.Z");

        result.Should().BeTrue();
        await seguridadRepo.Received(1).TienePermisoAsync(5, "PERMISO.Z", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPermisosAsync_DelegaARepositorio()
    {
        var seguridadRepo = Substitute.For<ISeguridadRepository>();
        var permisos = new Dictionary<string, bool> { { "PERMISO.A", true }, { "PERMISO.B", false } };
        seguridadRepo.GetPermisosUsuarioAsync(1, Arg.Any<CancellationToken>()).Returns(permisos);

        var svc = new PermisoService(seguridadRepo);

        var result = await svc.GetPermisosAsync(1);

        result.Should().BeEquivalentTo(permisos);
    }
}

// ─────────────────────────────────────────────
// StockService
// ─────────────────────────────────────────────
public class StockServiceTests
{
    private static (IStockRepository, IMovimientoStockRepository, StockService) CrearServicio()
    {
        var stockRepo     = Substitute.For<IStockRepository>();
        var movimientoRepo = Substitute.For<IMovimientoStockRepository>();
        var svc           = new StockService(stockRepo, movimientoRepo);
        return (stockRepo, movimientoRepo, svc);
    }

    [Fact]
    public async Task TransferirAsync_MismoDeposito_LanzaExcepcion()
    {
        var (_, _, svc) = CrearServicio();

        var act = async () => await svc.TransferirAsync(1, 5, 5, 10m, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*mismo*");
    }

    [Fact]
    public async Task IngresarAsync_DatosValidos_CreaMovimientoIngreso()
    {
        var (stockRepo, movimientoRepo, svc) = CrearServicio();

        var stock = StockItem.Crear(1, 1);
        stockRepo.GetOrCreateAsync(1, 1, Arg.Any<CancellationToken>()).Returns(stock);

        var mov = await svc.IngresarAsync(1, 1, 10m, TipoMovimientoStock.CompraRecepcion,
            null, null, null, null);

        mov.Should().NotBeNull();
        mov.Cantidad.Should().Be(10m);
        stock.Cantidad.Should().Be(10m);
        stockRepo.Received(1).Update(stock);
        await movimientoRepo.Received(1).AddAsync(Arg.Any<MovimientoStock>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EgresarAsync_DatosValidos_CreaMovimientoEgreso()
    {
        var (stockRepo, movimientoRepo, svc) = CrearServicio();

        var stock = StockItem.Crear(1, 1, 50m);
        stockRepo.GetOrCreateAsync(1, 1, Arg.Any<CancellationToken>()).Returns(stock);

        var mov = await svc.EgresarAsync(1, 1, 20m, TipoMovimientoStock.VentaDespacho,
            null, null, null, null);

        mov.Should().NotBeNull();
        mov.Cantidad.Should().Be(-20m);
        stock.Cantidad.Should().Be(30m);
        await movimientoRepo.Received(1).AddAsync(Arg.Any<MovimientoStock>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AjustarAsync_CantidadMayor_CreaMovimientoPositivo()
    {
        var (stockRepo, movimientoRepo, svc) = CrearServicio();

        var stock = StockItem.Crear(1, 1, 10m);
        stockRepo.GetOrCreateAsync(1, 1, Arg.Any<CancellationToken>()).Returns(stock);

        var mov = await svc.AjustarAsync(1, 1, 30m, null, null);

        mov.Should().NotBeNull();
        mov.TipoMovimiento.Should().Be(TipoMovimientoStock.AjustePositivo);
        stock.Cantidad.Should().Be(30m);
    }

    [Fact]
    public async Task AjustarAsync_CantidadMenor_CreaMovimientoNegativo()
    {
        var (stockRepo, movimientoRepo, svc) = CrearServicio();

        var stock = StockItem.Crear(1, 1, 50m);
        stockRepo.GetOrCreateAsync(1, 1, Arg.Any<CancellationToken>()).Returns(stock);

        var mov = await svc.AjustarAsync(1, 1, 20m, null, null);

        mov.Should().NotBeNull();
        mov.TipoMovimiento.Should().Be(TipoMovimientoStock.AjusteNegativo);
        stock.Cantidad.Should().Be(20m);
    }

    [Fact]
    public async Task TransferirAsync_DepositosDiferentes_EjecutaEgresoEIngreso()
    {
        var (stockRepo, movimientoRepo, svc) = CrearServicio();

        var stockOrigen = StockItem.Crear(1, 1, 100m);
        var stockDestino = StockItem.Crear(1, 2);
        stockRepo.GetOrCreateAsync(1, 1, Arg.Any<CancellationToken>()).Returns(stockOrigen);
        stockRepo.GetOrCreateAsync(1, 2, Arg.Any<CancellationToken>()).Returns(stockDestino);

        await svc.TransferirAsync(1, 1, 2, 40m, null, null);

        stockOrigen.Cantidad.Should().Be(60m);
        stockDestino.Cantidad.Should().Be(40m);
        await movimientoRepo.Received(2).AddAsync(Arg.Any<MovimientoStock>(), Arg.Any<CancellationToken>());
    }
}

// ─────────────────────────────────────────────
// ProduccionService
// ─────────────────────────────────────────────
public class ProduccionServiceTests
{
    private static OrdenTrabajo CrearOrdenTrabajo(long formulaId = 1) =>
        OrdenTrabajo.Crear(1, formulaId, 1, 2, new DateOnly(2025, 6, 1), null, 10m, null, null);

    [Fact]
    public async Task EjecutarProduccionAsync_FormulaNoEncontrada_LanzaExcepcion()
    {
        var formulaRepo = Substitute.For<IFormulaProduccionRepository>();
        var stockRepo   = Substitute.For<IStockRepository>();
        var movRepo     = Substitute.For<IMovimientoStockRepository>();

        formulaRepo.GetByIdConIngredientesAsync(1, Arg.Any<CancellationToken>())
            .Returns((FormulaProduccion?)null);

        var stockSvc = new StockService(stockRepo, movRepo);
        var consumoRepo = Substitute.For<IRepository<OrdenTrabajoConsumo>>();
        var svc      = new ProduccionService(formulaRepo, stockSvc, consumoRepo);

        var ot  = CrearOrdenTrabajo(1);
        var act = async () => await svc.EjecutarProduccionAsync(ot, null, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*fórmula*");
    }

    [Fact]
    public async Task EjecutarProduccionAsync_FormulaValida_IngresaProductoTerminado()
    {
        var formulaRepo = Substitute.For<IFormulaProduccionRepository>();
        var stockRepo   = Substitute.For<IStockRepository>();
        var movRepo     = Substitute.For<IMovimientoStockRepository>();

        var formula = FormulaProduccion.Crear("FORM1", "Formula test", 99, 5m, null, null, null);
        // Sin ingredientes no opcionales, sólo se ingresa producto terminado
        formulaRepo.GetByIdConIngredientesAsync(1, Arg.Any<CancellationToken>())
            .Returns(formula);

        var stockProducto = StockItem.Crear(99, 2); // deposito destino = 2
        stockRepo.GetOrCreateAsync(99, 2, Arg.Any<CancellationToken>()).Returns(stockProducto);

        var stockSvc = new StockService(stockRepo, movRepo);
        var consumoRepo = Substitute.For<IRepository<OrdenTrabajoConsumo>>();
        var svc      = new ProduccionService(formulaRepo, stockSvc, consumoRepo);

        var ot = CrearOrdenTrabajo(1);  // cantidad = 10, depositoDestino = 2
        await svc.EjecutarProduccionAsync(ot, null, null, null);

        // OT produce 10 unidades (factor = 10/5 = 2), ingresa producto terminado al deposito 2
        stockProducto.Cantidad.Should().Be(10m);
        await movRepo.Received(1).AddAsync(Arg.Any<MovimientoStock>(), Arg.Any<CancellationToken>());
    }
}
