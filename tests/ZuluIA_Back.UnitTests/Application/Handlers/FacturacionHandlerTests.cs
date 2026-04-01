using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Caea.Commands;
using ZuluIA_Back.Application.Features.Caea.Queries;
using ZuluIA_Back.Application.Features.Auditoria.Queries;
using ZuluIA_Back.Application.Features.Comprobantes.Services;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreatePuntoFacturacionCommandHandler ──────────────────────────────────────

public class CreatePuntoFacturacionCommandHandlerTests
{
    private readonly IPuntoFacturacionRepository _repo = Substitute.For<IPuntoFacturacionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreatePuntoFacturacionCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_NumeroYaExiste_RetornaFailure()
    {
        _repo.ExisteNumeroAsync(Arg.Any<long>(), Arg.Any<short>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(
            new CreatePuntoFacturacionCommand(1, 1, 1, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NumeroNuevo_CreaYRetornaId()
    {
        _repo.ExisteNumeroAsync(Arg.Any<long>(), Arg.Any<short>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(
            new CreatePuntoFacturacionCommand(1, 1, 1, "Principal"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<PuntoFacturacion>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── AbrirPeriodoIvaCommandHandler ─────────────────────────────────────────────

public class AbrirPeriodoIvaCommandHandlerTests
{
    private readonly IPeriodoIvaRepository _repo = Substitute.For<IPeriodoIvaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private AbrirPeriodoIvaCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_PeriodoNoExiste_CreaNuevoPeriodo()
    {
        _repo.GetPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns((PeriodoIva?)null);

        var result = await Sut().Handle(
            new AbrirPeriodoIvaCommand(1, 1, new DateOnly(2024, 1, 1)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<PeriodoIva>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PeriodoCerrado_Reabre()
    {
        var periodo = PeriodoIva.Crear(1, 1, new DateOnly(2024, 1, 1));
        periodo.Cerrar();
        _repo.GetPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(periodo);

        var result = await Sut().Handle(
            new AbrirPeriodoIvaCommand(1, 1, new DateOnly(2024, 1, 1)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        periodo.Cerrado.Should().BeFalse();
    }
}

// ── CerrarPeriodoIvaCommandHandler ────────────────────────────────────────────

public class CerrarPeriodoIvaCommandHandlerTests
{
    private readonly IPeriodoIvaRepository _repo = Substitute.For<IPeriodoIvaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private CerrarPeriodoIvaCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_PeriodoNoExiste_RetornaFailure()
    {
        _repo.GetPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns((PeriodoIva?)null);

        var result = await Sut().Handle(
            new CerrarPeriodoIvaCommand(1, new DateOnly(2024, 1, 1)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PeriodoAbierto_CierraYPersiste()
    {
        var periodo = PeriodoIva.Crear(1, 1, new DateOnly(2024, 1, 1));
        _repo.GetPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(periodo);

        var result = await Sut().Handle(
            new CerrarPeriodoIvaCommand(1, new DateOnly(2024, 1, 1)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        periodo.Cerrado.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetProximoNumeroQueryHandler ──────────────────────────────────────────────

public class GetProximoNumeroQueryHandlerTests
{
    private readonly NumeracionComprobanteService _svc =
        Substitute.For<NumeracionComprobanteService>(Substitute.For<IPuntoFacturacionRepository>());
    private GetProximoNumeroQueryHandler Sut() => new(_svc);

    [Fact]
    public async Task Handle_RetornaProximoNumeroDto()
    {
        _svc.ObtenerProximoNumeroAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Prefijo: (short)1, Numero: 5L));

        var result = await Sut().Handle(new GetProximoNumeroQuery(1, 1), CancellationToken.None);

        result.Should().NotBeNull();
        result.ProximoNumero.Should().Be(5L);
        result.NumeroFormateado.Should().NotBeNullOrWhiteSpace();
    }
}

// ── GetPuntosFacturacionQueryHandler ─────────────────────────────────────────

public class GetPuntosFacturacionQueryHandlerTests
{
    private readonly IPuntoFacturacionRepository _repo = Substitute.For<IPuntoFacturacionRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetPuntosFacturacionQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaPuntosActivos()
    {
        var puntos = (IReadOnlyList<PuntoFacturacion>)Array.Empty<PuntoFacturacion>();
        var dtos = (IReadOnlyList<PuntoFacturacionListDto>)Array.Empty<PuntoFacturacionListDto>();
        _repo.GetActivosBySucursalAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(puntos);
        _mapper.Map<IReadOnlyList<PuntoFacturacionListDto>>(puntos).Returns(dtos);

        var result = await Sut().Handle(new GetPuntosFacturacionQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.Received(1).GetActivosBySucursalAsync(1L, Arg.Any<CancellationToken>());
    }
}

// ── GetLibroIvaQueryHandler ───────────────────────────────────────────────────

public class GetLibroIvaQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetLibroIvaQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_SinComprobantes_RetornaLibroVacio()
    {
        var mockTiposComprobante36 = MockDbSetHelper.CreateMockDbSet<TipoComprobante>();
        _db.TiposComprobante.Returns(mockTiposComprobante36);
        var mockComprobantes37 = MockDbSetHelper.CreateMockDbSet<Comprobante>();
        _db.Comprobantes.Returns(mockComprobantes37);
        var mockTerceros38 = MockDbSetHelper.CreateMockDbSet<Tercero>();
        _db.Terceros.Returns(mockTerceros38);
        var mockCondicionesIva39 = MockDbSetHelper.CreateMockDbSet<CondicionIva>();
        _db.CondicionesIva.Returns(mockCondicionesIva39);

        var desde = new DateOnly(2024, 1, 1);
        var hasta = new DateOnly(2024, 12, 31);

        var result = await Sut().Handle(
            new GetLibroIvaQuery(1L, desde, hasta, TipoLibroIva.Ventas),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Lineas.Should().BeEmpty();
        result.CantidadComprobantes.Should().Be(0);
    }
}

public class ValidarTimbradoParaguayQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private ValidarTimbradoParaguayQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_SucursalNoParaguay_RetornaValidoSinTimbrado()
    {
        var sucursales = MockDbSetHelper.CreateMockDbSet<Sucursal>(new[] { BuildSucursal(1, 10) });
        var paises = MockDbSetHelper.CreateMockDbSet<Pais>(new[] { BuildPais(10, "AR", "Argentina") });
        var timbrados = MockDbSetHelper.CreateMockDbSet<Timbrado>();

        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Timbrados.Returns(timbrados);

        var result = await Sut().Handle(
            new ValidarTimbradoParaguayQuery(1, 1, 2, new DateOnly(2026, 3, 20), 10),
            CancellationToken.None);

        result.EsSucursalParaguay.Should().BeFalse();
        result.RequiereTimbrado.Should().BeFalse();
        result.EsValido.Should().BeTrue();
        result.TimbradoId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SucursalParaguayConTimbradoVigente_RetornaDetalle()
    {
        var sucursales = MockDbSetHelper.CreateMockDbSet<Sucursal>(new[] { BuildSucursal(1, 20) });
        var paises = MockDbSetHelper.CreateMockDbSet<Pais>(new[] { BuildPais(20, "PY", "Paraguay") });
        var timbrado = Timbrado.Crear(1, 3, 4, "12345678", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 1, 100);
        SetProperty(timbrado, nameof(Timbrado.Id), 99L);
        var timbrados = MockDbSetHelper.CreateMockDbSet(new[] { timbrado });

        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Timbrados.Returns(timbrados);

        var result = await Sut().Handle(
            new ValidarTimbradoParaguayQuery(1, 3, 4, new DateOnly(2026, 3, 20), 10),
            CancellationToken.None);

        result.EsSucursalParaguay.Should().BeTrue();
        result.RequiereTimbrado.Should().BeTrue();
        result.EsValido.Should().BeTrue();
        result.TimbradoId.Should().Be(99L);
        result.NroTimbrado.Should().Be("12345678");
        result.NroComprobanteDesde.Should().Be(1);
        result.NroComprobanteHasta.Should().Be(100);
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }

    private static Sucursal BuildSucursal(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal", "20123456789", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = Pais.Crear(codigo, descripcion);
        SetProperty(pais, nameof(Pais.Id), id);
        return pais;
    }
}

// ── SolicitarCaeaAfipCommandHandler ─────────────────────────────────────────

public class SolicitarCaeaAfipCommandHandlerTests
{
    private readonly IAfipWsfeCaeaService _afip = Substitute.For<IAfipWsfeCaeaService>();
    private readonly IRepository<Caea> _repo = Substitute.For<IRepository<Caea>>();
    private readonly IRepository<AuditoriaCaea> _auditoriaRepo = Substitute.For<IRepository<AuditoriaCaea>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private SolicitarCaeaAfipCommandHandler Sut() => new(_afip, _repo, _auditoriaRepo, _uow, _user);

    [Fact]
    public async Task Handle_RespuestaValida_DeAfip_CreaCaeaYPersiste()
    {
        _user.UserId.Returns((long?)1L);
        _afip.SolicitarCaeaAsync(Arg.Any<SolicitarCaeaAfipRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SolicitarCaeaAfipResponse(
                "12345678901234",
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 15)));

        var result = await Sut().Handle(
            new SolicitarCaeaAfipCommand(5, 202603, 1, "FA", 100),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(
            Arg.Is<Caea>(x =>
                x.NroCaea == "12345678901234" &&
                x.FechaProcesoAfip == new DateOnly(2026, 3, 1) &&
                x.FechaTopeInformarAfip == new DateOnly(2026, 3, 15)),
            Arg.Any<CancellationToken>());
        await _auditoriaRepo.Received(1).AddAsync(
            Arg.Is<AuditoriaCaea>(x =>
                x.Accion == ZuluIA_Back.Domain.Enums.AccionAuditoria.AfipSolicitud &&
                x.DetalleCambio != null &&
                x.DetalleCambio.Contains("nroCaea=12345678901234")),
            Arg.Any<CancellationToken>());
        await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AfipRechazaSolicitud_RetornaFailureYSinPersistir()
    {
        _afip.SolicitarCaeaAsync(Arg.Any<SolicitarCaeaAfipRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("AFIP rechazo la solicitud."));

        var result = await Sut().Handle(
            new SolicitarCaeaAfipCommand(5, 202603, 2, "FB", 50),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("AFIP");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Caea>(), Arg.Any<CancellationToken>());
        await _auditoriaRepo.DidNotReceive().AddAsync(Arg.Any<AuditoriaCaea>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class GetAuditoriaCaeaQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetAuditoriaCaeaQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_CuandoHayRegistros_RetornaAuditoriaOrdenada()
    {
        var registros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            AuditoriaCaea.Registrar(10, 2, ZuluIA_Back.Domain.Enums.AccionAuditoria.Anulado, "segundo", null),
            AuditoriaCaea.Registrar(10, 1, ZuluIA_Back.Domain.Enums.AccionAuditoria.Creado, "primero", null)
        });
        _db.AuditoriaCaeas.Returns(registros);

        var result = await Sut().Handle(new GetAuditoriaCaeaQuery(10), CancellationToken.None);

        result.Should().HaveCount(2);
        result.All(x => x.CaeaId == 10).Should().BeTrue();
    }
}

public class GetCaeaDetalleQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetCaeaDetalleQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_CaeaExistente_ExponeMetadatosAfip()
    {
        var caea = Caea.Crear(
            5,
            "12345678901234",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 15),
            new DateOnly(2026, 3, 2),
            new DateOnly(2026, 3, 16),
            "FA",
            100,
            1L);
        SetProperty(caea, nameof(Caea.Id), 10L);

        var caeas = MockDbSetHelper.CreateMockDbSet(new[] { caea });
        _db.Caeas.Returns(caeas);

        var result = await Sut().Handle(new GetCaeaDetalleQuery(10), CancellationToken.None);

        result.Should().NotBeNull();
        result!.FechaProcesoAfip.Should().Be(new DateOnly(2026, 3, 2));
        result.FechaTopeInformarAfip.Should().Be(new DateOnly(2026, 3, 16));
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class ParaguaySifenComprobanteServiceTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IParaguaySifenService _sifen = Substitute.For<IParaguaySifenService>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly Microsoft.EntityFrameworkCore.DbSet<AuditoriaComprobante> _auditoria = MockDbSetHelper.CreateMockDbSet<AuditoriaComprobante>();
    private readonly Microsoft.EntityFrameworkCore.DbSet<HistorialSifenComprobante> _historial = MockDbSetHelper.CreateMockDbSet<HistorialSifenComprobante>();

    private ParaguaySifenComprobanteService Sut() => new(_db, _sifen, _user);

    public ParaguaySifenComprobanteServiceTests()
    {
        _user.UserId.Returns((long?)7L);
        _db.AuditoriaComprobantes.Returns(_auditoria);
        _db.HistorialSifenComprobantes.Returns(_historial);
    }

    [Fact]
    public async Task Handle_ComprobanteParaguayEmitido_RetornaPreviewListo()
    {
        var comprobante = BuildComprobanteParaguay(10, 1, 4, 5, 3);
        var sucursal = BuildSucursalParaguay(1, 20);
        var pais = BuildPais(20, "PY", "Paraguay");
        var tercero = BuildTercero(3, "1234567", "Cliente Paraguay");
        var tipo = BuildTipoComprobante(5, "FCR", "Factura Contado");
        var punto = BuildPuntoFacturacion(4, 1, 12);
        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { comprobante });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { tercero });
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { punto });
        var items = MockDbSetHelper.CreateMockDbSet(new[]
        {
            ComprobanteItem.Crear(10, 1, "Producto", 2, 0, 100, 0, 1, 10, null, 1)
        });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Terceros.Returns(terceros);
        _db.TiposComprobante.Returns(tipos);
        _db.PuntosFacturacion.Returns(puntos);
        _db.ComprobantesItems.Returns(items);

        _sifen.PrepararEnvioAsync(
                Arg.Is<PrepararEnvioSifenRequest>(x =>
                    x.ComprobanteId == 10
                    && x.RucEmisor == "80012345-6"
                    && x.DocumentoReceptor == "1234567"
                    && x.NroTimbrado == "12345678"
                    && x.CantidadItems == 1
                    && x.PuntoExpedicion == 12),
                Arg.Any<CancellationToken>())
            .Returns(new PreparacionSifenParaguayDto
            {
                ComprobanteId = 10,
                IntegracionHabilitada = true,
                Ambiente = "test",
                Endpoint = "https://sifen.test/api",
                ModoTransporte = "http",
                Errores = [],
                Documento = new PreparacionSifenDocumentoDto
                {
                    RucEmisor = "80012345-6",
                    DocumentoReceptor = "1234567",
                    NroTimbrado = "12345678",
                    CantidadItems = 1
                }
            });

        var result = await Sut().PrepararAsync(10, CancellationToken.None);

        result.Should().NotBeNull();
        result!.EsSucursalParaguay.Should().BeTrue();
        result.IntegracionHabilitada.Should().BeTrue();
        result.ListoParaEnviar.Should().BeTrue();
        result.Errores.Should().BeEmpty();
        result.Documento.NroTimbrado.Should().Be("12345678");
    }

    [Fact]
    public async Task Handle_ComprobanteNoParaguay_RetornaNoListoConErrorDePais()
    {
        var comprobante = BuildComprobanteParaguay(10, 1, 4, 5, 3);
        var sucursal = BuildSucursalArgentina(1, 10);
        var pais = BuildPais(10, "AR", "Argentina");
        var tercero = BuildTercero(3, "1234567", "Cliente Argentina");
        var tipo = BuildTipoComprobante(5, "FAC", "Factura A");
        var punto = BuildPuntoFacturacion(4, 1, 12);
        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { comprobante });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { tercero });
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { punto });
        var items = MockDbSetHelper.CreateMockDbSet(new[]
        {
            ComprobanteItem.Crear(10, 1, "Producto", 1, 0, 100, 0, 1, 21, null, 1)
        });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Terceros.Returns(terceros);
        _db.TiposComprobante.Returns(tipos);
        _db.PuntosFacturacion.Returns(puntos);
        _db.ComprobantesItems.Returns(items);
        _sifen.PrepararEnvioAsync(Arg.Any<PrepararEnvioSifenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PreparacionSifenParaguayDto
            {
                ComprobanteId = 10,
                IntegracionHabilitada = true,
                Ambiente = "test",
                Endpoint = "https://sifen.test/api",
                ModoTransporte = "http",
                Errores = []
            });

        var result = await Sut().PrepararAsync(10, CancellationToken.None);

        result.Should().NotBeNull();
        result!.EsSucursalParaguay.Should().BeFalse();
        result.ListoParaEnviar.Should().BeFalse();
        result.Errores.Should().Contain(x => x.Contains("no pertenece a Paraguay", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EnviarAsync_RespuestaAceptada_MaterializaEstadoSifenYAuditoria()
    {
        var comprobante = BuildComprobanteParaguay(10, 1, 4, 5, 3);
        var sucursal = BuildSucursalParaguay(1, 20);
        var pais = BuildPais(20, "PY", "Paraguay");
        var tercero = BuildTercero(3, "1234567", "Cliente Paraguay");
        var tipo = BuildTipoComprobante(5, "FCR", "Factura Contado");
        var punto = BuildPuntoFacturacion(4, 1, 12);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { comprobante });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { tercero });
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { punto });
        var items = MockDbSetHelper.CreateMockDbSet(new[]
        {
            ComprobanteItem.Crear(10, 1, "Producto", 1, 0, 100, 0, 1, 10, null, 1)
        });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Terceros.Returns(terceros);
        _db.TiposComprobante.Returns(tipos);
        _db.PuntosFacturacion.Returns(puntos);
        _db.ComprobantesItems.Returns(items);

        _sifen.PrepararEnvioAsync(Arg.Any<PrepararEnvioSifenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PreparacionSifenParaguayDto
            {
                ComprobanteId = 10,
                IntegracionHabilitada = true,
                ListoParaEnviar = true,
                Errores = []
            });
        _sifen.EnviarAsync(Arg.Any<PrepararEnvioSifenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = 10,
                Aceptado = true,
                Estado = "accepted",
                CodigoRespuesta = null,
                MensajeRespuesta = null,
                TrackingId = "SIFEN-10",
                Cdc = "CDC-10",
                NumeroLote = "LOTE-10",
                FechaRespuesta = new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero)
            });

        var result = await Sut().EnviarAsync(comprobante, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comprobante.EstadoSifen.Should().Be(ZuluIA_Back.Domain.Enums.EstadoSifenParaguay.Aceptado);
        comprobante.SifenCodigoRespuesta.Should().BeNull();
        comprobante.SifenMensajeRespuesta.Should().BeNull();
        comprobante.SifenTrackingId.Should().Be("SIFEN-10");
        comprobante.SifenCdc.Should().Be("CDC-10");
        comprobante.SifenNumeroLote.Should().Be("LOTE-10");
        comprobante.SifenFechaRespuesta.Should().Be(new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero));
        _historial.Received(1).Add(Arg.Is<HistorialSifenComprobante>(x =>
            x.ComprobanteId == 10
            && x.EstadoSifen == ZuluIA_Back.Domain.Enums.EstadoSifenParaguay.Aceptado
            && x.Aceptado
            && x.CodigoRespuesta == null
            && x.MensajeRespuesta == null
            && x.TrackingId == "SIFEN-10"
            && x.Cdc == "CDC-10"
            && x.NumeroLote == "LOTE-10"
            && x.EstadoRespuesta == "accepted"));
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x => x.ComprobanteId == 10 && x.Accion == ZuluIA_Back.Domain.Enums.AccionAuditoria.SifenSolicitud));
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x => x.ComprobanteId == 10 && x.Accion == ZuluIA_Back.Domain.Enums.AccionAuditoria.SifenAprobado));
    }

    [Fact]
    public async Task EnviarAsync_ExcepcionMarcaEstadoErrorYPersisteAuditoria()
    {
        var comprobante = BuildComprobanteParaguay(10, 1, 4, 5, 3);
        var sucursal = BuildSucursalParaguay(1, 20);
        var pais = BuildPais(20, "PY", "Paraguay");
        var tercero = BuildTercero(3, "1234567", "Cliente Paraguay");
        var tipo = BuildTipoComprobante(5, "FCR", "Factura Contado");
        var punto = BuildPuntoFacturacion(4, 1, 12);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { comprobante });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { tercero });
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { punto });
        var items = MockDbSetHelper.CreateMockDbSet(new[]
        {
            ComprobanteItem.Crear(10, 1, "Producto", 1, 0, 100, 0, 1, 10, null, 1)
        });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Terceros.Returns(terceros);
        _db.TiposComprobante.Returns(tipos);
        _db.PuntosFacturacion.Returns(puntos);
        _db.ComprobantesItems.Returns(items);

        _sifen.PrepararEnvioAsync(Arg.Any<PrepararEnvioSifenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PreparacionSifenParaguayDto
            {
                ComprobanteId = 10,
                IntegracionHabilitada = true,
                ListoParaEnviar = true,
                Errores = []
            });
        _sifen.EnviarAsync(Arg.Any<PrepararEnvioSifenRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("SIFEN timeout"));

        var result = await Sut().EnviarAsync(comprobante, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        comprobante.EstadoSifen.Should().Be(ZuluIA_Back.Domain.Enums.EstadoSifenParaguay.Error);
        comprobante.SifenMensajeRespuesta.Should().Be("SIFEN timeout");
        comprobante.SifenFechaRespuesta.Should().NotBeNull();
        _historial.Received(1).Add(Arg.Is<HistorialSifenComprobante>(x =>
            x.ComprobanteId == 10
            && x.EstadoSifen == ZuluIA_Back.Domain.Enums.EstadoSifenParaguay.Error
            && !x.Aceptado
            && x.MensajeRespuesta == "SIFEN timeout"
            && x.EstadoRespuesta == "error"
            && x.Detalle == "SIFEN timeout"));
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x => x.ComprobanteId == 10 && x.Accion == ZuluIA_Back.Domain.Enums.AccionAuditoria.SifenSolicitud));
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x => x.ComprobanteId == 10 && x.Accion == ZuluIA_Back.Domain.Enums.AccionAuditoria.SifenError));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConciliarEstadoAsync_RespuestaAceptada_ActualizaEstadoYRegistraConsulta()
    {
        var comprobante = BuildComprobanteParaguay(10, 1, 4, 5, 3);
        comprobante.RegistrarResultadoSifen(ZuluIA_Back.Domain.Enums.EstadoSifenParaguay.Rechazado, "150", "Rechazo previo", "SIFEN-10", "CDC-10", "LOTE-10", new DateTimeOffset(2026, 3, 20, 17, 0, 0, TimeSpan.Zero), null);

        _sifen.ConsultarEstadoAsync(
                Arg.Is<ConsultarEstadoSifenRequest>(x =>
                    x.ComprobanteId == 10 && x.TrackingId == "SIFEN-10" && x.Cdc == "CDC-10" && x.NumeroLote == "LOTE-10"),
                Arg.Any<CancellationToken>())
            .Returns(new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = 10,
                Aceptado = true,
                Estado = "approved",
                CodigoRespuesta = null,
                MensajeRespuesta = null,
                TrackingId = "SIFEN-10",
                Cdc = "CDC-10",
                NumeroLote = "LOTE-10",
                FechaRespuesta = new DateTimeOffset(2026, 3, 20, 18, 30, 0, TimeSpan.Zero)
            });

        var result = await Sut().ConciliarEstadoAsync(comprobante, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comprobante.EstadoSifen.Should().Be(ZuluIA_Back.Domain.Enums.EstadoSifenParaguay.Aceptado);
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x => x.ComprobanteId == 10 && x.Accion == ZuluIA_Back.Domain.Enums.AccionAuditoria.SifenConsulta));
        _historial.Received(1).Add(Arg.Is<HistorialSifenComprobante>(x =>
            x.ComprobanteId == 10
            && x.EstadoSifen == ZuluIA_Back.Domain.Enums.EstadoSifenParaguay.Aceptado
            && x.EstadoRespuesta == "approved"));
    }

    [Fact]
    public async Task ConciliarEstadoAsync_SinIdentificadores_RetornaFailure()
    {
        var comprobante = BuildComprobanteParaguay(10, 1, 4, 5, 3);

        var result = await Sut().ConciliarEstadoAsync(comprobante, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("TrackingId");
        await _sifen.DidNotReceive().ConsultarEstadoAsync(Arg.Any<ConsultarEstadoSifenRequest>(), Arg.Any<CancellationToken>());
    }

    private static Comprobante BuildComprobanteParaguay(long id, long sucursalId, long puntoFacturacionId, long tipoComprobanteId, long terceroId)
    {
        var comprobante = Comprobante.Crear(
            sucursalId,
            puntoFacturacionId,
            tipoComprobanteId,
            1,
            123,
            new DateOnly(2026, 3, 20),
            null,
            terceroId,
            1,
            1,
            "Observacion SIFEN",
            null);

        SetProperty(comprobante, nameof(Comprobante.Id), id);
        comprobante.AgregarItem(ComprobanteItem.Crear(id, 1, "Producto", 1, 0, 100, 0, 1, 10, null, 1));
        comprobante.SetPercepciones(5, null);
        comprobante.AsignarTimbrado(99, "12345678", null);
        comprobante.Emitir(null);
        return comprobante;
    }

    private static Sucursal BuildSucursalParaguay(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal PY", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        sucursal.Actualizar(
            "Sucursal PY",
            "Casa Central",
            "80012345-6",
            null,
            1,
            1,
            paisId,
            ZuluIA_Back.Domain.ValueObjects.Domicilio.Crear("Av. Paraguay", "123", null, null, "1000", null, null),
            null,
            "py@zulia.test",
            null,
            null,
            null,
            null,
            443,
            true,
            null);
        return sucursal;
    }

    private static Sucursal BuildSucursalArgentina(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal AR", "20123456789", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static Tercero BuildTercero(long id, string documento, string razonSocial)
    {
        var tercero = Tercero.Crear("CLI-1", razonSocial, 1, documento, 1, true, false, false, null, null);
        SetProperty(tercero, nameof(Tercero.Id), id);
        tercero.SetDomicilio(ZuluIA_Back.Domain.ValueObjects.Domicilio.Crear("Calle Cliente", "456", null, null, "1100", null, null));
        return tercero;
    }

    private static PuntoFacturacion BuildPuntoFacturacion(long id, long sucursalId, short numero)
    {
        var punto = PuntoFacturacion.Crear(sucursalId, 1, numero, "Mostrador", null);
        SetProperty(punto, nameof(PuntoFacturacion.Id), id);
        return punto;
    }

    private static TipoComprobante BuildTipoComprobante(long id, string codigo, string descripcion)
    {
        var tipo = (TipoComprobante)Activator.CreateInstance(typeof(TipoComprobante), true)!;
        SetProperty(tipo, nameof(TipoComprobante.Id), id);
        SetProperty(tipo, nameof(TipoComprobante.Codigo), codigo);
        SetProperty(tipo, nameof(TipoComprobante.Descripcion), descripcion);
        SetProperty(tipo, nameof(TipoComprobante.EsVenta), true);
        SetProperty(tipo, nameof(TipoComprobante.Activo), true);
        return tipo;
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = Pais.Crear(codigo, descripcion);
        SetProperty(pais, nameof(Pais.Id), id);
        return pais;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}
