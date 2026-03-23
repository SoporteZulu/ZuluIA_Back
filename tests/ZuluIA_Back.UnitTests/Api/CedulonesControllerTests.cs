using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CedulonesControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginadoEnriquecido()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var cedulon1 = BuildCedulon(1, 10, 3, 5, "CED-001", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 10), 150m, 50m, EstadoCedulon.PagadoParcial);
        var cedulon2 = BuildCedulon(2, 20, 3, null, "CED-002", new DateOnly(2026, 3, 5), new DateOnly(2026, 4, 10), 200m, 0m, EstadoCedulon.Pendiente);
        var paged = new PagedResult<Cedulon>([cedulon1, cedulon2], 2, 15, 31);
        var terceros = MockDbSetHelper.CreateMockDbSet([
            BuildTercero(10, "Cliente Uno")
        ]);
        repo.GetPagedAsync(2, 15, 10, 3, EstadoCedulon.Pendiente, Arg.Any<CancellationToken>())
            .Returns(paged);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetAll(2, 15, 10, 3, "Pendiente", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "page", 2);
        AssertAnonymousProperty(ok.Value!, "pageSize", 15);
        AssertAnonymousProperty(ok.Value!, "totalCount", 31);
        var data = ((IEnumerable<object>)ok.Value!.GetType().GetProperty("data")!.GetValue(ok.Value)!).ToList();
        data.Should().HaveCount(2);
        data[0].Should().BeOfType<CedulonDto>();
        var dto1 = (CedulonDto)data[0];
        dto1.TerceroRazonSocial.Should().Be("Cliente Uno");
        dto1.SaldoPendiente.Should().Be(100m);
        dto1.Estado.Should().Be("PAGADOPARCIAL");
        dto1.Vencido.Should().BeTrue();
        var dto2 = (CedulonDto)data[1];
        dto2.TerceroRazonSocial.Should().Be("—");
        dto2.Vencido.Should().BeFalse();
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Cedulon?)null);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("cedulón con ID 99");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalleEnriquecido()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var cedulon = BuildCedulon(5, 10, 3, 7, "CED-005", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 10), 150m, 150m, EstadoCedulon.Pagado);
        var terceros = MockDbSetHelper.CreateMockDbSet([
            BuildTercero(10, "Cliente Uno")
        ]);
        repo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(cedulon);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<CedulonDto>().Subject;
        dto.Id.Should().Be(5);
        dto.TerceroRazonSocial.Should().Be("Cliente Uno");
        dto.PlanPagoId.Should().Be(7);
        dto.SaldoPendiente.Should().Be(0m);
        dto.Estado.Should().Be("PAGADO");
        dto.Vencido.Should().BeFalse();
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCedulonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El importe del cedulón debe ser mayor a 0."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Create(new CreateCedulonRequest(10, 3, null, "CED-001", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 10), 0m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCedulonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Create(new CreateCedulonRequest(10, 3, null, "CED-001", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 10), 100m), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCedulonById");
        AssertAnonymousProperty(created.Value!, "id", 21L);
    }

    [Fact]
    public async Task Pagar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<PagarCedulonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PagarCedulonResult>("No se encontro el cedulón."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Pagar(7, new PagarCedulonRequest(50m), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Pagar_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<PagarCedulonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PagarCedulonResult>("El importe pagado supera el total del cedulón."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Pagar(7, new PagarCedulonRequest(500m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Pagar_CuandoTieneExito_DevuelveOkConResumen()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<PagarCedulonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new PagarCedulonResult(100m, 50m, "PAGADO_PARCIAL")));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Pagar(7, new PagarCedulonRequest(100m), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "importePagado", 100m);
        AssertAnonymousProperty(ok.Value!, "saldoPendiente", 50m);
        AssertAnonymousProperty(ok.Value!, "estado", "PAGADO_PARCIAL");
    }

    [Fact]
    public async Task Vencer_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<VencerCedulonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>("No se encontro el cedulón."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Vencer(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Vencer_CuandoTieneExito_DevuelveOkConEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<VencerCedulonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success("VENCIDO"));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Vencer(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "estado", "VENCIDO");
    }

    [Fact]
    public async Task GetVencidos_CuandoHayDatos_DevuelveResumenConTotales()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ICedulonRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var cedulones = MockDbSetHelper.CreateMockDbSet([
            BuildCedulon(1, 10, 3, null, "CED-001", new DateOnly(2026, 2, 1), new DateOnly(2026, 3, 10), 150m, 50m, EstadoCedulon.PagadoParcial),
            BuildCedulon(2, 20, 3, null, "CED-002", new DateOnly(2026, 2, 5), new DateOnly(2026, 3, 15), 200m, 0m, EstadoCedulon.Pendiente),
            BuildCedulon(3, 30, 4, null, "CED-003", new DateOnly(2026, 2, 10), new DateOnly(2026, 4, 10), 300m, 0m, EstadoCedulon.Pendiente),
            BuildCedulon(4, 40, 3, null, "CED-004", new DateOnly(2026, 2, 1), new DateOnly(2026, 3, 5), 100m, 100m, EstadoCedulon.Pagado)
        ]);
        db.Cedulones.Returns(cedulones);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetVencidos(3, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "totalVencidos", 2);
        AssertAnonymousProperty(ok.Value!, "totalImporte", 300m);
        var items = ((IEnumerable<object>)ok.Value!.GetType().GetProperty("cedulones")!.GetValue(ok.Value)!).ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "SaldoPendiente", 100m);
        AssertAnonymousProperty(items[1], "Id", 2L);
        AssertAnonymousProperty(items[1], "SaldoPendiente", 200m);
    }

    private static CedulonesController CreateController(IMediator mediator, ICedulonRepository repo, IApplicationDbContext db)
    {
        var controller = new CedulonesController(mediator, repo, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Cedulon BuildCedulon(
        long id,
        long terceroId,
        long sucursalId,
        long? planPagoId,
        string nroCedulon,
        DateOnly fechaEmision,
        DateOnly fechaVencimiento,
        decimal importe,
        decimal importePagado,
        EstadoCedulon estado)
    {
        var entity = Cedulon.Crear(terceroId, sucursalId, planPagoId, nroCedulon, fechaEmision, fechaVencimiento, importe, userId: null);
        SetProperty(entity, nameof(Cedulon.Id), id);
        SetProperty(entity, nameof(Cedulon.ImportePagado), importePagado);
        SetProperty(entity, nameof(Cedulon.Estado), estado);
        return entity;
    }

    private static Tercero BuildTercero(long id, string razonSocial)
    {
        var entity = Tercero.Crear("LEG" + id, razonSocial, 1, id.ToString(), 1, true, false, false, null, null);
        SetProperty(entity, nameof(Tercero.Id), id);
        return entity;
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