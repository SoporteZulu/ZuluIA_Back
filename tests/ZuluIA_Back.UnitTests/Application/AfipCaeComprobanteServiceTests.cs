using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Services;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class AfipCaeComprobanteServiceTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IAfipWsfeCaeService _afipWsfeCaeService = Substitute.For<IAfipWsfeCaeService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly DbSet<AuditoriaComprobante> _auditoria = MockDbSetHelper.CreateMockDbSet<AuditoriaComprobante>();

    private AfipCaeComprobanteService Sut() => new(_db, _afipWsfeCaeService, _currentUser);

    public AfipCaeComprobanteServiceTests()
    {
        _currentUser.UserId.Returns((long?)7L);
        _db.AuditoriaComprobantes.Returns(_auditoria);
    }

    [Fact]
    public async Task SolicitarYAsignarAsync_ComprobantePersistido_RegistraSolicitudYExito()
    {
        var comprobante = BuildComprobante(id: 10);
        ConfigureCatalogs(comprobante);
        _afipWsfeCaeService.SolicitarCaeAsync(Arg.Any<SolicitarCaeAfipRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SolicitarCaeAfipResponse("12345678901234", new DateOnly(2026, 4, 30)));

        var result = await Sut().SolicitarYAsignarAsync(comprobante, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comprobante.Cae.Should().Be("12345678901234");
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x =>
            x.ComprobanteId == 10 &&
            x.Accion == AccionAuditoria.AfipSolicitud &&
            x.DetalleCambio != null &&
            x.DetalleCambio.Contains("request=")));
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x =>
            x.ComprobanteId == 10 &&
            x.Accion == AccionAuditoria.CaeAsignado &&
            x.DetalleCambio != null &&
            x.DetalleCambio.Contains("response=")));
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SolicitarYAsignarAsync_ComprobantePersistidoConError_RegistraSolicitudYError()
    {
        var comprobante = BuildComprobante(id: 10);
        ConfigureCatalogs(comprobante);
        _afipWsfeCaeService.SolicitarCaeAsync(Arg.Any<SolicitarCaeAfipRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("AFIP rechazo la solicitud"));

        var result = await Sut().SolicitarYAsignarAsync(comprobante, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("AFIP rechazo la solicitud");
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x =>
            x.ComprobanteId == 10 && x.Accion == AccionAuditoria.AfipSolicitud));
        _auditoria.Received(1).Add(Arg.Is<AuditoriaComprobante>(x =>
            x.ComprobanteId == 10 &&
            x.Accion == AccionAuditoria.AfipError &&
            x.DetalleCambio != null &&
            x.DetalleCambio.Contains("AFIP rechazo la solicitud")));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SolicitarYAsignarAsync_ConPercepciones_UsaTributoConfiguradoEnRequestAfip()
    {
        var comprobante = BuildComprobante(id: 10, percepciones: 35m);
        ConfigureCatalogs(comprobante);
        SolicitarCaeAfipRequest? requestCapturado = null;

        var impuestos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuesto(10, "99", "Percepciones", 3.5m, "percepcion")
        });
        var asignaciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuestoPorTipoComprobante(10, comprobante.TipoComprobanteId, 1)
        });

        _db.Impuestos.Returns(impuestos);
        _db.ImpuestosPorTipoComprobante.Returns(asignaciones);
        _afipWsfeCaeService.SolicitarCaeAsync(Arg.Do<SolicitarCaeAfipRequest>(x => requestCapturado = x), Arg.Any<CancellationToken>())
            .Returns(new SolicitarCaeAfipResponse("12345678901234", new DateOnly(2026, 4, 30)));

        var result = await Sut().SolicitarYAsignarAsync(comprobante, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        requestCapturado.Should().NotBeNull();
        requestCapturado!.ImporteTributos.Should().Be(35m);
        requestCapturado.Tributos.Should().ContainSingle();
        requestCapturado.Tributos.First().Id.Should().Be(99);
        requestCapturado.Tributos.First().Descripcion.Should().Be("Percepciones");
        requestCapturado.Tributos.First().Alicuota.Should().Be(3.5m);
        requestCapturado.Tributos.First().Importe.Should().Be(35m);
    }

    private void ConfigureCatalogs(Comprobante comprobante)
    {
        var tiposComprobante = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipoComprobante(comprobante.TipoComprobanteId, true, 1)
        });
        var puntosFacturacion = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildPuntoFacturacion(comprobante.PuntoFacturacionId!.Value, comprobante.SucursalId, comprobante.Numero.Prefijo)
        });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildSucursal(comprobante.SucursalId, "20123456789")
        });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTercero(comprobante.TerceroId, 1, "30111222")
        });
        var tiposDocumento = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipoDocumento(1, 80)
        });
        var monedas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMoneda(comprobante.MonedaId, "PES")
        });
        var alicuotas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildAlicuota(1, 5, 21)
        });
        var impuestos = MockDbSetHelper.CreateMockDbSet<Impuesto>();
        var asignaciones = MockDbSetHelper.CreateMockDbSet<ImpuestoPorTipoComprobante>();

        _db.TiposComprobante.Returns(tiposComprobante);
        _db.PuntosFacturacion.Returns(puntosFacturacion);
        _db.Sucursales.Returns(sucursales);
        _db.Terceros.Returns(terceros);
        _db.TiposDocumento.Returns(tiposDocumento);
        _db.Monedas.Returns(monedas);
        _db.AlicuotasIva.Returns(alicuotas);
        _db.Impuestos.Returns(impuestos);
        _db.ImpuestosPorTipoComprobante.Returns(asignaciones);
    }

    private static Comprobante BuildComprobante(long id, decimal percepciones = 0m)
    {
        var comprobante = Comprobante.Crear(1, 1, 1, 1, 25, new DateOnly(2026, 4, 1), null, 1, 1, 1m, null, 7L);
        SetProperty(comprobante, nameof(Comprobante.Id), id);
        comprobante.AgregarItem(ComprobanteItem.Crear(0, 1, "Prod", 1m, 0, 1000, 0m, 1, 21, null, 1));
        if (percepciones > 0)
            comprobante.SetPercepciones(percepciones, 7L);
        comprobante.Emitir(7L);
        return comprobante;
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

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}