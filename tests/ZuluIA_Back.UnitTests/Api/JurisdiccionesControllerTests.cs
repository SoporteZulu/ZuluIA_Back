using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Referencia.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class JurisdiccionesControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var jurisdicciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildJurisdiccion(2, "Santa Fe", true),
            BuildJurisdiccion(1, "Buenos Aires", true)
        });
        db.Jurisdicciones.Returns(jurisdicciones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Buenos Aires");
        AssertAnonymousProperty(items[1], "Descripcion", "Santa Fe");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var jurisdicciones = MockDbSetHelper.CreateMockDbSet<Jurisdiccion>();
        db.Jurisdicciones.Returns(jurisdicciones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Jurisdicción 999 no encontrada");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var jurisdicciones = MockDbSetHelper.CreateMockDbSet(new[] { BuildJurisdiccion(5, "Buenos Aires", true) });
        db.Jurisdicciones.Returns(jurisdicciones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Buenos Aires");
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Descripcion requerida."));

        var result = await controller.Create(new JurisdiccionRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExitoYNoSeRelee_DevuelveCreatedAtRouteConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var jurisdicciones = MockDbSetHelper.CreateMockDbSet<Jurisdiccion>();
        db.Jurisdicciones.Returns(jurisdicciones);
        var controller = CreateController(mediator, db);
        mediator.Send(Arg.Any<CreateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));

        var result = await controller.Create(new JurisdiccionRequest("Buenos Aires"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetJurisdiccionById");
        AssertAnonymousProperty(created.Value!, "Id", 9L);
    }

    [Fact]
    public async Task Create_CuandoTieneExitoYSeRelee_DevuelveCreatedAtRouteConEntidad()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var jurisdicciones = MockDbSetHelper.CreateMockDbSet(new[] { BuildJurisdiccion(9, "Buenos Aires", true) });
        db.Jurisdicciones.Returns(jurisdicciones);
        var controller = CreateController(mediator, db);
        mediator.Send(Arg.Any<CreateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));

        var result = await controller.Create(new JurisdiccionRequest("Buenos Aires"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetJurisdiccionById");
        AssertAnonymousProperty(created.Value!, "Id", 9L);
        AssertAnonymousProperty(created.Value!, "Descripcion", "Buenos Aires");
        AssertAnonymousProperty(created.Value!, "Activo", true);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Jurisdiccion no encontrada."));

        var result = await controller.Update(8, new JurisdiccionRequest("Buenos Aires"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Jurisdicción 8 no encontrada");
    }

    [Fact]
    public async Task Update_CuandoTieneExitoYNoSeRelee_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var jurisdicciones = MockDbSetHelper.CreateMockDbSet<Jurisdiccion>();
        db.Jurisdicciones.Returns(jurisdicciones);
        var controller = CreateController(mediator, db);
        mediator.Send(Arg.Any<UpdateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(8, new JurisdiccionRequest("Buenos Aires"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task Update_CuandoTieneExitoYSeRelee_DevuelveOkConEntidad()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var jurisdicciones = MockDbSetHelper.CreateMockDbSet(new[] { BuildJurisdiccion(8, "Buenos Aires", true) });
        db.Jurisdicciones.Returns(jurisdicciones);
        var controller = CreateController(mediator, db);
        mediator.Send(Arg.Any<UpdateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(8, new JurisdiccionRequest("Buenos Aires"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Buenos Aires");
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Jurisdiccion no encontrada."));

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateJurisdiccionCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Jurisdiccion no encontrada."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateJurisdiccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateJurisdiccionCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    private static JurisdiccionesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new JurisdiccionesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Jurisdiccion BuildJurisdiccion(long id, string descripcion, bool activo)
    {
        var entity = Jurisdiccion.Crear(descripcion);
        typeof(Jurisdiccion).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activo)
            entity.Desactivar();

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}