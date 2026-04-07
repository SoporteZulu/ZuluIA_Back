using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class MenuControllerTests
{
    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IMenuRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var menu = MockDbSetHelper.CreateMockDbSet<MenuItem>();
        db.Menu.Returns(menu);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IMenuRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var menu = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMenuItem(15, null, "Administracion", "AdminForm", "gear", 1, 2, true)
        });
        db.Menu.Returns(menu);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetById(15, CancellationToken.None);

        var dto = result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().BeOfType<MenuItemDto>().Subject;
        dto.Id.Should().Be(15);
        dto.Descripcion.Should().Be("Administracion");
        dto.Formulario.Should().Be("AdminForm");
        dto.Icono.Should().Be("gear");
        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task GetArbol_CuandoHayPadreEHijo_DevuelveEstructuraJerarquica()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IMenuRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.GetArbolCompletoAsync(Arg.Any<CancellationToken>()).Returns(new List<MenuItem>
        {
            BuildMenuItem(1, null, "Root", null, null, 1, 1, true),
            BuildMenuItem(2, 1, "Child", "ChildForm", null, 2, 3, true),
            BuildMenuItem(3, null, "Second", null, null, 1, 2, true)
        });
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetArbol(CancellationToken.None);

        var roots = result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().BeAssignableTo<IEnumerable<MenuItemDto>>().Subject.ToList();
        roots.Should().HaveCount(2);
        roots[0].Id.Should().Be(1);
        roots[0].Hijos.Should().ContainSingle();
        roots[0].Hijos[0].Id.Should().Be(2);
        roots[1].Id.Should().Be(3);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("descripcion requerida"));

        var result = await controller.Create(new CreateMenuItemRequest(null, "", null, null, 1, 1), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripcion requerida");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(44L));

        var result = await controller.Create(new CreateMenuItemRequest(null, "Menu", "Form", "ico", 1, 1), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().Contain("44");
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el item."));

        var result = await controller.Update(10, new UpdateMenuItemRequest("Menu", "Form", "ico", 1), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("descripcion invalida"));

        var result = await controller.Update(10, new UpdateMenuItemRequest("", null, null, 1), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        var request = new UpdateMenuItemRequest("Menu", "Form", "ico", 1);
        mediator.Send(Arg.Any<UpdateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(10, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().Contain("actualizado correctamente");
        await mediator.Received(1).Send(
            Arg.Is<UpdateMenuItemCommand>(command =>
                command.Id == 10 &&
                command.Descripcion == request.Descripcion &&
                command.Formulario == request.Formulario &&
                command.Icono == request.Icono &&
                command.Orden == request.Orden),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el item."));

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("no puede desactivarse"));

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().Contain("desactivado correctamente");
        await mediator.Received(1).Send(
            Arg.Is<DeleteMenuItemCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el item."));

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("no puede activarse"));

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMenuItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateMenuItemCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static MenuController CreateController(IMediator mediator, IMenuRepository? repo = null, IApplicationDbContext? db = null)
    {
        var controller = new MenuController(mediator, repo ?? Substitute.For<IMenuRepository>(), db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static MenuItem BuildMenuItem(long id, long? parentId, string descripcion, string? formulario, string? icono, short nivel, short orden, bool activo)
    {
        var entity = MenuItem.Crear(parentId, descripcion, formulario, icono, nivel, orden);
        typeof(MenuItem).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activo)
            entity.Desactivar();

        return entity;
    }
}