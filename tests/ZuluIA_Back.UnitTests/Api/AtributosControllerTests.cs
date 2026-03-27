using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class AtributosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var atributos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildAtributo(2, "Peso", "numero", false, true),
            BuildAtributo(1, "Color", "texto", true, true)
        });
        db.Atributos.Returns(atributos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Color");
        AssertAnonymousProperty(items[1], "Descripcion", "Peso");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var atributos = MockDbSetHelper.CreateMockDbSet<Atributo>();
        db.Atributos.Returns(atributos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Atributo 999 no encontrado");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var atributos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildAtributo(7, "Color", "texto", true, true)
        });
        db.Atributos.Returns(atributos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Color");
        AssertAnonymousProperty(ok.Value!, "Tipo", "texto");
        AssertAnonymousProperty(ok.Value!, "Requerido", true);
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Descripcion requerida."));

        var result = await controller.Create(new AtributoRequest("", "texto", false), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));

        var result = await controller.Create(new AtributoRequest("Color", "texto", true), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetAtributoById");
        AssertAnonymousProperty(created.Value!, "Id", 10L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Atributo no encontrado."));

        var result = await controller.Update(7, new AtributoRequest("Color", "texto", true), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo inválido."));

        var result = await controller.Update(7, new AtributoRequest("Color", "otro", true), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkConActivoTrue()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "activo", true);
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Atributo no encontrado."));

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Atributo no encontrado."));

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkConActivoFalseYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Desactivar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "activo", false);
        await mediator.Received(1).Send(
            Arg.Is<DeactivateAtributoCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByItem_CuandoHayValores_DevuelveJoinConAtributo()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var atributos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildAtributo(3, "Color", "texto", false, true)
        });
        var atributosItems = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildAtributoItem(12, 50, 3, "Rojo")
        });
        db.Atributos.Returns(atributos);
        db.AtributosItems.Returns(atributosItems);
        var controller = CreateController(mediator, db);

        var result = await controller.GetByItem(50, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().ContainSingle();
        AssertAnonymousProperty(items[0], "ItemId", 50L);
        AssertAnonymousProperty(items[0], "AtributoId", 3L);
        AssertAnonymousProperty(items[0], "Descripcion", "Color");
        AssertAnonymousProperty(items[0], "Tipo", "texto");
        AssertAnonymousProperty(items[0], "Valor", "Rojo");
    }

    [Fact]
    public async Task SetAtributoItem_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<SetAtributoItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SetAtributoItemResult>("El atributo es requerido."));

        var result = await controller.SetAtributoItem(50, new AtributoItemRequest(0, "Rojo"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SetAtributoItem_CuandoActualiza_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<SetAtributoItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SetAtributoItemResult(12, true)));

        var result = await controller.SetAtributoItem(50, new AtributoItemRequest(3, "Azul"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 12L);
        ok.Value!.ToString().Should().Contain("Valor actualizado");
    }

    [Fact]
    public async Task SetAtributoItem_CuandoCrea_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<SetAtributoItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SetAtributoItemResult(15, false)));

        var result = await controller.SetAtributoItem(50, new AtributoItemRequest(3, "Azul"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(AtributosController.GetByItem));
        AssertAnonymousProperty(created.Value!, "Id", 15L);
    }

    [Fact]
    public async Task DeleteAtributoItem_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteAtributoItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No existe el valor."));

        var result = await controller.DeleteAtributoItem(50, 3, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteAtributoItem_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteAtributoItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.DeleteAtributoItem(50, 3, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    private static AtributosController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new AtributosController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Atributo BuildAtributo(long id, string descripcion, string tipo, bool requerido, bool activo)
    {
        var entity = Atributo.Crear(descripcion, tipo, requerido);
        typeof(Atributo).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!activo)
            entity.Desactivar();

        return entity;
    }

    private static AtributoItem BuildAtributoItem(long id, long itemId, long atributoId, string valor)
    {
        var entity = AtributoItem.Crear(itemId, atributoId, valor);
        typeof(AtributoItem).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}