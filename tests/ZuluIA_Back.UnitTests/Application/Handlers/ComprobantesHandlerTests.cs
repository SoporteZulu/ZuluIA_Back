using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Comprobantes.Services;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.Domain.ValueObjects;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateComprobanteCommandHandler ──────────────────────────────────────────

public class CreateComprobanteCommandHandlerTests
{
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private CreateComprobanteCommandHandler Sut() => new(_repo, _uow, _user, _db, new TerceroOperacionValidationService(_db));

    private static CreateComprobanteCommand CmdValido() => new(
        1, null, 1, 1, 1,
        DateOnly.FromDateTime(DateTime.Today), null,
        1, 1, 1m, null,
        [new CreateComprobanteItemDto(1, "Prod", 1m, 1000, 0, 1, 21, null, 1)]);

    [Fact]
    public async Task Handle_NumeroExistente_RetornaFailure()
    {
        var existente = Comprobante.Crear(1, null, 1, 1, 1,
            DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        _repo.GetByNumeroAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<short>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(existente);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NumeroNuevo_CreaYRetornaId()
    {
        _user.UserId.Returns((long?)1L);
        _repo.GetByNumeroAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<short>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Comprobante?)null);
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { BuildTipoComprobante(1, true, 1) });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildCliente(1, null) });
        var impuestos = Substitute.For<DbSet<ComprobanteImpuesto>>();
        _db.TiposComprobante.Returns(tipos);
        _db.Terceros.Returns(terceros);
        _db.ComprobantesImpuestos.Returns(impuestos);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
        impuestos.Received(1).Add(Arg.Is<ComprobanteImpuesto>(x => x.BaseImponible == 1000m && x.ImporteIva == 210m));
        await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DescuentoSuperaTopeCliente_RetornaFailure()
    {
        _repo.GetByNumeroAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<short>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Comprobante?)null);

        var tipo = BuildTipoComprobante(1, true, 1);
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildCliente(1, 5m) });

        _db.TiposComprobante.Returns(tipos);
        _db.Terceros.Returns(terceros);

        var cmd = CmdValido() with
        {
            Items = [new CreateComprobanteItemDto(1, "Prod", 1m, 1000, 10, 1, 21, null, 1)]
        };

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("porcentaje máximo permitido");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
    }

    private static TipoComprobante BuildTipoComprobante(long id, bool esVenta, short tipoAfip)
    {
        var tipo = (TipoComprobante)Activator.CreateInstance(typeof(TipoComprobante), nonPublic: true)!;
        SetProperty(tipo, nameof(TipoComprobante.Id), id);
        SetProperty(tipo, nameof(TipoComprobante.EsVenta), esVenta);
        SetProperty(tipo, nameof(TipoComprobante.TipoAfip), (short?)tipoAfip);
        return tipo;
    }

    private static Tercero BuildCliente(long id, decimal? porcentajeMaximoDescuento)
    {
        var tercero = Tercero.Crear("CLI0001", "Cliente Test", 1, "20123456789", 1, true, false, false, null, null);
        SetProperty(tercero, nameof(Tercero.Id), id);
        tercero.Actualizar(
            "Cliente Test",
            "Cliente Test",
            1,
            null,
            null,
            null,
            null,
            Domicilio.Vacio(),
            null,
            null,
            null,
            porcentajeMaximoDescuento,
            null,
            null,
            true,
            null,
            false,
            0m,
            null,
            false,
            0m,
            null,
            null);
        return tercero;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

// ── EmitirComprobanteCommandHandler ──────────────────────────────────────────

public class EmitirComprobanteCommandHandlerTests
{
    private readonly IComprobanteRepository _comprobanteRepo = Substitute.For<IComprobanteRepository>();
    private readonly IPeriodoIvaRepository _periodoRepo = Substitute.For<IPeriodoIvaRepository>();
    private readonly StockService _stockService = Substitute.For<StockService>(
        Substitute.For<IStockRepository>(),
        Substitute.For<IMovimientoStockRepository>());
    private readonly IAfipCaeComprobanteService _afipCae = Substitute.For<IAfipCaeComprobanteService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private EmitirComprobanteCommandHandler Sut() =>
        new(_comprobanteRepo, _periodoRepo, _uow, _user, _db, BuildServiceProvider());

    private IServiceProvider BuildServiceProvider()
    {
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(StockService)).Returns(_stockService);
        provider.GetService(typeof(IAfipCaeComprobanteService)).Returns(_afipCae);
        provider.GetService(typeof(TerceroOperacionValidationService)).Returns(new TerceroOperacionValidationService(_db));
        provider.GetService(typeof(ItemCommercialStockService)).Returns(new ItemCommercialStockService(_db));
        return provider;
    }

    private static EmitirComprobanteCommand CmdValido() => new(
        null, 1, null, 1,
        DateOnly.FromDateTime(DateTime.Today), null,
        1, 1, 1m, 0m, null,
        [new ComprobanteItemInput(1, "Prod", 1m, 0, 1000, 0m, 1, null, 1)],
        false);

    [Fact]
    public async Task Handle_PeriodoCerrado_RetornaFailure()
    {
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SinItems_RetornaFailure()
    {
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var cmd = CmdValido() with { Items = [] };
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TipoComprobanteNoEncontrado_RetornaFailure()
    {
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var mockTiposComprobante1 = MockDbSetHelper.CreateMockDbSet<TipoComprobante>();
        _db.TiposComprobante.Returns(mockTiposComprobante1);
        var mockAlicuotasIva2 = MockDbSetHelper.CreateMockDbSet<AlicuotaIva>();
        _db.AlicuotasIva.Returns(mockAlicuotasIva2);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConfigFiscalOnline_SolicitaCaeAutomaticamente()
    {
        _user.UserId.Returns((long?)1L);
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _afipCae.SolicitarYAsignarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _comprobanteRepo.GetProximoNumeroAsync(1, 1, Arg.Any<CancellationToken>())
            .Returns(1L);

        var tipo = BuildTipoComprobante(1, true, 1);
        SetProperty(tipo, nameof(TipoComprobante.AfectaStock), false);
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildCliente(1, null) });
        var alicuotas = MockDbSetHelper.CreateMockDbSet(new[] { BuildAlicuota(1, 5, 21) });
        var items = MockDbSetHelper.CreateMockDbSet(new[] { BuildItem(1, "Prod") });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { BuildPuntoFacturacion(1, 1, 1) });
        var configuraciones = MockDbSetHelper.CreateMockDbSet(new[] { BuildConfiguracionFiscal(1, 1, true) });
        var impuestos = Substitute.For<DbSet<ComprobanteImpuesto>>();
        var tributos = Substitute.For<DbSet<ComprobanteTributo>>();
        var impuestosConfigurados = MockDbSetHelper.CreateMockDbSet(new[] { BuildImpuesto(10, "99", "Percepciones", 3.5m, "percepcion") });
        var asignaciones = MockDbSetHelper.CreateMockDbSet(new[] { BuildImpuestoPorTipoComprobante(10, 1, 1) });
        var sucursales = MockDbSetHelper.CreateMockDbSet<Sucursal>();
        var paises = MockDbSetHelper.CreateMockDbSet<Pais>();
        var timbrados = MockDbSetHelper.CreateMockDbSet<Timbrado>();

        _db.TiposComprobante.Returns(tipos);
        _db.Terceros.Returns(terceros);
        _db.AlicuotasIva.Returns(alicuotas);
        _db.Items.Returns(items);
        _db.PuntosFacturacion.Returns(puntos);
        _db.ConfiguracionesFiscales.Returns(configuraciones);
        _db.ComprobantesImpuestos.Returns(impuestos);
        _db.ComprobantesTributos.Returns(tributos);
        _db.Impuestos.Returns(impuestosConfigurados);
        _db.ImpuestosPorTipoComprobante.Returns(asignaciones);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Timbrados.Returns(timbrados);

        var result = await Sut().Handle(CmdValido() with { PuntoFacturacionId = 1, Percepciones = 35m }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _afipCae.Received(1).SolicitarYAsignarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
        impuestos.Received(1).Add(Arg.Is<ComprobanteImpuesto>(x => x.BaseImponible == 1000m && x.ImporteIva == 210m));
        tributos.Received(1).Add(Arg.Is<ComprobanteTributo>(x => x.Codigo == "99" && x.Importe == 35m && x.Alicuota == 3.5m));
        await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConfigFiscalOnline_AfipRechaza_PersisteAntesDelIntentoYRetornaFailure()
    {
        _user.UserId.Returns((long?)1L);
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _afipCae.SolicitarYAsignarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("AFIP rechazo"));
        _comprobanteRepo.GetProximoNumeroAsync(1, 1, Arg.Any<CancellationToken>())
            .Returns(1L);

        var tipo = BuildTipoComprobante(1, true, 1);
        SetProperty(tipo, nameof(TipoComprobante.AfectaStock), false);
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildCliente(1, null) });
        var alicuotas = MockDbSetHelper.CreateMockDbSet(new[] { BuildAlicuota(1, 5, 21) });
        var items = MockDbSetHelper.CreateMockDbSet(new[] { BuildItem(1, "Prod") });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { BuildPuntoFacturacion(1, 1, 1) });
        var configuraciones = MockDbSetHelper.CreateMockDbSet(new[] { BuildConfiguracionFiscal(1, 1, true) });
        var impuestos = Substitute.For<DbSet<ComprobanteImpuesto>>();
        var tributos = Substitute.For<DbSet<ComprobanteTributo>>();
        var impuestosConfigurados = MockDbSetHelper.CreateMockDbSet(new[] { BuildImpuesto(10, "99", "Percepciones", 3.5m, "percepcion") });
        var asignaciones = MockDbSetHelper.CreateMockDbSet(new[] { BuildImpuestoPorTipoComprobante(10, 1, 1) });
        var sucursales = MockDbSetHelper.CreateMockDbSet<Sucursal>();
        var paises = MockDbSetHelper.CreateMockDbSet<Pais>();
        var timbrados = MockDbSetHelper.CreateMockDbSet<Timbrado>();

        _db.TiposComprobante.Returns(tipos);
        _db.Terceros.Returns(terceros);
        _db.AlicuotasIva.Returns(alicuotas);
        _db.Items.Returns(items);
        _db.PuntosFacturacion.Returns(puntos);
        _db.ConfiguracionesFiscales.Returns(configuraciones);
        _db.ComprobantesImpuestos.Returns(impuestos);
        _db.ComprobantesTributos.Returns(tributos);
        _db.Impuestos.Returns(impuestosConfigurados);
        _db.ImpuestosPorTipoComprobante.Returns(asignaciones);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Timbrados.Returns(timbrados);

        var result = await Sut().Handle(CmdValido() with { PuntoFacturacionId = 1, Percepciones = 35m }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("AFIP");
        await _afipCae.Received(1).SolicitarYAsignarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SucursalParaguaySinTimbradoVigente_RetornaFailureYNoInvocaAfip()
    {
        _user.UserId.Returns((long?)1L);
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _comprobanteRepo.GetProximoNumeroAsync(1, 1, Arg.Any<CancellationToken>())
            .Returns(1L);

        var tipo = BuildTipoComprobante(1, true, 1);
        SetProperty(tipo, nameof(TipoComprobante.AfectaStock), false);
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildCliente(1, null) });
        var alicuotas = MockDbSetHelper.CreateMockDbSet(new[] { BuildAlicuota(1, 5, 21) });
        var items = MockDbSetHelper.CreateMockDbSet(new[] { BuildItem(1, "Prod") });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { BuildPuntoFacturacion(1, 1, 1) });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { BuildSucursal(1, 2) });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { BuildPais(2, "PY", "Paraguay") });
        var timbrados = MockDbSetHelper.CreateMockDbSet<Timbrado>();

        _db.TiposComprobante.Returns(tipos);
        _db.Terceros.Returns(terceros);
        _db.AlicuotasIva.Returns(alicuotas);
        _db.Items.Returns(items);
        _db.PuntosFacturacion.Returns(puntos);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Timbrados.Returns(timbrados);

        var result = await Sut().Handle(CmdValido() with { PuntoFacturacionId = 1 }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("timbrado vigente");
        await _afipCae.DidNotReceive().SolicitarYAsignarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SucursalParaguayConTimbradoVigente_PermiteEmitir()
    {
        _user.UserId.Returns((long?)1L);
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _comprobanteRepo.GetProximoNumeroAsync(1, 1, Arg.Any<CancellationToken>())
            .Returns(1L);

        var tipo = BuildTipoComprobante(1, true, 1);
        SetProperty(tipo, nameof(TipoComprobante.AfectaStock), false);
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildCliente(1, null) });
        var alicuotas = MockDbSetHelper.CreateMockDbSet(new[] { BuildAlicuota(1, 5, 21) });
        var items = MockDbSetHelper.CreateMockDbSet(new[] { BuildItem(1, "Prod") });
        var puntos = MockDbSetHelper.CreateMockDbSet(new[] { BuildPuntoFacturacion(1, 1, 1) });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { BuildSucursal(1, 2) });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { BuildPais(2, "PY", "Paraguay") });
        var timbrado = Timbrado.Crear(1, 1, 1, "12345678", DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateOnly.FromDateTime(DateTime.Today.AddDays(10)), 1, 100);
        SetProperty(timbrado, nameof(Timbrado.Id), 1L);
        var timbrados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            timbrado
        });
        var configuraciones = MockDbSetHelper.CreateMockDbSet<ConfiguracionFiscal>();

        _db.TiposComprobante.Returns(tipos);
        _db.Terceros.Returns(terceros);
        _db.AlicuotasIva.Returns(alicuotas);
        _db.Items.Returns(items);
        _db.PuntosFacturacion.Returns(puntos);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);
        _db.Timbrados.Returns(timbrados);
        _db.ConfiguracionesFiscales.Returns(configuraciones);

        var result = await Sut().Handle(CmdValido() with { PuntoFacturacionId = 1 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _comprobanteRepo.Received(1).AddAsync(Arg.Is<Comprobante>(x => x.TimbradoId == 1 && x.NroTimbrado == "12345678"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DescuentoSuperaTopeCliente_RetornaFailure()
    {
        _periodoRepo.EstaAbiertoPeriodoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var tipo = BuildTipoComprobante(1, true, 1);
        var tipos = MockDbSetHelper.CreateMockDbSet(new[] { tipo });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildCliente(1, 5m) });

        _db.TiposComprobante.Returns(tipos);
        _db.Terceros.Returns(terceros);

        var cmd = CmdValido() with
        {
            Items = [new ComprobanteItemInput(1, "Prod", 1m, 0, 1000, 10m, 1, null, 1)]
        };

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("porcentaje máximo permitido");
        await _comprobanteRepo.DidNotReceive().AddAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
    }

    private static TipoComprobante BuildTipoComprobante(long id, bool esVenta, short tipoAfip)
    {
        var tipo = (TipoComprobante)Activator.CreateInstance(typeof(TipoComprobante), nonPublic: true)!;
        SetProperty(tipo, nameof(TipoComprobante.Id), id);
        SetProperty(tipo, nameof(TipoComprobante.EsVenta), esVenta);
        SetProperty(tipo, nameof(TipoComprobante.TipoAfip), (short?)tipoAfip);
        return tipo;
    }

    private static PuntoFacturacion BuildPuntoFacturacion(long id, long sucursalId, short numero)
    {
        var punto = PuntoFacturacion.Crear(sucursalId, 1, numero, null, null);
        SetProperty(punto, nameof(PuntoFacturacion.Id), id);
        return punto;
    }

    private static AlicuotaIva BuildAlicuota(long id, short codigo, long porcentaje)
    {
        var alicuota = (AlicuotaIva)Activator.CreateInstance(typeof(AlicuotaIva), nonPublic: true)!;
        SetProperty(alicuota, nameof(AlicuotaIva.Id), id);
        SetProperty(alicuota, nameof(AlicuotaIva.Codigo), codigo);
        SetProperty(alicuota, nameof(AlicuotaIva.Porcentaje), porcentaje);
        return alicuota;
    }

    private static Item BuildItem(long id, string descripcion)
    {
        var item = (Item)Activator.CreateInstance(typeof(Item), nonPublic: true)!;
        SetProperty(item, nameof(Item.Id), id);
        SetProperty(item, nameof(Item.Descripcion), descripcion);
        return item;
    }

    private static ConfiguracionFiscal BuildConfiguracionFiscal(long puntoFacturacionId, long tipoComprobanteId, bool online)
    {
        return ConfiguracionFiscal.Crear(puntoFacturacionId, tipoComprobanteId, online: online);
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

    private static Impuesto BuildImpuesto(long id, string codigo, string descripcion, decimal alicuota, string tipo)
    {
        var impuesto = Impuesto.Crear(codigo, descripcion, alicuota, 0m, tipo);
        SetProperty(impuesto, nameof(Impuesto.Id), id);
        return impuesto;
    }

    private static ImpuestoPorTipoComprobante BuildImpuestoPorTipoComprobante(long impuestoId, long tipoComprobanteId, int orden)
    {
        return ImpuestoPorTipoComprobante.Crear(impuestoId, tipoComprobanteId, orden);
    }

    private static Tercero BuildCliente(long id, decimal? porcentajeMaximoDescuento)
    {
        var tercero = Tercero.Crear("CLI0001", "Cliente Test", 1, "20123456789", 1, true, false, false, null, null);
        SetProperty(tercero, nameof(Tercero.Id), id);
        tercero.Actualizar(
            "Cliente Test",
            "Cliente Test",
            1,
            null,
            null,
            null,
            null,
            Domicilio.Vacio(),
            null,
            null,
            null,
            porcentajeMaximoDescuento,
            null,
            null,
            true,
            null,
            false,
            0m,
            null,
            false,
            0m,
            null,
            null);
        return tercero;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

// ── AnularComprobanteCommandHandler ──────────────────────────────────────────

public class AnularComprobanteCommandHandlerTests
{
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly StockService _stockService = Substitute.For<StockService>(
        Substitute.For<IStockRepository>(),
        Substitute.For<IMovimientoStockRepository>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private AnularComprobanteCommandHandler Sut() => new(_repo, _stockService, _uow, _user, _db);

    [Fact]
    public async Task Handle_ComprobanteNoEncontrado_RetornaFailure()
    {
        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Comprobante?)null);

        var result = await Sut().Handle(new AnularComprobanteCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ComprobanteYaAnulado_RetornaFailure()
    {
        var comp = Comprobante.Crear(1, null, 1, 1, 1,
            DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        comp.AgregarItem(ComprobanteItem.Crear(0, 1, "Prod", 1m, 0, 1000, 0m, 1, 21, null, 1));
        comp.Emitir(null);
        comp.Anular(null);

        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(comp);
        var mockTiposComprobante3 = MockDbSetHelper.CreateMockDbSet<TipoComprobante>();
        _db.TiposComprobante.Returns(mockTiposComprobante3);

        var result = await Sut().Handle(new AnularComprobanteCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ComprobanteEmitido_AnulaYGuarda()
    {
        _user.UserId.Returns((long?)1L);
        var comp = Comprobante.Crear(1, null, 1, 1, 1,
            DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        comp.AgregarItem(ComprobanteItem.Crear(0, 1, "Prod", 1m, 0, 1000, 0m, 1, 21, null, 1));
        comp.Emitir(null);

        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(comp);
        var mockTiposComprobante4 = MockDbSetHelper.CreateMockDbSet<TipoComprobante>();
        _db.TiposComprobante.Returns(mockTiposComprobante4);

        var result = await Sut().Handle(new AnularComprobanteCommand(1, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Comprobante>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class SolicitarCaeAfipComprobanteCommandHandlerTests
{
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IAfipCaeComprobanteService _afipCae = Substitute.For<IAfipCaeComprobanteService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private SolicitarCaeAfipComprobanteCommandHandler Sut() =>
        new(_repo, _afipCae, _uow);

    [Fact]
    public async Task Handle_ComprobanteValido_SolicitaCaeYActualizaComprobante()
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 10, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        comprobante.AgregarItem(ComprobanteItem.Crear(0, 1, "Prod", 1m, 0, 1000, 0m, 1, 21, null, 1));
        comprobante.Emitir(null);

        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(comprobante);
        comprobante.AsignarCae("12345678901234", new DateOnly(2026, 4, 30), "qr", 1L);
        _afipCae.SolicitarYAsignarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await Sut().Handle(new SolicitarCaeAfipComprobanteCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comprobante.Cae.Should().Be("12345678901234");
        comprobante.QrData.Should().NotBeNullOrWhiteSpace();
        _repo.Received(1).Update(comprobante);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConPercepciones_EnviaTributoAAfipYPersiste()
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 10, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(comprobante);
        _afipCae.SolicitarYAsignarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("AFIP rechazo"));

        var result = await Sut().Handle(new SolicitarCaeAfipComprobanteCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("AFIP");
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SolicitarSifenParaguay_ConResultadoExitoso_ActualizaComprobanteYPersiste()
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 10, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        comprobante.AgregarItem(ComprobanteItem.Crear(0, 1, "Prod", 1m, 0, 1000, 0m, 1, 21, null, 1));
        comprobante.AsignarTimbrado(1, "12345678", null);
        comprobante.Emitir(null);

        var sifen = Substitute.For<IParaguaySifenComprobanteService>();
        var handler = new SolicitarSifenParaguayComprobanteCommandHandler(_repo, sifen, _uow);

        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(comprobante);
        sifen.EnviarAsync(comprobante, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = 1,
                Aceptado = true,
                Estado = "accepted",
                TrackingId = "SIFEN-1"
            })));

        var result = await handler.Handle(new SolicitarSifenParaguayComprobanteCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Aceptado.Should().BeTrue();
        result.Value.TrackingId.Should().Be("SIFEN-1");
        _repo.Received(1).Update(comprobante);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SolicitarSifenParaguay_ConError_NoPersiste()
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 10, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        var sifen = Substitute.For<IParaguaySifenComprobanteService>();
        var handler = new SolicitarSifenParaguayComprobanteCommandHandler(_repo, sifen, _uow);

        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(comprobante);
        sifen.EnviarAsync(comprobante, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<ResultadoEnvioSifenParaguayDto>("SIFEN rechazado")));

        var result = await handler.Handle(new SolicitarSifenParaguayComprobanteCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("SIFEN");
        comprobante.EstadoSifen.Should().BeNull();
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SolicitarSifenParaguay_ComprobanteYaAceptado_NoReenvia()
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 10, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        comprobante.RegistrarResultadoSifen(EstadoSifenParaguay.Aceptado, null, null, "SIFEN-1", null, null, DateTimeOffset.UtcNow, null);

        var sifen = Substitute.For<IParaguaySifenComprobanteService>();
        var handler = new SolicitarSifenParaguayComprobanteCommandHandler(_repo, sifen, _uow);

        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(comprobante);

        var result = await handler.Handle(new SolicitarSifenParaguayComprobanteCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ya fue aceptado");
        await sifen.DidNotReceive().EnviarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReintentarSifenParaguay_ComprobantePendiente_RetornaFailure()
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 10, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        comprobante.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, "SIFEN-1", null, null, DateTimeOffset.UtcNow, null);

        var sifen = Substitute.For<IParaguaySifenComprobanteService>();
        var handler = new ReintentarSifenParaguayComprobanteCommandHandler(_repo, sifen, _uow);

        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(comprobante);

        var result = await handler.Handle(new ReintentarSifenParaguayComprobanteCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("rechazado o error");
        await sifen.DidNotReceive().EnviarAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConciliarSifenParaguay_ComprobanteEncontrado_PersisteResultado()
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 10, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        comprobante.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo inicial", "SIFEN-1", "CDC-1", "LOTE-1", DateTimeOffset.UtcNow, null);
        var sifen = Substitute.For<IParaguaySifenComprobanteService>();
        var handler = new ConciliarSifenParaguayComprobanteCommandHandler(_repo, sifen, _uow);

        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(comprobante);
        sifen.ConciliarEstadoAsync(comprobante, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = 1,
                Aceptado = true,
                Estado = "approved",
                TrackingId = "SIFEN-1",
                Cdc = "CDC-1",
                NumeroLote = "LOTE-1"
            })));

        var result = await handler.Handle(new ConciliarSifenParaguayComprobanteCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Estado.Should().Be("approved");
        _repo.Received(1).Update(comprobante);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static TipoComprobante BuildTipoComprobante(long id, bool esVenta, short tipoAfip)
    {
        var tipo = (TipoComprobante)Activator.CreateInstance(typeof(TipoComprobante), nonPublic: true)!;
        SetProperty(tipo, nameof(TipoComprobante.Id), id);
        SetProperty(tipo, nameof(TipoComprobante.EsVenta), esVenta);
        SetProperty(tipo, nameof(TipoComprobante.TipoAfip), (short?)tipoAfip);
        return tipo;
    }

    private static PuntoFacturacion BuildPuntoFacturacion(long id, long sucursalId, short numero)
    {
        var punto = PuntoFacturacion.Crear(sucursalId, 1, numero, null, null);
        SetProperty(punto, nameof(PuntoFacturacion.Id), id);
        return punto;
    }

    private static Sucursal BuildSucursal(long id, string cuit)
    {
        var sucursal = Sucursal.Crear("Casa Central", cuit, 1, 1, 1, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static Tercero BuildTercero(long id, long tipoDocumentoId, string nroDocumento)
    {
        var tercero = Tercero.Crear("CLI1", "Cliente", tipoDocumentoId, nroDocumento, 1, true, false, false, 1, null);
        SetProperty(tercero, nameof(Tercero.Id), id);
        return tercero;
    }

    private static ZuluIA_Back.Domain.Entities.Referencia.TipoDocumento BuildTipoDocumento(long id, short codigo)
    {
        var tipo = (ZuluIA_Back.Domain.Entities.Referencia.TipoDocumento)Activator.CreateInstance(
            typeof(ZuluIA_Back.Domain.Entities.Referencia.TipoDocumento),
            nonPublic: true)!;
        SetProperty(tipo, nameof(ZuluIA_Back.Domain.Entities.Referencia.TipoDocumento.Id), id);
        SetProperty(tipo, nameof(ZuluIA_Back.Domain.Entities.Referencia.TipoDocumento.Codigo), codigo);
        return tipo;
    }

    private static Moneda BuildMoneda(long id, string codigo)
    {
        var moneda = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        SetProperty(moneda, nameof(Moneda.Id), id);
        SetProperty(moneda, nameof(Moneda.Codigo), codigo);
        return moneda;
    }

    private static AlicuotaIva BuildAlicuota(long id, short codigo, long porcentaje)
    {
        var alicuota = (AlicuotaIva)Activator.CreateInstance(typeof(AlicuotaIva), nonPublic: true)!;
        SetProperty(alicuota, nameof(AlicuotaIva.Id), id);
        SetProperty(alicuota, nameof(AlicuotaIva.Codigo), codigo);
        SetProperty(alicuota, nameof(AlicuotaIva.Porcentaje), porcentaje);
        return alicuota;
    }

    private static Impuesto BuildImpuesto(long id, string codigo, string descripcion, decimal alicuota)
    {
        var impuesto = Impuesto.Crear(codigo, descripcion, alicuota);
        SetProperty(impuesto, nameof(Impuesto.Id), id);
        return impuesto;
    }

    private static ImpuestoPorTipoComprobante BuildImpuestoPorTipoComprobante(long impuestoId, long tipoComprobanteId, int orden)
    {
        return ImpuestoPorTipoComprobante.Crear(impuestoId, tipoComprobanteId, orden);
    }

    private static Item BuildItem(long id, string descripcion)
    {
        var item = (Item)Activator.CreateInstance(typeof(Item), nonPublic: true)!;
        SetProperty(item, nameof(Item.Id), id);
        SetProperty(item, nameof(Item.Descripcion), descripcion);
        return item;
    }

    private static ConfiguracionFiscal BuildConfiguracionFiscal(long puntoFacturacionId, long tipoComprobanteId, bool online)
    {
        return ConfiguracionFiscal.Crear(puntoFacturacionId, tipoComprobanteId, online: online);
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class ReintentarSifenParaguayPendientesCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IParaguaySifenComprobanteService _sifen = Substitute.For<IParaguaySifenComprobanteService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private ReintentarSifenParaguayPendientesCommandHandler Sut() => new(_db, _repo, _sifen, _uow);

    [Fact]
    public async Task Handle_AplicaFiltrosOperativosYReintentaSoloElegibles()
    {
        var pais = BuildPais(20, "PY", "Paraguay");
        var sucursal = BuildSucursalParaguay(1, 20);
        var elegible = Comprobante.Crear(1, 1, 1, 1, 121, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var otroElegible = Comprobante.Crear(1, 1, 1, 1, 125, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var otroCodigo = Comprobante.Crear(1, 1, 1, 1, 122, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var pendiente = Comprobante.Crear(1, 1, 1, 1, 123, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var fueraFecha = Comprobante.Crear(1, 1, 1, 1, 124, new DateOnly(2026, 4, 20), null, 1, 1, 1m, null, null);

        SetProperty(elegible, nameof(Comprobante.Id), 31L);
        SetProperty(otroElegible, nameof(Comprobante.Id), 35L);
        SetProperty(otroCodigo, nameof(Comprobante.Id), 32L);
        SetProperty(pendiente, nameof(Comprobante.Id), 33L);
        SetProperty(fueraFecha, nameof(Comprobante.Id), 34L);
        SetProperty(elegible, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(otroElegible, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(otroCodigo, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(fueraFecha, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        elegible.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo", "track-31", null, null, DateTimeOffset.UtcNow, null);
        otroElegible.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo 2", "track-35", null, null, DateTimeOffset.UtcNow, null);
        otroCodigo.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "151", "otro rechazo", "track-32", null, null, DateTimeOffset.UtcNow, null);
        pendiente.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, "track-33", null, null, DateTimeOffset.UtcNow, null);
        fueraFecha.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "150", "fuera fecha", "track-34", null, null, DateTimeOffset.UtcNow, null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { elegible, otroElegible, otroCodigo, pendiente, fueraFecha });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        _repo.GetByIdConItemsAsync(31, Arg.Any<CancellationToken>()).Returns(elegible);
        _repo.GetByIdConItemsAsync(35, Arg.Any<CancellationToken>()).Returns(otroElegible);
        _sifen.EnviarAsync(elegible, Arg.Any<CancellationToken>())
            .Returns(Result.Success(new ResultadoEnvioSifenParaguayDto { ComprobanteId = 31, Estado = "accepted", Aceptado = true, TrackingId = "track-31" }));

        var result = await Sut().Handle(
            new ReintentarSifenParaguayPendientesCommand(
                MaxItems: 1,
                SucursalId: 1,
                EstadoSifen: nameof(EstadoSifenParaguay.Rechazado),
                CodigoRespuesta: "150",
                FechaDesde: new DateOnly(2026, 4, 1),
                FechaHasta: new DateOnly(2026, 4, 15)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Encontrados.Should().Be(1);
        result.Value.TotalElegibles.Should().Be(2);
        result.Value.HayMasResultados.Should().BeTrue();
        result.Value.Procesados.Should().Be(1);
        result.Value.Items.Should().ContainSingle(x => x.ComprobanteId == 31 && x.Exitoso);
        result.Value.Estados.Should().ContainSingle(x => x.Estado == "accepted" && x.Cantidad == 1);
        result.Value.Errores.Should().BeEmpty();
        await _repo.Received(1).GetByIdConItemsAsync(31, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByIdConItemsAsync(35, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByIdConItemsAsync(32, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByIdConItemsAsync(33, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByIdConItemsAsync(34, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MaxItemsInvalido_RetornaFailure()
    {
        var result = await Sut().Handle(new ReintentarSifenParaguayPendientesCommand(0, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MaxItems");
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = (Pais)Activator.CreateInstance(typeof(Pais), nonPublic: true)!;
        SetProperty(pais, nameof(Pais.Id), id);
        SetProperty(pais, nameof(Pais.Codigo), codigo);
        SetProperty(pais, nameof(Pais.Descripcion), descripcion);
        return pais;
    }

    private static Sucursal BuildSucursalParaguay(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal PY", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class PreviewReintentarSifenParaguayPendientesQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private PreviewReintentarSifenParaguayPendientesQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_RetornaLoteRealConMismoOrdenYFiltros()
    {
        var pais = BuildPais(20, "PY", "Paraguay");
        var sucursal = BuildSucursalParaguay(1, 20);
        var primero = Comprobante.Crear(1, 1, 1, 1, 131, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var segundo = Comprobante.Crear(1, 1, 1, 1, 132, new DateOnly(2026, 4, 11), null, 1, 1, 1m, null, null);
        var tercero = Comprobante.Crear(1, 1, 1, 1, 133, new DateOnly(2026, 4, 12), null, 1, 1, 1m, null, null);
        var cuarto = Comprobante.Crear(1, 1, 1, 1, 135, new DateOnly(2026, 4, 12), null, 1, 1, 1m, null, null);
        var pendiente = Comprobante.Crear(1, 1, 1, 1, 134, new DateOnly(2026, 4, 13), null, 1, 1, 1m, null, null);

        SetProperty(primero, nameof(Comprobante.Id), 41L);
        SetProperty(segundo, nameof(Comprobante.Id), 42L);
        SetProperty(tercero, nameof(Comprobante.Id), 43L);
        SetProperty(cuarto, nameof(Comprobante.Id), 45L);
        SetProperty(pendiente, nameof(Comprobante.Id), 44L);
        SetProperty(primero, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(segundo, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(tercero, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(cuarto, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        primero.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo 1", "track-41", null, null, new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero), null);
        segundo.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo 2", "track-42", null, null, new DateTimeOffset(2026, 4, 10, 9, 0, 0, TimeSpan.Zero), null);
        tercero.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", "track-43", null, null, new DateTimeOffset(2026, 4, 10, 11, 0, 0, TimeSpan.Zero), null);
        cuarto.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo 3", "track-45", null, null, new DateTimeOffset(2026, 4, 10, 12, 0, 0, TimeSpan.Zero), null);
        pendiente.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, "track-44", null, null, new DateTimeOffset(2026, 4, 10, 8, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { primero, segundo, tercero, cuarto, pendiente });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new PreviewReintentarSifenParaguayPendientesQuery(
                MaxItems: 2,
                SucursalId: 1,
                EstadoSifen: nameof(EstadoSifenParaguay.Rechazado),
                CodigoRespuesta: "150",
                FechaDesde: new DateOnly(2026, 4, 1),
                FechaHasta: new DateOnly(2026, 4, 15)),
            CancellationToken.None);

        result.MaxItems.Should().Be(2);
        result.Encontrados.Should().Be(2);
        result.TotalElegibles.Should().Be(3);
        result.HayMasResultados.Should().BeTrue();
        result.Items.Select(x => x.ComprobanteId).Should().Equal(42, 41);
        result.Items.Should().OnlyContain(x => x.EstadoSifen == EstadoSifenParaguay.Rechazado && x.CodigoRespuesta == "150");
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == EstadoSifenParaguay.Rechazado && x.Cantidad == 2);
        result.CodigosRespuesta.Should().ContainSingle(x => x.CodigoRespuesta == "150" && x.Cantidad == 2);
        result.MensajesRespuesta.Should().ContainSingle(x => x.MensajeRespuesta == "rechazo 1" && x.Cantidad == 1);
        result.MensajesRespuesta.Should().ContainSingle(x => x.MensajeRespuesta == "rechazo 2" && x.Cantidad == 1);
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = (Pais)Activator.CreateInstance(typeof(Pais), nonPublic: true)!;
        SetProperty(pais, nameof(Pais.Id), id);
        SetProperty(pais, nameof(Pais.Codigo), codigo);
        SetProperty(pais, nameof(Pais.Descripcion), descripcion);
        return pais;
    }

    private static Sucursal BuildSucursalParaguay(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal PY", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

// ── ImputarComprobanteCommandHandler ─────────────────────────────────────────

public class ImputarComprobanteCommandHandlerTests
{
    private readonly ComprobanteService _comprobanteService = Substitute.For<ComprobanteService>(
        Substitute.For<IComprobanteRepository>(),
        Substitute.For<IImputacionRepository>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private ImputarComprobanteCommandHandler Sut() => new(_comprobanteService, _uow, _user, _db);

    [Fact]
    public async Task Handle_ImputacionValida_LlamaServicioYRetornaId()
    {
        _user.UserId.Returns((long?)1L);
        var imputacion = Imputacion.Crear(1, 2, 500m, DateOnly.FromDateTime(DateTime.Today), null);
        _comprobanteService
            .ImputarAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<decimal>(),
                Arg.Any<DateOnly>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(imputacion);

        var cmd = new ImputarComprobanteCommand(1, 2, 500m, DateOnly.FromDateTime(DateTime.Today));
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── ImputarComprobantesMasivosCommandHandler ──────────────────────────────────

public class ImputarComprobantesMasivosCommandHandlerTests
{
    private readonly ComprobanteService _comprobanteService = Substitute.For<ComprobanteService>(
        Substitute.For<IComprobanteRepository>(),
        Substitute.For<IImputacionRepository>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private ImputarComprobantesMasivosCommandHandler Sut() =>
        new(_comprobanteService, _uow, _user, _db);

    [Fact]
    public async Task Handle_SinItems_RetornaFailure()
    {
        var cmd = new ImputarComprobantesMasivosCommand(DateOnly.FromDateTime(DateTime.Today), []);
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConItems_LlamaServicioParaCadaItemYRetornaIds()
    {
        _user.UserId.Returns((long?)1L);
        var imputacion = Imputacion.Crear(1, 2, 500m, DateOnly.FromDateTime(DateTime.Today), null);
        _comprobanteService
            .ImputarAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<decimal>(),
                Arg.Any<DateOnly>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(imputacion);

        var items = new List<ImputacionMasivaItemDto>
        {
            new(1, 2, 500m),
            new(3, 4, 300m)
        };
        var cmd = new ImputarComprobantesMasivosCommand(DateOnly.FromDateTime(DateTime.Today), items);
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(2);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ServicioLanzaExcepcion_RetornaFailure()
    {
        _comprobanteService
            .ImputarAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<decimal>(),
                Arg.Any<DateOnly>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Saldo insuficiente"));

        var items = new List<ImputacionMasivaItemDto> { new(1, 2, 9999m) };
        var cmd = new ImputarComprobantesMasivosCommand(DateOnly.FromDateTime(DateTime.Today), items);
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

// ── GetComprobanteByIdQueryHandler ────────────────────────────────────────────

public class GetComprobanteByIdQueryHandlerTests
{
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    private GetComprobanteByIdQueryHandler Sut() => new(_repo, _db, _mapper);

    [Fact]
    public async Task Handle_ComprobanteNoEncontrado_RetornaNull()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Comprobante?)null);

        var result = await Sut().Handle(new GetComprobanteByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComprobanteEncontrado_MapaYRetorna()
    {
        var comp = Comprobante.Crear(1, null, 1, 1, 1,
            DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        SetProperty(comp, nameof(Comprobante.Id), 1L);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(comp);
        _mapper.Map<ComprobanteDto>(comp).Returns(new ComprobanteDto
        {
            TimbradoId = 10,
            NroTimbrado = "12345678",
            EstadoSifen = EstadoSifenParaguay.Aceptado,
            SifenCodigoRespuesta = null,
            SifenMensajeRespuesta = null,
            SifenTrackingId = "SIFEN-10",
            SifenCdc = "CDC-10",
            SifenNumeroLote = "LOTE-10",
            SifenFechaRespuesta = new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero),
            TieneIdentificadoresSifen = true,
            PuedeReintentarSifen = false,
            PuedeConciliarSifen = false
        });
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            ComprobanteImpuesto.Crear(1, 1, 21m, 1000m, 210m)
        });
        var tributos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            ComprobanteTributo.Crear(1, 10, "99", "Percepciones", 1000m, 3.5m, 35m, 1)
        });
        _db.ComprobantesImpuestos.Returns(impuestos);
        _db.ComprobantesTributos.Returns(tributos);

        var result = await Sut().Handle(new GetComprobanteByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Impuestos.Should().ContainSingle();
        result.Impuestos[0].ImporteIva.Should().Be(210m);
        result.Tributos.Should().ContainSingle();
        result.Tributos[0].Importe.Should().Be(35m);
        result.TimbradoId.Should().Be(10);
        result.NroTimbrado.Should().Be("12345678");
        result.EstadoSifen.Should().Be(EstadoSifenParaguay.Aceptado);
        result.SifenCodigoRespuesta.Should().BeNull();
        result.SifenMensajeRespuesta.Should().BeNull();
        result.SifenTrackingId.Should().Be("SIFEN-10");
        result.SifenCdc.Should().Be("CDC-10");
        result.SifenNumeroLote.Should().Be("LOTE-10");
        result.SifenFechaRespuesta.Should().Be(new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero));
        result.TieneIdentificadoresSifen.Should().BeTrue();
        result.PuedeReintentarSifen.Should().BeFalse();
        result.PuedeConciliarSifen.Should().BeFalse();
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class GetComprobanteSifenEstadoQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetComprobanteSifenEstadoQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_ComprobanteAceptado_RetornaEstadoYNoPermiteReintento()
    {
        var comp = Comprobante.Crear(1, null, 1, 1, 1,
            DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        SetProperty(comp, nameof(Comprobante.Id), 1L);
        comp.AsignarTimbrado(10, "12345678", null);
        comp.RegistrarResultadoSifen(EstadoSifenParaguay.Aceptado, null, null, "SIFEN-10", "CDC-10", "LOTE-10", new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { comp });

        _db.Comprobantes.Returns(comprobantes);

        var result = await Sut().Handle(new GetComprobanteSifenEstadoQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.EstadoSifen.Should().Be(EstadoSifenParaguay.Aceptado);
        result.FueAceptado.Should().BeTrue();
        result.PuedeReintentar.Should().BeFalse();
        result.SifenCodigoRespuesta.Should().BeNull();
        result.SifenMensajeRespuesta.Should().BeNull();
        result.SifenTrackingId.Should().Be("SIFEN-10");
        result.SifenCdc.Should().Be("CDC-10");
        result.SifenNumeroLote.Should().Be("LOTE-10");
        result.TieneIdentificadores.Should().BeTrue();
        result.PuedeConciliar.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ComprobanteConError_PermiteReintento()
    {
        var comp = Comprobante.Crear(1, null, 1, 1, 1,
            DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        SetProperty(comp, nameof(Comprobante.Id), 2L);
        comp.AsignarTimbrado(11, "87654321", null);
        comp.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, null, null, new DateTimeOffset(2026, 3, 20, 19, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { comp });

        _db.Comprobantes.Returns(comprobantes);

        var result = await Sut().Handle(new GetComprobanteSifenEstadoQuery(2), CancellationToken.None);

        result.Should().NotBeNull();
        result!.EstadoSifen.Should().Be(EstadoSifenParaguay.Error);
        result.SifenCodigoRespuesta.Should().Be("E001");
        result.SifenMensajeRespuesta.Should().Be("timeout");
        result.FueAceptado.Should().BeFalse();
        result.TieneIdentificadores.Should().BeFalse();
        result.PuedeReintentar.Should().BeTrue();
        result.PuedeConciliar.Should().BeFalse();
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class GetComprobanteSifenHistorialQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetComprobanteSifenHistorialQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_ConRegistros_RetornaHistorialOrdenadoDescendente()
    {
        var primero = HistorialSifenComprobante.Registrar(
            10,
            7,
            EstadoSifenParaguay.Rechazado,
            false,
            "rejected",
            "150",
            "rechazo inicial",
            "track-1",
            "cdc-1",
            "lote-1",
            new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero),
            "rechazo inicial",
            "{\"accepted\":false}");
        var segundo = HistorialSifenComprobante.Registrar(
            10,
            7,
            EstadoSifenParaguay.Aceptado,
            true,
            "accepted",
            null,
            null,
            "track-2",
            "cdc-2",
            "lote-2",
            new DateTimeOffset(2026, 3, 20, 18, 5, 0, TimeSpan.Zero),
            "reintento aprobado",
            "{\"accepted\":true}");

        SetProperty(primero, nameof(HistorialSifenComprobante.Id), 1L);
        SetProperty(segundo, nameof(HistorialSifenComprobante.Id), 2L);
        SetProperty(primero, nameof(HistorialSifenComprobante.FechaHora), new DateTime(2026, 3, 20, 18, 0, 0, DateTimeKind.Utc));
        SetProperty(segundo, nameof(HistorialSifenComprobante.FechaHora), new DateTime(2026, 3, 20, 18, 5, 0, DateTimeKind.Utc));

        var historial = MockDbSetHelper.CreateMockDbSet(new[] { primero, segundo });
        _db.HistorialSifenComprobantes.Returns(historial);

        var result = await Sut().Handle(new GetComprobanteSifenHistorialQuery(10), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(2);
        result[0].EstadoSifen.Should().Be(EstadoSifenParaguay.Aceptado);
        result[0].CodigoRespuesta.Should().BeNull();
        result[0].MensajeRespuesta.Should().BeNull();
        result[0].Cdc.Should().Be("cdc-2");
        result[0].NumeroLote.Should().Be("lote-2");
        result[1].Id.Should().Be(1);
        result[1].EstadoSifen.Should().Be(EstadoSifenParaguay.Rechazado);
        result[1].CodigoRespuesta.Should().Be("150");
        result[1].MensajeRespuesta.Should().Be("rechazo inicial");
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class GetComprobantesSifenPendientesQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetComprobantesSifenPendientesQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_RetornaSoloParaguayEmitidosNoAceptados()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var paisAr = BuildPais(21, "AR", "Argentina");
        var sucursalPy = BuildSucursal(1, 20);
        var sucursalAr = BuildSucursal(2, 21);

        var pendiente = Comprobante.Crear(1, 1, 1, 1, 101, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        var rechazado = Comprobante.Crear(1, 1, 1, 1, 102, new DateOnly(2026, 4, 2), null, 1, 1, 1m, null, null);
        var aceptado = Comprobante.Crear(1, 1, 1, 1, 103, new DateOnly(2026, 4, 3), null, 1, 1, 1m, null, null);
        var borrador = Comprobante.Crear(2, 1, 1, 1, 104, new DateOnly(2026, 4, 4), null, 1, 1, 1m, null, null);

        SetProperty(pendiente, nameof(Comprobante.Id), 11L);
        SetProperty(rechazado, nameof(Comprobante.Id), 12L);
        SetProperty(aceptado, nameof(Comprobante.Id), 13L);
        SetProperty(borrador, nameof(Comprobante.Id), 14L);

        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(rechazado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(aceptado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(borrador, nameof(Comprobante.Estado), EstadoComprobante.Borrador);

        rechazado.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo", "track-12", "cdc-12", "lote-12", new DateTimeOffset(2026, 4, 2, 10, 0, 0, TimeSpan.Zero), null);
        aceptado.RegistrarResultadoSifen(EstadoSifenParaguay.Aceptado, null, null, "track-13", "cdc-13", "lote-13", new DateTimeOffset(2026, 4, 3, 10, 0, 0, TimeSpan.Zero), null);

        var extranjero = Comprobante.Crear(2, 1, 1, 2, 105, new DateOnly(2026, 4, 5), null, 1, 1, 1m, null, null);
        SetProperty(extranjero, nameof(Comprobante.Id), 15L);
        SetProperty(extranjero, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        extranjero.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "151", "otro pais", null, null, null, new DateTimeOffset(2026, 4, 5, 10, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { pendiente, rechazado, aceptado, borrador, extranjero });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy, sucursalAr });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy, paisAr });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(new GetComprobantesSifenPendientesQuery(1, 20), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().ContainSingle(x => x.ComprobanteId == 11 && x.EstadoSifen == null);
        result.Items.Should().ContainSingle(x => x.ComprobanteId == 12
            && x.EstadoSifen == EstadoSifenParaguay.Rechazado
            && x.SifenCodigoRespuesta == "150"
            && x.SifenMensajeRespuesta == "rechazo"
            && x.SifenTrackingId == "track-12"
            && x.SifenCdc == "cdc-12"
            && x.SifenNumeroLote == "lote-12"
            && x.TieneIdentificadores
            && x.PuedeReintentar
            && x.PuedeConciliar);
        result.Items.Should().ContainSingle(x => x.ComprobanteId == 11
            && !x.TieneIdentificadores
            && !x.PuedeConciliar);
    }

    [Fact]
    public async Task Handle_AplicaFiltrosOperativos()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var rechazadoConIds = Comprobante.Crear(1, 1, 1, 1, 201, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var errorSinIds = Comprobante.Crear(1, 1, 1, 1, 202, new DateOnly(2026, 4, 11), null, 1, 1, 1m, null, null);

        SetProperty(rechazadoConIds, nameof(Comprobante.Id), 21L);
        SetProperty(errorSinIds, nameof(Comprobante.Id), 22L);
        SetProperty(rechazadoConIds, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(errorSinIds, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        rechazadoConIds.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo con ids", "track-21", "cdc-21", null, new DateTimeOffset(2026, 4, 10, 15, 0, 0, TimeSpan.Zero), null);
        errorSinIds.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, null, null, new DateTimeOffset(2026, 4, 11, 15, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { rechazadoConIds, errorSinIds });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesQuery(
                Page: 1,
                PageSize: 20,
                SucursalId: 1,
                EstadoSifen: nameof(EstadoSifenParaguay.Rechazado),
                CodigoRespuesta: "150",
                SoloConIdentificadores: true),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].ComprobanteId.Should().Be(21);
        result.Items[0].SifenCodigoRespuesta.Should().Be("150");
        result.Items[0].SifenTrackingId.Should().Be("track-21");
    }

    [Fact]
    public async Task Handle_PuedeReintentarTrue_RetornaSoloRechazadosOError()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var rechazado = Comprobante.Crear(1, 1, 1, 1, 211, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var error = Comprobante.Crear(1, 1, 1, 1, 212, new DateOnly(2026, 4, 11), null, 1, 1, 1m, null, null);
        var pendiente = Comprobante.Crear(1, 1, 1, 1, 213, new DateOnly(2026, 4, 12), null, 1, 1, 1m, null, null);

        SetProperty(rechazado, nameof(Comprobante.Id), 24L);
        SetProperty(error, nameof(Comprobante.Id), 25L);
        SetProperty(pendiente, nameof(Comprobante.Id), 26L);
        SetProperty(rechazado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(error, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        rechazado.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo", null, null, null, new DateTimeOffset(2026, 4, 10, 15, 0, 0, TimeSpan.Zero), null);
        error.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, null, null, new DateTimeOffset(2026, 4, 11, 15, 0, 0, TimeSpan.Zero), null);
        pendiente.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, null, null, null, new DateTimeOffset(2026, 4, 12, 15, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { rechazado, error, pendiente });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesQuery(Page: 1, PageSize: 20, PuedeReintentar: true),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Select(x => x.ComprobanteId).Should().BeEquivalentTo([24L, 25L]);
        result.Items.Should().OnlyContain(x => x.PuedeReintentar);
    }

    [Fact]
    public async Task Handle_PuedeReintentarFalse_ExcluyeRechazadosYErrores()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var pendiente = Comprobante.Crear(1, 1, 1, 1, 221, new DateOnly(2026, 4, 13), null, 1, 1, 1m, null, null);
        var sinEstado = Comprobante.Crear(1, 1, 1, 1, 222, new DateOnly(2026, 4, 14), null, 1, 1, 1m, null, null);
        var rechazado = Comprobante.Crear(1, 1, 1, 1, 223, new DateOnly(2026, 4, 15), null, 1, 1, 1m, null, null);

        SetProperty(pendiente, nameof(Comprobante.Id), 27L);
        SetProperty(sinEstado, nameof(Comprobante.Id), 28L);
        SetProperty(rechazado, nameof(Comprobante.Id), 29L);
        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(sinEstado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(rechazado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        pendiente.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, null, null, null, new DateTimeOffset(2026, 4, 13, 15, 0, 0, TimeSpan.Zero), null);
        rechazado.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo", null, null, null, new DateTimeOffset(2026, 4, 15, 15, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { pendiente, sinEstado, rechazado });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesQuery(Page: 1, PageSize: 20, PuedeReintentar: false, SortBy: "fechaAsc"),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Select(x => x.ComprobanteId).Should().Equal(27, 28);
        result.Items.Should().OnlyContain(x => !x.PuedeReintentar);
    }

    [Fact]
    public async Task Handle_AplicaRangoFechas()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var fueraDesde = Comprobante.Crear(1, 1, 1, 1, 301, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        var dentro = Comprobante.Crear(1, 1, 1, 1, 302, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var fueraHasta = Comprobante.Crear(1, 1, 1, 1, 303, new DateOnly(2026, 4, 20), null, 1, 1, 1m, null, null);

        SetProperty(fueraDesde, nameof(Comprobante.Id), 31L);
        SetProperty(dentro, nameof(Comprobante.Id), 32L);
        SetProperty(fueraHasta, nameof(Comprobante.Id), 33L);
        SetProperty(fueraDesde, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(dentro, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(fueraHasta, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { fueraDesde, dentro, fueraHasta });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesQuery(
                Page: 1,
                PageSize: 20,
                FechaDesde: new DateOnly(2026, 4, 5),
                FechaHasta: new DateOnly(2026, 4, 15)),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].ComprobanteId.Should().Be(32);
        result.Items[0].Fecha.Should().Be(new DateOnly(2026, 4, 10));
    }

    [Fact]
    public async Task Handle_SortByFechaAsc_OrdenaPorFechaAscendente()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var primero = Comprobante.Crear(1, 1, 1, 1, 401, new DateOnly(2026, 4, 12), null, 1, 1, 1m, null, null);
        var segundo = Comprobante.Crear(1, 1, 1, 1, 402, new DateOnly(2026, 4, 8), null, 1, 1, 1m, null, null);
        var tercero = Comprobante.Crear(1, 1, 1, 1, 403, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);

        SetProperty(primero, nameof(Comprobante.Id), 41L);
        SetProperty(segundo, nameof(Comprobante.Id), 42L);
        SetProperty(tercero, nameof(Comprobante.Id), 43L);
        SetProperty(primero, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(segundo, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(tercero, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { primero, segundo, tercero });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesQuery(Page: 1, PageSize: 20, SortBy: "fechaAsc"),
            CancellationToken.None);

        result.Items.Select(x => x.ComprobanteId).Should().Equal(42, 43, 41);
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = (Pais)Activator.CreateInstance(typeof(Pais), nonPublic: true)!;
        SetProperty(pais, nameof(Pais.Id), id);
        SetProperty(pais, nameof(Pais.Codigo), codigo);
        SetProperty(pais, nameof(Pais.Descripcion), descripcion);
        return pais;
    }

    private static Sucursal BuildSucursal(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class GetComprobantesSifenPendientesResumenQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetComprobantesSifenPendientesResumenQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_RetornaTotalesYEstadosAgrupados()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);

        var sinEstado = Comprobante.Crear(1, 1, 1, 1, 501, new DateOnly(2026, 4, 21), null, 1, 1, 1m, null, null);
        var pendiente = Comprobante.Crear(1, 1, 1, 1, 502, new DateOnly(2026, 4, 22), null, 1, 1, 1m, null, null);
        var rechazado = Comprobante.Crear(1, 1, 1, 1, 503, new DateOnly(2026, 4, 23), null, 1, 1, 1m, null, null);
        var rechazadoMismoCodigo = Comprobante.Crear(1, 1, 1, 1, 506, new DateOnly(2026, 4, 23), null, 1, 1, 1m, null, null);
        var error = Comprobante.Crear(1, 1, 1, 1, 504, new DateOnly(2026, 4, 24), null, 1, 1, 1m, null, null);
        var aceptado = Comprobante.Crear(1, 1, 1, 1, 505, new DateOnly(2026, 4, 25), null, 1, 1, 1m, null, null);

        SetProperty(sinEstado, nameof(Comprobante.Id), 51L);
        SetProperty(pendiente, nameof(Comprobante.Id), 52L);
        SetProperty(rechazado, nameof(Comprobante.Id), 53L);
        SetProperty(rechazadoMismoCodigo, nameof(Comprobante.Id), 56L);
        SetProperty(error, nameof(Comprobante.Id), 54L);
        SetProperty(aceptado, nameof(Comprobante.Id), 55L);

        SetProperty(sinEstado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(rechazado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(rechazadoMismoCodigo, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(error, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(aceptado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        pendiente.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, null, null, null, new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero), null);
        rechazado.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo", "track-53", null, null, new DateTimeOffset(2026, 4, 23, 10, 0, 0, TimeSpan.Zero), null);
        rechazadoMismoCodigo.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo repetido", "track-56", null, null, new DateTimeOffset(2026, 4, 23, 11, 0, 0, TimeSpan.Zero), null);
        error.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, "cdc-54", null, new DateTimeOffset(2026, 4, 24, 10, 0, 0, TimeSpan.Zero), null);
        aceptado.RegistrarResultadoSifen(EstadoSifenParaguay.Aceptado, null, null, null, null, null, new DateTimeOffset(2026, 4, 25, 10, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { sinEstado, pendiente, rechazado, rechazadoMismoCodigo, error, aceptado });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(new GetComprobantesSifenPendientesResumenQuery(), CancellationToken.None);

        result.Total.Should().Be(5);
        result.Reintentables.Should().Be(3);
        result.ConIdentificadores.Should().Be(3);
        result.Conciliables.Should().Be(3);
        result.SinEstadoSifen.Should().Be(1);
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == null && x.Cantidad == 1);
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == EstadoSifenParaguay.Pendiente && x.Cantidad == 1);
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == EstadoSifenParaguay.Rechazado && x.Cantidad == 2);
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == EstadoSifenParaguay.Error && x.Cantidad == 1);
        result.CodigosRespuesta.Should().HaveCount(2);
        result.CodigosRespuesta[0].CodigoRespuesta.Should().Be("150");
        result.CodigosRespuesta[0].Cantidad.Should().Be(2);
        result.CodigosRespuesta[1].CodigoRespuesta.Should().Be("E001");
        result.CodigosRespuesta[1].Cantidad.Should().Be(1);
        result.MensajesRespuesta.Should().HaveCount(3);
        result.MensajesRespuesta[0].MensajeRespuesta.Should().Be("rechazo");
        result.MensajesRespuesta[0].Cantidad.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AplicaMismosFiltrosQueLaBandeja()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var rechazado = Comprobante.Crear(1, 1, 1, 1, 601, new DateOnly(2026, 4, 26), null, 1, 1, 1m, null, null);
        var pendiente = Comprobante.Crear(1, 1, 1, 1, 602, new DateOnly(2026, 4, 27), null, 1, 1, 1m, null, null);
        var errorSinIds = Comprobante.Crear(1, 1, 1, 1, 603, new DateOnly(2026, 4, 28), null, 1, 1, 1m, null, null);

        SetProperty(rechazado, nameof(Comprobante.Id), 61L);
        SetProperty(pendiente, nameof(Comprobante.Id), 62L);
        SetProperty(errorSinIds, nameof(Comprobante.Id), 63L);
        SetProperty(rechazado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(errorSinIds, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        rechazado.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo", "track-61", null, null, new DateTimeOffset(2026, 4, 26, 10, 0, 0, TimeSpan.Zero), null);
        pendiente.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, null, null, null, new DateTimeOffset(2026, 4, 27, 10, 0, 0, TimeSpan.Zero), null);
        errorSinIds.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, null, null, new DateTimeOffset(2026, 4, 28, 10, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { rechazado, pendiente, errorSinIds });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesResumenQuery(
                SucursalId: 1,
                PuedeReintentar: true,
                SoloConIdentificadores: true,
                FechaDesde: new DateOnly(2026, 4, 25),
                FechaHasta: new DateOnly(2026, 4, 27)),
            CancellationToken.None);

        result.Total.Should().Be(1);
        result.Reintentables.Should().Be(1);
        result.ConIdentificadores.Should().Be(1);
        result.Conciliables.Should().Be(1);
        result.SinEstadoSifen.Should().Be(0);
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == EstadoSifenParaguay.Rechazado && x.Cantidad == 1);
        result.CodigosRespuesta.Should().ContainSingle(x => x.CodigoRespuesta == "150" && x.Cantidad == 1);
        result.MensajesRespuesta.Should().ContainSingle(x => x.MensajeRespuesta == "rechazo" && x.Cantidad == 1);
    }

    [Fact]
    public async Task Handle_TopCodigos_LimitaRankingDeCodigosRespuesta()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var codigo150A = Comprobante.Crear(1, 1, 1, 1, 611, new DateOnly(2026, 4, 26), null, 1, 1, 1m, null, null);
        var codigo150B = Comprobante.Crear(1, 1, 1, 1, 612, new DateOnly(2026, 4, 27), null, 1, 1, 1m, null, null);
        var codigoE001 = Comprobante.Crear(1, 1, 1, 1, 613, new DateOnly(2026, 4, 28), null, 1, 1, 1m, null, null);

        SetProperty(codigo150A, nameof(Comprobante.Id), 64L);
        SetProperty(codigo150B, nameof(Comprobante.Id), 65L);
        SetProperty(codigoE001, nameof(Comprobante.Id), 66L);
        SetProperty(codigo150A, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(codigo150B, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(codigoE001, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        codigo150A.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo A", null, null, null, new DateTimeOffset(2026, 4, 26, 10, 0, 0, TimeSpan.Zero), null);
        codigo150B.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo B", null, null, null, new DateTimeOffset(2026, 4, 27, 10, 0, 0, TimeSpan.Zero), null);
        codigoE001.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, null, null, new DateTimeOffset(2026, 4, 28, 10, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { codigo150A, codigo150B, codigoE001 });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesResumenQuery(TopCodigos: 1),
            CancellationToken.None);

        result.Total.Should().Be(3);
        result.CodigosRespuesta.Should().HaveCount(1);
        result.CodigosRespuesta[0].CodigoRespuesta.Should().Be("150");
        result.CodigosRespuesta[0].Cantidad.Should().Be(2);
    }

    [Fact]
    public async Task Handle_TopMensajes_LimitaRankingDeMensajesRespuesta()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var rechazoA = Comprobante.Crear(1, 1, 1, 1, 621, new DateOnly(2026, 4, 26), null, 1, 1, 1m, null, null);
        var rechazoB = Comprobante.Crear(1, 1, 1, 1, 622, new DateOnly(2026, 4, 27), null, 1, 1, 1m, null, null);
        var timeout = Comprobante.Crear(1, 1, 1, 1, 623, new DateOnly(2026, 4, 28), null, 1, 1, 1m, null, null);

        SetProperty(rechazoA, nameof(Comprobante.Id), 67L);
        SetProperty(rechazoB, nameof(Comprobante.Id), 68L);
        SetProperty(timeout, nameof(Comprobante.Id), 69L);
        SetProperty(rechazoA, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(rechazoB, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(timeout, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        rechazoA.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo repetido", null, null, null, new DateTimeOffset(2026, 4, 26, 10, 0, 0, TimeSpan.Zero), null);
        rechazoB.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "151", "rechazo repetido", null, null, null, new DateTimeOffset(2026, 4, 27, 10, 0, 0, TimeSpan.Zero), null);
        timeout.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, null, null, new DateTimeOffset(2026, 4, 28, 10, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { rechazoA, rechazoB, timeout });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new GetComprobantesSifenPendientesResumenQuery(TopMensajes: 1),
            CancellationToken.None);

        result.Total.Should().Be(3);
        result.MensajesRespuesta.Should().HaveCount(1);
        result.MensajesRespuesta[0].MensajeRespuesta.Should().Be("rechazo repetido");
        result.MensajesRespuesta[0].Cantidad.Should().Be(2);
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = (Pais)Activator.CreateInstance(typeof(Pais), nonPublic: true)!;
        SetProperty(pais, nameof(Pais.Id), id);
        SetProperty(pais, nameof(Pais.Codigo), codigo);
        SetProperty(pais, nameof(Pais.Descripcion), descripcion);
        return pais;
    }

    private static Sucursal BuildSucursal(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class ExportarComprobantesSifenPendientesCsvQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private ExportarComprobantesSifenPendientesCsvQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_GeneraCsvFiltradoYEscapaCampos()
    {
        var paisPy = BuildPais(20, "PY", "Paraguay");
        var sucursalPy = BuildSucursal(1, 20);
        var rechazado = Comprobante.Crear(1, 1, 1, 1, 701, new DateOnly(2026, 4, 29), null, 1, 1, 1m, null, null);
        var error = Comprobante.Crear(1, 1, 1, 1, 702, new DateOnly(2026, 4, 30), null, 1, 1, 1m, null, null);

        SetProperty(rechazado, nameof(Comprobante.Id), 71L);
        SetProperty(error, nameof(Comprobante.Id), 72L);
        SetProperty(rechazado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(error, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        rechazado.AsignarTimbrado(10, "12345678", null);
        rechazado.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo, coma y \"comillas\"", "track-71", "cdc-71", null, new DateTimeOffset(2026, 4, 29, 10, 0, 0, TimeSpan.Zero), null);
        error.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", null, null, null, new DateTimeOffset(2026, 4, 30, 10, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { rechazado, error });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursalPy });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { paisPy });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new ExportarComprobantesSifenPendientesCsvQuery(
                EstadoSifen: nameof(EstadoSifenParaguay.Rechazado),
                CodigoRespuesta: "150",
                PuedeReintentar: true,
                SoloConIdentificadores: true),
            CancellationToken.None);

        result.CantidadRegistros.Should().Be(1);
        result.NombreArchivo.Should().StartWith("SIFEN_PENDIENTES_");
        result.NombreArchivo.Should().EndWith(".csv");
        result.Contenido.Should().Contain("comprobanteId,sucursalId,terceroId");
        result.Contenido.Should().Contain("tieneIdentificadores,puedeReintentar,puedeConciliar");
        result.Contenido.Should().Contain("71,1,1,1,701,2026-04-29,Emitido,Rechazado,150,\"rechazo, coma y \"\"comillas\"\"\"",
            because: "el CSV debe incluir las columnas de elegibilidad operativa");
        result.Contenido.Should().Contain(",12345678,true,true,true");
        result.Contenido.Should().NotContain("72,1,1,1,702");
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = (Pais)Activator.CreateInstance(typeof(Pais), nonPublic: true)!;
        SetProperty(pais, nameof(Pais.Id), id);
        SetProperty(pais, nameof(Pais.Codigo), codigo);
        SetProperty(pais, nameof(Pais.Descripcion), descripcion);
        return pais;
    }

    private static Sucursal BuildSucursal(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class ConciliarSifenParaguayPendientesCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IParaguaySifenComprobanteService _sifen = Substitute.For<IParaguaySifenComprobanteService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private ConciliarSifenParaguayPendientesCommandHandler Sut() => new(_db, _repo, _sifen, _uow);

    [Fact]
    public async Task Handle_ConElegibles_ProcesaYDevuelveResumen()
    {
        var pais = BuildPais(20, "PY", "Paraguay");
        var sucursal = BuildSucursalParaguay(1, 20);
        var comp1 = Comprobante.Crear(1, 1, 1, 1, 101, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        var comp2 = Comprobante.Crear(1, 1, 1, 1, 102, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        var comp3 = Comprobante.Crear(1, 1, 1, 1, 103, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, null);
        SetProperty(comp1, nameof(Comprobante.Id), 11L);
        SetProperty(comp2, nameof(Comprobante.Id), 12L);
        SetProperty(comp3, nameof(Comprobante.Id), 13L);
        SetProperty(comp1, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(comp2, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(comp3, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        comp1.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo batch", "track-11", "cdc-11", null, DateTimeOffset.UtcNow, null);
        comp2.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout batch", "track-12", null, "lote-12", DateTimeOffset.UtcNow, null);
        comp3.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo extra", "track-13", null, null, DateTimeOffset.UtcNow, null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { comp1, comp2, comp3 });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        _repo.GetByIdConItemsAsync(11, Arg.Any<CancellationToken>()).Returns(comp1);
        _repo.GetByIdConItemsAsync(12, Arg.Any<CancellationToken>()).Returns(comp2);
        _sifen.ConciliarEstadoAsync(comp1, Arg.Any<CancellationToken>())
            .Returns(Result.Success(new ResultadoEnvioSifenParaguayDto { ComprobanteId = 11, Aceptado = true, Estado = "approved", TrackingId = "track-11" }));
        _sifen.ConciliarEstadoAsync(comp2, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ResultadoEnvioSifenParaguayDto>("timeout"));

        var result = await Sut().Handle(new ConciliarSifenParaguayPendientesCommand(2, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Encontrados.Should().Be(2);
        result.Value.TotalElegibles.Should().Be(3);
        result.Value.HayMasResultados.Should().BeTrue();
        result.Value.Procesados.Should().Be(2);
        result.Value.Exitosos.Should().Be(1);
        result.Value.Fallidos.Should().Be(1);
        result.Value.Items.Should().ContainSingle(x => x.ComprobanteId == 11 && x.Exitoso);
        result.Value.Items.Should().ContainSingle(x => x.ComprobanteId == 12 && !x.Exitoso && x.Error == "timeout");
        result.Value.Estados.Should().ContainSingle(x => x.Estado == "approved" && x.Cantidad == 1);
        result.Value.Errores.Should().ContainSingle(x => x.Error == "timeout" && x.Cantidad == 1);
        _repo.Received(1).Update(comp1);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MaxItemsInvalido_RetornaFailure()
    {
        var result = await Sut().Handle(new ConciliarSifenParaguayPendientesCommand(0, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MaxItems");
    }

    [Fact]
    public async Task Handle_AplicaFiltrosOperativosAntesDeConciliar()
    {
        var pais = BuildPais(20, "PY", "Paraguay");
        var sucursal = BuildSucursalParaguay(1, 20);
        var elegible = Comprobante.Crear(1, 1, 1, 1, 111, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var otroCodigo = Comprobante.Crear(1, 1, 1, 1, 112, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var pendiente = Comprobante.Crear(1, 1, 1, 1, 113, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var fueraFecha = Comprobante.Crear(1, 1, 1, 1, 114, new DateOnly(2026, 4, 20), null, 1, 1, 1m, null, null);

        SetProperty(elegible, nameof(Comprobante.Id), 21L);
        SetProperty(otroCodigo, nameof(Comprobante.Id), 22L);
        SetProperty(pendiente, nameof(Comprobante.Id), 23L);
        SetProperty(fueraFecha, nameof(Comprobante.Id), 24L);
        SetProperty(elegible, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(otroCodigo, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(pendiente, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(fueraFecha, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        elegible.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo", "track-21", null, null, DateTimeOffset.UtcNow, null);
        otroCodigo.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "151", "otro rechazo", "track-22", null, null, DateTimeOffset.UtcNow, null);
        pendiente.RegistrarResultadoSifen(EstadoSifenParaguay.Pendiente, null, null, "track-23", null, null, DateTimeOffset.UtcNow, null);
        fueraFecha.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "fuera fecha", "track-24", null, null, DateTimeOffset.UtcNow, null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { elegible, otroCodigo, pendiente, fueraFecha });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        _repo.GetByIdConItemsAsync(21, Arg.Any<CancellationToken>()).Returns(elegible);
        _sifen.ConciliarEstadoAsync(elegible, Arg.Any<CancellationToken>())
            .Returns(Result.Success(new ResultadoEnvioSifenParaguayDto { ComprobanteId = 21, Estado = "approved", Aceptado = true, TrackingId = "track-21" }));

        var result = await Sut().Handle(
            new ConciliarSifenParaguayPendientesCommand(
                MaxItems: 10,
                SucursalId: 1,
                EstadoSifen: nameof(EstadoSifenParaguay.Rechazado),
                CodigoRespuesta: "150",
                PuedeReintentar: true,
                FechaDesde: new DateOnly(2026, 4, 1),
                FechaHasta: new DateOnly(2026, 4, 15)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Encontrados.Should().Be(1);
        result.Value.TotalElegibles.Should().Be(1);
        result.Value.HayMasResultados.Should().BeFalse();
        result.Value.Procesados.Should().Be(1);
        result.Value.Items.Should().ContainSingle(x => x.ComprobanteId == 21 && x.Exitoso);
        result.Value.Estados.Should().ContainSingle(x => x.Estado == "approved" && x.Cantidad == 1);
        result.Value.Errores.Should().BeEmpty();
        await _repo.Received(1).GetByIdConItemsAsync(21, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByIdConItemsAsync(22, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByIdConItemsAsync(23, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByIdConItemsAsync(24, Arg.Any<CancellationToken>());
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = (Pais)Activator.CreateInstance(typeof(Pais), nonPublic: true)!;
        SetProperty(pais, nameof(Pais.Id), id);
        SetProperty(pais, nameof(Pais.Codigo), codigo);
        SetProperty(pais, nameof(Pais.Descripcion), descripcion);
        return pais;
    }

    private static Sucursal BuildSucursalParaguay(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal PY", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class PreviewConciliarSifenParaguayPendientesQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private PreviewConciliarSifenParaguayPendientesQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_RetornaSoloConciliablesConIdentificadoresYMismoOrden()
    {
        var pais = BuildPais(20, "PY", "Paraguay");
        var sucursal = BuildSucursalParaguay(1, 20);
        var primero = Comprobante.Crear(1, 1, 1, 1, 141, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var segundo = Comprobante.Crear(1, 1, 1, 1, 142, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var sinIdentificadores = Comprobante.Crear(1, 1, 1, 1, 143, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);
        var aceptado = Comprobante.Crear(1, 1, 1, 1, 144, new DateOnly(2026, 4, 10), null, 1, 1, 1m, null, null);

        SetProperty(primero, nameof(Comprobante.Id), 51L);
        SetProperty(segundo, nameof(Comprobante.Id), 52L);
        SetProperty(sinIdentificadores, nameof(Comprobante.Id), 53L);
        SetProperty(aceptado, nameof(Comprobante.Id), 54L);
        SetProperty(primero, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(segundo, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(sinIdentificadores, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(aceptado, nameof(Comprobante.Estado), EstadoComprobante.Emitido);

        primero.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo 1", "track-51", null, null, new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero), null);
        segundo.RegistrarResultadoSifen(EstadoSifenParaguay.Error, "E001", "timeout", "track-52", null, null, new DateTimeOffset(2026, 4, 10, 9, 0, 0, TimeSpan.Zero), null);
        sinIdentificadores.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "sin ids", null, null, null, new DateTimeOffset(2026, 4, 10, 8, 0, 0, TimeSpan.Zero), null);
        aceptado.RegistrarResultadoSifen(EstadoSifenParaguay.Aceptado, null, null, "track-54", null, null, new DateTimeOffset(2026, 4, 10, 7, 0, 0, TimeSpan.Zero), null);

        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[] { primero, segundo, sinIdentificadores, aceptado });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { sucursal });
        var paises = MockDbSetHelper.CreateMockDbSet(new[] { pais });

        _db.Comprobantes.Returns(comprobantes);
        _db.Sucursales.Returns(sucursales);
        _db.Paises.Returns(paises);

        var result = await Sut().Handle(
            new PreviewConciliarSifenParaguayPendientesQuery(
                MaxItems: 2,
                SucursalId: 1,
                PuedeReintentar: true,
                FechaDesde: new DateOnly(2026, 4, 1),
                FechaHasta: new DateOnly(2026, 4, 15)),
            CancellationToken.None);

        result.MaxItems.Should().Be(2);
        result.Encontrados.Should().Be(2);
        result.TotalElegibles.Should().Be(2);
        result.HayMasResultados.Should().BeFalse();
        result.Items.Select(x => x.ComprobanteId).Should().Equal(52, 51);
        result.Items.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.TrackingId));
        result.Items.Should().OnlyContain(x => x.PuedeReintentar);
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == EstadoSifenParaguay.Error && x.Cantidad == 1);
        result.Estados.Should().ContainSingle(x => x.EstadoSifen == EstadoSifenParaguay.Rechazado && x.Cantidad == 1);
        result.CodigosRespuesta.Should().ContainSingle(x => x.CodigoRespuesta == "150" && x.Cantidad == 1);
        result.CodigosRespuesta.Should().ContainSingle(x => x.CodigoRespuesta == "E001" && x.Cantidad == 1);
        result.MensajesRespuesta.Should().ContainSingle(x => x.MensajeRespuesta == "rechazo 1" && x.Cantidad == 1);
        result.MensajesRespuesta.Should().ContainSingle(x => x.MensajeRespuesta == "timeout" && x.Cantidad == 1);
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var pais = (Pais)Activator.CreateInstance(typeof(Pais), nonPublic: true)!;
        SetProperty(pais, nameof(Pais.Id), id);
        SetProperty(pais, nameof(Pais.Codigo), codigo);
        SetProperty(pais, nameof(Pais.Descripcion), descripcion);
        return pais;
    }

    private static Sucursal BuildSucursalParaguay(long id, long paisId)
    {
        var sucursal = Sucursal.Crear("Sucursal PY", "80012345-6", 1, 1, paisId, true, null);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        return sucursal;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

// ── GetComprobantesPagedQueryHandler ─────────────────────────────────────────

public class GetComprobantesPagedQueryHandlerTests
{
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetComprobantesPagedQueryHandler Sut() => new(_repo, _db);

    [Fact]
    public async Task Handle_ConResultadoVacio_RetornaPagedResultVacio()
    {
        _repo.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<long?>(), Arg.Any<long?>(),
                Arg.Any<long?>(), Arg.Any<EstadoComprobante?>(),
                Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Comprobante>([], 1, 20, 0));

        var mockTerceros5 = MockDbSetHelper.CreateMockDbSet<Tercero>();

        _db.Terceros.Returns(mockTerceros5);
        var mockTiposComprobante6 = MockDbSetHelper.CreateMockDbSet<TipoComprobante>();
        _db.TiposComprobante.Returns(mockTiposComprobante6);
        var mockMonedas7 = MockDbSetHelper.CreateMockDbSet<Moneda>();
        _db.Monedas.Returns(mockMonedas7);

        var query = new GetComprobantesPagedQuery(1, 20, null, null, null, null, null, null);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}

// ── GetSaldoPendienteTerceroQueryHandler ─────────────────────────────────────

public class GetSaldoPendienteTerceroQueryHandlerTests
{
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetSaldoPendienteTerceroQueryHandler Sut() => new(_repo, _db);

    [Fact]
    public async Task Handle_SinComprobantes_RetornaListaVacia()
    {
        _repo.GetSaldoPendienteByTerceroAsync(Arg.Any<long>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(new List<Comprobante>().AsReadOnly() as IReadOnlyList<Comprobante>);
        var mockTiposComprobante8 = MockDbSetHelper.CreateMockDbSet<TipoComprobante>();
        _db.TiposComprobante.Returns(mockTiposComprobante8);

        var result = await Sut().Handle(new GetSaldoPendienteTerceroQuery(1, null), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetComprobanteDetalleQueryHandler ────────────────────────────────────────

public class GetComprobanteDetalleQueryHandlerTests
{
    private readonly IComprobanteRepository _repo = Substitute.For<IComprobanteRepository>();
    private readonly IImputacionRepository _imputRepo = Substitute.For<IImputacionRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetComprobanteDetalleQueryHandler Sut() => new(_repo, _imputRepo, _db);

    [Fact]
    public async Task Handle_ComprobanteNoEncontrado_RetornaNull()
    {
        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Comprobante?)null);

        var result = await Sut().Handle(new GetComprobanteDetalleQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComprobanteEncontrado_RetornaDetalle()
    {
        var comp = Comprobante.Crear(1, null, 1, 1, 1,
            DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, null, null);
        SetProperty(comp, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        comp.AsignarTimbrado(44, "80004444", null);
        comp.RegistrarResultadoSifen(EstadoSifenParaguay.Rechazado, "150", "rechazo detalle", "track-det", null, null, new DateTimeOffset(2026, 3, 20, 20, 0, 0, TimeSpan.Zero), null);
        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(comp);

        _imputRepo.GetByComprobanteDestinoAsync(Arg.Any<long>(), false, Arg.Any<CancellationToken>())
            .Returns(new List<Imputacion>().AsReadOnly() as IReadOnlyList<Imputacion>);

        var mockTerceros9 = MockDbSetHelper.CreateMockDbSet<Tercero>();

        _db.Terceros.Returns(mockTerceros9);
        var mockTiposComprobante10 = MockDbSetHelper.CreateMockDbSet<TipoComprobante>();
        _db.TiposComprobante.Returns(mockTiposComprobante10);
        var mockMonedas11 = MockDbSetHelper.CreateMockDbSet<Moneda>();
        _db.Monedas.Returns(mockMonedas11);
        var mockSucursales12 = MockDbSetHelper.CreateMockDbSet<Sucursal>();
        _db.Sucursales.Returns(mockSucursales12);
        var mockCondicionesIva13 = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.CondicionIva>();
        _db.CondicionesIva.Returns(mockCondicionesIva13);
        var mockItems14 = MockDbSetHelper.CreateMockDbSet<Item>();
        _db.Items.Returns(mockItems14);
        var mockDepositos15 = MockDbSetHelper.CreateMockDbSet<Deposito>();
        _db.Depositos.Returns(mockDepositos15);
        var mockComprobantes16 = MockDbSetHelper.CreateMockDbSet<Comprobante>();
        _db.Comprobantes.Returns(mockComprobantes16);

        var result = await Sut().Handle(new GetComprobanteDetalleQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TimbradoId.Should().Be(44);
        result.NroTimbrado.Should().Be("80004444");
        result.EstadoSifen.Should().Be(EstadoSifenParaguay.Rechazado);
        result.SifenCodigoRespuesta.Should().Be("150");
        result.SifenMensajeRespuesta.Should().Be("rechazo detalle");
        result.SifenTrackingId.Should().Be("track-det");
        result.TieneIdentificadoresSifen.Should().BeTrue();
        result.PuedeReintentarSifen.Should().BeTrue();
        result.PuedeConciliarSifen.Should().BeTrue();
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

// ── ConvertirPresupuestoCommandHandler ───────────────────────────────────────

public class ConvertirPresupuestoCommandHandlerTests
{
    private readonly IComprobanteRepository _comprobanteRepo = Substitute.For<IComprobanteRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private ConvertirPresupuestoCommandHandler Sut() => new(_comprobanteRepo, _db, _uow, _user, new TerceroOperacionValidationService(_db));

    [Fact]
    public async Task Handle_PresupuestoNoEncontrado_RetornaFailure()
    {
        var mockComprobantes17 = MockDbSetHelper.CreateMockDbSet<Comprobante>();
        _db.Comprobantes.Returns(mockComprobantes17);

        var cmd = new ConvertirPresupuestoCommand(99, 1, null,
            DateOnly.FromDateTime(DateTime.Today), null, null);
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
