using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Collections;
using System.Reflection;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class AutorizacionesComprobantesControllerTests
{
    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var autorizaciones = MockDbSetHelper.CreateMockDbSet<AutorizacionComprobante>();
        db.AutorizacionesComprobantes.Returns(autorizaciones);
        var controller = CreateAutorizacionesController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItems()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var entity = CrearAutorizacion(25);
        var autorizaciones = MockDbSetHelper.CreateMockDbSet(new[] { entity });
        db.AutorizacionesComprobantes.Returns(autorizaciones);
        var controller = CreateAutorizacionesController(mediator, db);

        var result = await controller.GetAll(null, null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().ContainSingle();
        var first = items[0];
        first.GetType().GetProperty("TipoOperacion")!.GetValue(first).Should().Be("VENTA");
        first.GetType().GetProperty("Estado")!.GetValue(first).Should().Be(nameof(EstadoAutorizacion.Pendiente));
    }

    [Fact]
    public async Task Create_CuandoCommandFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateAutorizacionesController(mediator);
        mediator.Send(Arg.Any<CreateAutorizacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("error create"));

        var result = await controller.Create(new CrearAutorizacionComprobanteRequest(10, 2, "VENTA"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("error create");
    }

    [Fact]
    public async Task Create_CuandoCommandTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateAutorizacionesController(mediator);
        mediator.Send(Arg.Any<CreateAutorizacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(25L));

        var result = await controller.Create(new CrearAutorizacionComprobanteRequest(10, 2, "VENTA"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(AutorizacionesComprobantesController.GetById));
    }

    [Fact]
    public async Task Autorizar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateAutorizacionesController(mediator);
        mediator.Send(Arg.Any<AutorizarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Autorización 5 no encontrada."));

        var result = await controller.Autorizar(5, new ResolverAutorizacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Rechazar_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateAutorizacionesController(mediator);
        mediator.Send(Arg.Any<RechazarAutorizacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Solo se pueden rechazar solicitudes pendientes."));

        var result = await controller.Rechazar(5, new ResolverAutorizacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("pendientes");
    }

    private static AutorizacionesComprobantesController CreateAutorizacionesController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new AutorizacionesComprobantesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    private static AutorizacionComprobante CrearAutorizacion(long id)
    {
        var entity = AutorizacionComprobante.Crear(10, 2, "VENTA", 1);
        ComprobantesWorkflowControllerTestHelper.SetProperty(entity, nameof(AutorizacionComprobante.Id), id);
        return entity;
    }
}

public class HabilitacionesComprobantesControllerTests
{
    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var habilitaciones = MockDbSetHelper.CreateMockDbSet<HabilitacionComprobante>();
        db.HabilitacionesComprobantes.Returns(habilitaciones);
        var controller = CreateHabilitacionesController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItems()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var entity = CrearHabilitacion(30);
        var habilitaciones = MockDbSetHelper.CreateMockDbSet(new[] { entity });
        db.HabilitacionesComprobantes.Returns(habilitaciones);
        var controller = CreateHabilitacionesController(mediator, db);

        var result = await controller.GetAll(null, null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().ContainSingle();
        var first = items[0];
        first.GetType().GetProperty("TipoDocumento")!.GetValue(first).Should().Be("COMPROBANTE");
        first.GetType().GetProperty("Estado")!.GetValue(first).Should().Be(nameof(EstadoHabilitacion.Pendiente));
    }

    [Fact]
    public async Task Create_CuandoCommandFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateHabilitacionesController(mediator);
        mediator.Send(Arg.Any<CreateHabilitacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("error create"));

        var result = await controller.Create(new CrearHabilitacionComprobanteRequest(10, 2, "COMPROBANTE", "bloqueo"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("error create");
    }

    [Fact]
    public async Task Habilitar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateHabilitacionesController(mediator);
        mediator.Send(Arg.Any<HabilitarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Habilitación 5 no encontrada."));

        var result = await controller.Habilitar(5, new ResolverHabilitacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Rechazar_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateHabilitacionesController(mediator);
        mediator.Send(Arg.Any<RechazarHabilitacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Solo se pueden rechazar solicitudes pendientes."));

        var result = await controller.Rechazar(5, new ResolverHabilitacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("pendientes");
    }

    private static HabilitacionesComprobantesController CreateHabilitacionesController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new HabilitacionesComprobantesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    private static HabilitacionComprobante CrearHabilitacion(long id)
    {
        var entity = HabilitacionComprobante.Crear(10, 2, "COMPROBANTE", "bloqueo", 1);
        ComprobantesWorkflowControllerTestHelper.SetProperty(entity, nameof(HabilitacionComprobante.Id), id);
        return entity;
    }
}

internal static class ComprobantesWorkflowControllerTestHelper
{
    public static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }
}