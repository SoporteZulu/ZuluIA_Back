using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Pagos.Commands;
using ZuluIA_Back.Application.Features.Pagos.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PagosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConEnriquecimiento()
    {
        var repo = Substitute.For<IPagoRepository>();
        repo.GetPagedAsync(2, 15, 3, 10, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Pago>(
            [
                BuildPago(1, 3, 10, new DateOnly(2026, 3, 21), 1, 150m, EstadoPago.Activo),
                BuildPago(2, 3, 11, new DateOnly(2026, 3, 20), 2, 200m, EstadoPago.Anulado)
            ],
            2,
            15,
            31));

        var db = BuildDb(
            terceros: [BuildTercero(10, "PRO001", "Proveedor Uno")],
            monedas: [BuildMoneda(1, "$"), BuildMoneda(2, "Gs")]);
        var controller = CreateController(repo: repo, db: db);

        var result = await controller.GetAll(2, 15, 3, 10, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!.GetType().GetProperty("data")!.GetValue(ok.Value)!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "TerceroRazonSocial", "Proveedor Uno");
        AssertAnonymousProperty(data[0], "MonedaSimbolo", "$");
        AssertAnonymousProperty(data[0], "Estado", "ACTIVO");
        AssertAnonymousProperty(data[1], "TerceroRazonSocial", "—");
        AssertAnonymousProperty(data[1], "MonedaSimbolo", "Gs");
        AssertAnonymousProperty(ok.Value!, "page", 2);
        AssertAnonymousProperty(ok.Value!, "pageSize", 15);
        AssertAnonymousProperty(ok.Value!, "totalCount", 31);
        AssertAnonymousProperty(ok.Value!, "totalPages", 3);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var repo = Substitute.For<IPagoRepository>();
        repo.GetByIdConMediosAsync(99, Arg.Any<CancellationToken>())
            .Returns((Pago?)null);
        var controller = CreateController(repo: repo);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el pago con ID 99");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalleEnriquecido()
    {
        var pago = BuildPago(7, 3, 10, new DateOnly(2026, 3, 21), 1, 150m, EstadoPago.Activo, "Pago de prueba");
        pago.AgregarMedio(BuildPagoMedio(1, 7, 5, 2, 150m, 1, 1m));

        var repo = Substitute.For<IPagoRepository>();
        repo.GetByIdConMediosAsync(7, Arg.Any<CancellationToken>())
            .Returns(pago);

        var db = BuildDb(
            terceros: [BuildTercero(10, "PRO001", "Proveedor Uno")],
            monedas: [BuildMoneda(1, "$")],
            cajas: [BuildCaja(5, "Caja Principal")],
            formasPago: [BuildFormaPago(2, "Efectivo")],
            retenciones: [BuildRetencion(1, 7, "IIBB", 12m, "CERT-1", new DateOnly(2026, 3, 21))]);

        var controller = CreateController(repo: repo, db: db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<ZuluIA_Back.Application.Features.Finanzas.DTOs.PagoDto>().Subject;
        dto.Id.Should().Be(7);
        dto.TerceroRazonSocial.Should().Be("Proveedor Uno");
        dto.MonedaSimbolo.Should().Be("$");
        dto.Medios.Should().ContainSingle();
        dto.Medios[0].CajaDescripcion.Should().Be("Caja Principal");
        dto.Medios[0].FormaPagoDescripcion.Should().Be("Efectivo");
        dto.Retenciones.Should().ContainSingle();
        dto.Retenciones[0].Tipo.Should().Be("IIBB");
    }

    [Fact]
    public async Task Registrar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Debe informar al menos un medio de pago."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Registrar(BuildRegistrarPagoCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("al menos un medio de pago");
    }

    [Fact]
    public async Task Registrar_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Registrar(BuildRegistrarPagoCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetPagoById");
        AssertAnonymousProperty(created.Value!, "id", 21L);
    }

    [Fact]
    public async Task RegistrarBasico_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La fecha es requerida."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.RegistrarBasico(BuildCreatePagoCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("fecha es requerida");
    }

    [Fact]
    public async Task RegistrarBasico_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(31L));
        var controller = CreateController(mediator: mediator);

        var result = await controller.RegistrarBasico(BuildCreatePagoCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetPagoById");
        AssertAnonymousProperty(created.Value!, "id", 31L);
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El pago ya está anulado."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está anulado");
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static PagosController CreateController(IMediator? mediator = null, IPagoRepository? repo = null, IApplicationDbContext? db = null)
    {
        var controller = new PagosController(
            mediator ?? Substitute.For<IMediator>(),
            repo ?? Substitute.For<IPagoRepository>(),
            db ?? BuildDb())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<Tercero>? terceros = null,
        IEnumerable<Moneda>? monedas = null,
        IEnumerable<CajaCuentaBancaria>? cajas = null,
        IEnumerable<FormaPago>? formasPago = null,
        IEnumerable<Retencion>? retenciones = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var tercerosDbSet = MockDbSetHelper.CreateMockDbSet(terceros);
        var monedasDbSet = MockDbSetHelper.CreateMockDbSet(monedas);
        var cajasDbSet = MockDbSetHelper.CreateMockDbSet(cajas);
        var formasPagoDbSet = MockDbSetHelper.CreateMockDbSet(formasPago);
        var retencionesDbSet = MockDbSetHelper.CreateMockDbSet(retenciones);

        db.Terceros.Returns(tercerosDbSet);
        db.Monedas.Returns(monedasDbSet);
        db.CajasCuentasBancarias.Returns(cajasDbSet);
        db.FormasPago.Returns(formasPagoDbSet);
        db.Retenciones.Returns(retencionesDbSet);
        return db;
    }

    private static Pago BuildPago(long id, long sucursalId, long terceroId, DateOnly fecha, long monedaId, decimal total, EstadoPago estado, string? observacion = null)
    {
        var entity = Pago.Crear(sucursalId, terceroId, fecha, monedaId, 1m, observacion, 1);
        SetProperty(entity, nameof(Pago.Id), id);
        SetProperty(entity, nameof(Pago.Total), total);
        SetProperty(entity, nameof(Pago.Estado), estado);
        SetProperty(entity, nameof(Pago.CreatedAt), new DateTimeOffset(2026, 3, 21, 12, 0, 0, TimeSpan.Zero));
        return entity;
    }

    private static PagoMedio BuildPagoMedio(long id, long pagoId, long cajaId, long formaPagoId, decimal importe, long monedaId, decimal cotizacion)
    {
        var entity = PagoMedio.Crear(pagoId, cajaId, formaPagoId, null, importe, monedaId, cotizacion);
        SetProperty(entity, nameof(PagoMedio.Id), id);
        return entity;
    }

    private static Retencion BuildRetencion(long id, long pagoId, string tipo, decimal importe, string certificado, DateOnly fecha)
    {
        var entity = Retencion.CrearEnPago(pagoId, tipo, importe, certificado, fecha);
        SetProperty(entity, nameof(Retencion.Id), id);
        return entity;
    }

    private static Tercero BuildTercero(long id, string legajo, string razonSocial)
    {
        var entity = Tercero.Crear(legajo, razonSocial, 1, "123", 1, false, true, false, 1, 1);
        SetProperty(entity, nameof(Tercero.Id), id);
        return entity;
    }

    private static Moneda BuildMoneda(long id, string simbolo)
    {
        var entity = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        SetProperty(entity, nameof(Moneda.Id), id);
        SetProperty(entity, nameof(Moneda.Simbolo), simbolo);
        return entity;
    }

    private static CajaCuentaBancaria BuildCaja(long id, string descripcion)
    {
        var entity = CajaCuentaBancaria.Crear(1, 1, descripcion, 1, true, null, 1);
        SetProperty(entity, nameof(CajaCuentaBancaria.Id), id);
        return entity;
    }

    private static FormaPago BuildFormaPago(long id, string descripcion)
    {
        var entity = (FormaPago)Activator.CreateInstance(typeof(FormaPago), nonPublic: true)!;
        SetProperty(entity, nameof(FormaPago.Id), id);
        SetProperty(entity, nameof(FormaPago.Descripcion), descripcion);
        return entity;
    }

    private static RegistrarPagoCommand BuildRegistrarPagoCommand()
    {
        return new RegistrarPagoCommand(
            3,
            10,
            new DateOnly(2026, 3, 21),
            1,
            1m,
            "Pago de prueba",
            [new MedioPagoInput(5, 2, null, 150m, 1, 1m)],
            [new RetencionInput("IIBB", 12m, "CERT-1")],
            [new ComprobanteAImputarInput(40, 138m)]);
    }

    private static CreatePagoCommand BuildCreatePagoCommand()
    {
        return new CreatePagoCommand(
            3,
            10,
            new DateOnly(2026, 3, 21),
            1,
            1m,
            "Pago básico",
            [new CreatePagoMedioDto(5, 2, 150m, 1, 1m)]);
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}