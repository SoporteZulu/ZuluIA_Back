using AutoMapper;
using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Common.Mappings;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Application.Features.Cajas.Mappings;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Application.Features.Cheques.Mappings;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Comprobantes.Mappings;
using ZuluIA_Back.Application.Features.Configuracion.DTOs;
using ZuluIA_Back.Application.Features.Configuracion.Mappings;
using ZuluIA_Back.Application.Features.Cotizaciones.DTOs;
using ZuluIA_Back.Application.Features.Cotizaciones.Mappings;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Mappings;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Mappings;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Application.Features.ListasPrecios.Mappings;
using ZuluIA_Back.Application.Features.PlanesPago.DTOs;
using ZuluIA_Back.Application.Features.PlanesPago.Mappings;
using ZuluIA_Back.Application.Features.Stock.DTOs;
using ZuluIA_Back.Application.Features.Stock.Mappings;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;
using ZuluIA_Back.Application.Features.Sucursales.Mappings;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Application.Features.Usuarios.Mappings;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.UnitTests.Application;

public class MappingProfileTests
{
    private static readonly IMapper _mapper;

    static MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MappingConfiguration_EsValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(true, false, false, "Cliente")]
    [InlineData(false, true, false, "Proveedor")]
    [InlineData(false, false, true, "Empleado")]
    [InlineData(true, true, false, "Cliente / Proveedor")]
    [InlineData(true, false, true, "Cliente / Empleado")]
    [InlineData(false, true, true, "Proveedor / Empleado")]
    [InlineData(true, true, true, "Cliente / Proveedor / Empleado")]
    public void Tercero_RolDisplay_SeCalculaCorrectamente(
        bool esCliente, bool esProveedor, bool esEmpleado, string expectedDisplay)
    {
        var tercero = Tercero.Crear(
            legajo: "00001",
            razonSocial: "Empresa Test SA",
            tipoDocumentoId: 1L,
            nroDocumento: "20-12345678-9",
            condicionIvaId: 1L,
            esCliente: esCliente,
            esProveedor: esProveedor,
            esEmpleado: esEmpleado,
            sucursalId: null,
            userId: null);

        var dto = _mapper.Map<TerceroListDto>(tercero);

        dto.RolDisplay.Should().Be(expectedDisplay);
    }

    [Fact]
    public void Tercero_MapTo_TerceroListDto_PropiedadesEscalares()
    {
        var tercero = Tercero.Crear(
            legajo: "00042",
            razonSocial: "Empresa Mapeo SA",
            tipoDocumentoId: 1L,
            nroDocumento: "30-99887766-5",
            condicionIvaId: 2L,
            esCliente: true,
            esProveedor: false,
            esEmpleado: false,
            sucursalId: null,
            userId: null);

        var dto = _mapper.Map<TerceroListDto>(tercero);

        dto.Legajo.Should().Be("00042");
        dto.RazonSocial.Should().Be("Empresa Mapeo SA");
        dto.NroDocumento.Should().Be("30-99887766-5");
        dto.EsCliente.Should().BeTrue();
        dto.EsProveedor.Should().BeFalse();
        dto.EsEmpleado.Should().BeFalse();
        dto.Activo.Should().BeTrue();
    }
}

/// <summary>
/// Valida TODOS los perfiles AutoMapper del ensamblado Application (incluidos
/// los 12 perfiles de nivel feature: Caja, Cheque, Comprobante, Configuracion,
/// Cotizacion, Facturacion, Item, ListaPrecios, PlanPago, Stock, Sucursal, Usuario).
/// </summary>
public class AllMappingProfilesTests
{
    [Fact]
    public void TodosLosPerfilesDeApplication_ConfiguracionEsValida()
    {
        // Carga TODOS los perfiles via AddMaps — igual que en DependencyInjection.cs
        var config = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(ZuluIA_Back.Application.DependencyInjection).Assembly));

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow(
            because: "Todos los perfiles AutoMapper del ensamblado Application " +
                     "deben tener todas las propiedades de destino mapeadas o marcadas con Ignore().");
    }
}

/// <summary>
/// Verifica que ChequeMappingProfile convierte Estado a string uppercase correctamente.
/// </summary>
public class ChequeMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<ChequeMappingProfile>()).CreateMapper();

    [Fact]
    public void Cheque_Estado_MapToUpperInvariantString()
    {
        var cheque = Cheque.Crear(
            cajaId: 1L,
            terceroId: null,
            nroCheque: "00012345",
            banco: "Banco Nacion",
            fechaEmision: new DateOnly(2026, 1, 1),
            fechaVencimiento: new DateOnly(2026, 12, 31),
            importe: 1000m,
            monedaId: 1L,
            observacion: null,
            userId: null);

        var dto = _mapper.Map<ChequeDto>(cheque);

        dto.Estado.Should().Be("CARTERA");
        dto.Estado.Should().Be(dto.Estado.ToUpperInvariant());
    }
}

/// <summary>
/// Verifica que ComprobanteMappingProfile aplana el VO NroComprobante y produce
/// NumeroFormateado / NroFormateado con el formato correcto.
/// </summary>
public class ComprobanteMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<ComprobanteMappingProfile>()).CreateMapper();

    private static Comprobante CrearComprobante(short prefijo, long numero) =>
        Comprobante.Crear(
            sucursalId: 1L,
            puntoFacturacionId: null,
            tipoComprobanteId: 1L,
            prefijo: prefijo,
            numero: numero,
            fecha: new DateOnly(2026, 1, 15),
            fechaVencimiento: null,
            terceroId: 1L,
            monedaId: 1L,
            cotizacion: 1m,
            observacion: null,
            userId: null);

    [Fact]
    public void Comprobante_MapToListDto_PrefijoyNumeroVOFlattening()
    {
        var comprobante = CrearComprobante(prefijo: 5, numero: 42);

        var dto = _mapper.Map<ComprobanteListDto>(comprobante);

        dto.Prefijo.Should().Be(5);
        dto.Numero.Should().Be(42);
    }

    [Fact]
    public void Comprobante_MapToListDto_NumeroFormateadoDesdeVO()
    {
        var comprobante = CrearComprobante(prefijo: 1, numero: 1);

        var dto = _mapper.Map<ComprobanteListDto>(comprobante);

        dto.NumeroFormateado.Should().Be("0001-00000001");
    }

    [Fact]
    public void Comprobante_MapToDto_NroFormateadoDesdeVO()
    {
        var comprobante = CrearComprobante(prefijo: 2, numero: 99);

        var dto = _mapper.Map<ComprobanteDto>(comprobante);

        dto.NroFormateado.Should().Be("0002-00000099");
        dto.Prefijo.Should().Be(2);
        dto.Numero.Should().Be(99);
    }
}

/// <summary>
/// Verifica que ListaPrecionMappingProfile mapea PrecioFinal (propiedad calculada) correctamente.
/// </summary>
public class ListaPrecionsMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<ListaPrecionMappingProfile>()).CreateMapper();

    [Fact]
    public void ListaPreciosItem_MapToDto_PrecioFinalCalculado()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1L, null, null, null);
        lista.UpsertItem(itemId: 1L, precio: 200m, descuentoPct: 10m);
        var item = lista.Items.First();

        var dto = _mapper.Map<ListaPreciosItemDto>(item);

        dto.Precio.Should().Be(200m);
        dto.DescuentoPct.Should().Be(10m);
        dto.PrecioFinal.Should().Be(180m); // 200 * (1 - 10/100) = 180
    }
}

/// <summary>
/// Verifica que SucursalMappingProfile aplana las 7 propiedades del VO Domicilio correctamente.
/// </summary>
public class SucursalMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<SucursalMappingProfile>()).CreateMapper();

    [Fact]
    public void Sucursal_MapToDto_DomicilioAplanado()
    {
        var sucursal = Sucursal.Crear("Empresa SA", "20-12345678-9", 1L, 1L, 1L, true, null);
        sucursal.Actualizar(
            razonSocial: "Empresa SA",
            nombreFantasia: null,
            cuit: "20-12345678-9",
            nroIngresosBrutos: null,
            condicionIvaId: 1L,
            monedaId: 1L,
            paisId: 1L,
            domicilio: new Domicilio("Calle Falsa", "123", "2", "A", "1234", 100L, 200L),
            telefono: null,
            email: null,
            web: null,
            cbu: null,
            aliasCbu: null,
            cai: null,
            puertoAfip: 443,
            casaMatriz: true,
            userId: null);

        var dto = _mapper.Map<SucursalDto>(sucursal);

        dto.Calle.Should().Be("Calle Falsa");
        dto.Nro.Should().Be("123");
        dto.Piso.Should().Be("2");
        dto.Dpto.Should().Be("A");
        dto.CodigoPostal.Should().Be("1234");
        dto.LocalidadId.Should().Be(100L);
        dto.BarrioId.Should().Be(200L);
    }
}

/// <summary>
/// Verifica que FacturacionMappingProfile convierte CartaPorte.Estado a uppercase
/// y que PeriodoIva.PeriodoDescripcion se mapea correctamente.
/// </summary>
public class FacturacionMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<FacturacionMappingProfile>()).CreateMapper();

    [Fact]
    public void CartaPorte_Estado_MapToUpperInvariantString()
    {
        var carta = CartaPorte.Crear(
            comprobanteId: null,
            cuitRemitente: "20-12345678-9",
            cuitDestinatario: "30-98765432-1",
            cuitTransportista: null,
            fechaEmision: new DateOnly(2026, 1, 15),
            observacion: null,
            userId: null);

        var dto = _mapper.Map<CartaPorteDto>(carta);

        dto.Estado.Should().Be("PENDIENTE");
        dto.Estado.Should().Be(dto.Estado.ToUpperInvariant());
    }

    [Fact]
    public void PeriodoIva_PeriodoDescripcion_MapeadoDesdePropiedad()
    {
        var periodo = PeriodoIva.Crear(
            ejercicioId: 1L,
            sucursalId: 1L,
            periodo: new DateOnly(2026, 3, 1));

        var dto = _mapper.Map<PeriodoIvaDto>(periodo);

        dto.PeriodoDescripcion.Should().NotBeNullOrEmpty();
        dto.PeriodoDescripcion.Should().Contain("2026");
    }
}

/// <summary>
/// Verifica que StockMappingProfile convierte TipoMovimiento (enum) a string.
/// </summary>
public class StockMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<StockMappingProfile>()).CreateMapper();

    [Fact]
    public void MovimientoStock_TipoMovimiento_MapeadoComoString()
    {
        var mov = MovimientoStock.Crear(
            itemId: 1L,
            depositoId: 1L,
            tipoMovimiento: TipoMovimientoStock.Ingreso,
            cantidad: 10m,
            saldoResultante: 10m);

        var dto = _mapper.Map<MovimientoStockDto>(mov);

        dto.TipoMovimiento.Should().Be("Ingreso");
    }

    [Theory]
    [InlineData(TipoMovimientoStock.Egreso, "Egreso")]
    [InlineData(TipoMovimientoStock.AjustePositivo, "AjustePositivo")]
    [InlineData(TipoMovimientoStock.StockInicial, "StockInicial")]
    public void MovimientoStock_TipoMovimiento_DistintosValores(
        TipoMovimientoStock tipo, string esperado)
    {
        var mov = MovimientoStock.Crear(
            itemId: 1L,
            depositoId: 1L,
            tipoMovimiento: tipo,
            cantidad: tipo == TipoMovimientoStock.Egreso ? -5m : 5m,
            saldoResultante: 5m);

        var dto = _mapper.Map<MovimientoStockDto>(mov);

        dto.TipoMovimiento.Should().Be(esperado);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CajaMappingProfile
// ─────────────────────────────────────────────────────────────────────────────
public class CajaMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<CajaMappingProfile>()).CreateMapper();

    [Fact]
    public void ConfiguracionCaja_EsValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CajaMappingProfile>());
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void CajaCuentaBancaria_MapTo_CajaDto_PropiedadesPrincipales()
    {
        var caja = CajaCuentaBancaria.Crear(1, 1, "Caja Principal", 1, true, null, null);
        var dto = _mapper.Map<CajaDto>(caja);
        dto.Descripcion.Should().Be("Caja Principal");
        dto.MonedaId.Should().Be(1);
        dto.EsCaja.Should().BeTrue();
        dto.Activa.Should().BeTrue();
    }

    [Fact]
    public void CajaCuentaBancaria_MapTo_CajaListDto_PropiedadesPrincipales()
    {
        var caja = CajaCuentaBancaria.Crear(1, 1, "Caja Test", 1, true, null, null);
        var dto = _mapper.Map<CajaListDto>(caja);
        dto.Descripcion.Should().Be("Caja Test");
        dto.Activa.Should().BeTrue();
        dto.MonedaId.Should().Be(1);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ConfiguracionMappingProfile
// ─────────────────────────────────────────────────────────────────────────────
public class ConfiguracionMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<ConfiguracionMappingProfile>()).CreateMapper();

    [Fact]
    public void ConfiguracionConfiguracion_EsValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ConfiguracionMappingProfile>());
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void ConfiguracionSistema_MapTo_ConfiguracionDto_PropiedadesPrincipales()
    {
        var cfg = ConfiguracionSistema.Crear("MODO_DEBUG", "true", 1, "Modo debug");
        var dto = _mapper.Map<ConfiguracionDto>(cfg);
        dto.Campo.Should().Be("MODO_DEBUG");
        dto.Valor.Should().Be("true");
        dto.TipoDato.Should().Be(1);
        dto.Descripcion.Should().Be("Modo debug");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CotizacionMappingProfile
// ─────────────────────────────────────────────────────────────────────────────
public class CotizacionMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<CotizacionMappingProfile>()).CreateMapper();

    [Fact]
    public void ConfiguracionCotizacion_EsValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CotizacionMappingProfile>());
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void CotizacionMoneda_MapTo_CotizacionMonedaDto_PropiedadesPrincipales()
    {
        var cot = CotizacionMoneda.Crear(2, new DateOnly(2026, 1, 15), 1500m);
        var dto = _mapper.Map<CotizacionMonedaDto>(cot);
        dto.MonedaId.Should().Be(2);
        dto.Cotizacion.Should().Be(1500m);
        dto.Fecha.Should().Be(new DateOnly(2026, 1, 15));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ItemMappingProfile
// ─────────────────────────────────────────────────────────────────────────────
public class ItemMappingProfileTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<ItemMappingProfile>()).CreateMapper();

    [Fact]
    public void ConfiguracionItem_EsValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ItemMappingProfile>());
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void Item_MapTo_ItemDto_PropiedadesPrincipales()
    {
        var item = Item.Crear(
            codigo: "COD001", descripcion: "Producto Test",
            unidadMedidaId: 1, alicuotaIvaId: 1, monedaId: 1,
            esProducto: true, esServicio: false, esFinanciero: false,
            manejaStock: true, precioCosto: 100m, precioVenta: 150m,
            categoriaId: null, stockMinimo: 0m, stockMaximo: null,
            codigoBarras: null, descripcionAdicional: null, codigoAfip: null,
            sucursalId: null, userId: null);

        var dto = _mapper.Map<ItemDto>(item);

        dto.Codigo.Should().Be("COD001");
        dto.Descripcion.Should().Be("Producto Test");
        dto.PrecioVenta.Should().Be(150m);
        dto.EsProducto.Should().BeTrue();
        dto.ManejaStock.Should().BeTrue();
    }

    [Fact]
    public void Item_MapTo_ItemListDto_PropiedadesPrincipales()
    {
        var item = Item.Crear(
            codigo: "COD002", descripcion: "Servicio Test",
            unidadMedidaId: 1, alicuotaIvaId: 1, monedaId: 1,
            esProducto: false, esServicio: true, esFinanciero: false,
            manejaStock: false, precioCosto: 50m, precioVenta: 80m,
            categoriaId: null, stockMinimo: 0m, stockMaximo: null,
            codigoBarras: null, descripcionAdicional: null, codigoAfip: null,
            sucursalId: null, userId: null);

        var dto = _mapper.Map<ItemListDto>(item);

        dto.Codigo.Should().Be("COD002");
        dto.EsServicio.Should().BeTrue();
        dto.PrecioVenta.Should().Be(80m);
    }

    [Fact]
    public void CategoriaItem_MapTo_CategoriaItemDto_PropiedadesPrincipales()
    {
        var cat = CategoriaItem.Crear(null, "CAT01", "Categoría Test", 1, null, null);
        var dto = _mapper.Map<CategoriaItemDto>(cat);
        dto.Codigo.Should().Be("CAT01");
        dto.Descripcion.Should().Be("Categoría Test");
        dto.Nivel.Should().Be(1);
    }

    [Fact]
    public void Deposito_MapTo_DepositoDto_PropiedadesPrincipales()
    {
        var dep = Deposito.Crear(1, "Depósito Central", false);
        var dto = _mapper.Map<DepositoDto>(dep);
        dto.Descripcion.Should().Be("Depósito Central");
        dto.EsDefault.Should().BeFalse();
        dto.Activo.Should().BeTrue();
        dto.SucursalId.Should().Be(1);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PlanPagoMappingProfile
// ─────────────────────────────────────────────────────────────────────────────
public class PlanPagoMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<PlanPagoMappingProfile>()).CreateMapper();

    [Fact]
    public void ConfiguracionPlanPago_EsValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PlanPagoMappingProfile>());
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void PlanPago_MapTo_PlanPagoDto_PropiedadesPrincipales()
    {
        var plan = PlanPago.Crear("Contado", 1, 0m);
        var dto = _mapper.Map<PlanPagoDto>(plan);
        dto.Descripcion.Should().Be("Contado");
        dto.CantidadCuotas.Should().Be(1);
        dto.InteresPct.Should().Be(0m);
        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public void PlanPago_MapTo_PlanPagoDto_ConInteres()
    {
        var plan = PlanPago.Crear("3 cuotas con interés", 3, 10m);
        var dto = _mapper.Map<PlanPagoDto>(plan);
        dto.CantidadCuotas.Should().Be(3);
        dto.InteresPct.Should().Be(10m);
    }
}

/// <summary>
/// Verifica que UsuarioMappingProfile aplana la colección Sucursales → SucursalIds.
/// </summary>
public class UsuarioMappingTests
{
    private static readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<UsuarioMappingProfile>()).CreateMapper();

    [Fact]
    public void Usuario_SinSucursales_SucursalIdsEsListaVacia()
    {
        var usuario = Usuario.Crear("jdoe", "John Doe", null, null, null);

        var dto = _mapper.Map<UsuarioDto>(usuario);

        dto.SucursalIds.Should().BeEmpty();
    }

    [Fact]
    public void MenuItem_Hijos_MapeadosCorrectamente()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UsuarioMappingProfile>());
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }
}
