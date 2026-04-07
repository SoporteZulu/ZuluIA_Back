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
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CategoriasItemsControllerTests
{
    [Fact]
    public async Task GetArbol_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = new List<CategoriaItemDto>
        {
            new()
            {
                Id = 1,
                Codigo = "RAIZ",
                Descripcion = "Raiz",
                Nivel = 1,
                Activo = true,
                Hijos = [new CategoriaItemDto { Id = 2, Codigo = "SUB", Descripcion = "Sub", Nivel = 2, Activo = true }]
            }
        };
        mediator.Send(Arg.Any<GetCategoriasItemsQuery>(), Arg.Any<CancellationToken>())
            .Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetArbol(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(categorias);
        await mediator.Received(1).Send(Arg.Any<GetCategoriasItemsQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByNivel_FiltraActivasYOrdenaPorOrdenNivelYDescripcion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildCategoria(1, null, "B", "Beta", 2, "002", true),
            BuildCategoria(2, null, "A", "Alfa", 2, "001", true),
            BuildCategoria(3, null, "C", "Gamma", 2, "003", false),
            BuildCategoria(4, null, "X", "Otra", 1, "001", true)
        });
        db.CategoriasItems.Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetByNivel(2, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IEnumerable<CategoriaItemDto>>().Subject.ToList();
        items.Should().HaveCount(2);
        items[0].Id.Should().Be(2);
        items[0].Codigo.Should().Be("A");
        items[1].Id.Should().Be(1);
        items[1].Codigo.Should().Be("B");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConIdYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(mediator, db);
        var command = new CreateCategoriaItemCommand(1, "SUB", "Subrubro", 2, "001.001");

        var result = await controller.Create(command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 9L);
        await mediator.Received(1).Send(
            Arg.Is<CreateCategoriaItemCommand>(request =>
                request.ParentId == 1 &&
                request.Codigo == "SUB" &&
                request.Descripcion == "Subrubro" &&
                request.Nivel == 2 &&
                request.OrdenNivel == "001.001"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Codigo requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateCategoriaItemCommand(null, string.Empty, "Desc", 1, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la categoría con ID 8."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateCategoriaItemRequest("CAT", "Categoria", "001"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Ya existe una categoría con el código 'CAT'."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateCategoriaItemRequest("CAT", "Categoria", "001"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateCategoriaItemRequest("CAT", "Categoria", "001"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("Categoría actualizada correctamente");
        await mediator.Received(1).Send(
            Arg.Is<UpdateCategoriaItemCommand>(command =>
                command.Id == 8 &&
                command.Codigo == "CAT" &&
                command.Descripcion == "Categoria" &&
                command.OrdenNivel == "001"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la categoría con ID 8."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneItemsActivos_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede desactivar una categoría que tiene ítems activos asociados."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(8, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("Categoría desactivada correctamente");
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la categoría con ID 8."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateCategoriaItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("Categoría activada correctamente");
        await mediator.Received(1).Send(
            Arg.Is<ActivateCategoriaItemCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    private static CategoriasItemsController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new CategoriasItemsController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CategoriaItem BuildCategoria(long id, long? parentId, string codigo, string descripcion, short nivel, string? ordenNivel, bool activa)
    {
        var entity = CategoriaItem.Crear(parentId, codigo, descripcion, nivel, ordenNivel, null);
        typeof(CategoriaItem).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activa)
            entity.Desactivar(null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}