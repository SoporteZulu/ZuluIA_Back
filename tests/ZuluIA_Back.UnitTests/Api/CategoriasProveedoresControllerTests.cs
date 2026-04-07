using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CategoriasProveedoresControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildCategoria(2, "EXT", "Exterior", true),
            BuildCategoria(1, "LOC", "Local", true)
        });
        db.CategoriasProveedores.Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "EXT");
        AssertAnonymousProperty(items[1], "Codigo", "LOC");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = MockDbSetHelper.CreateMockDbSet<CategoriaProveedor>();
        db.CategoriasProveedores.Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = MockDbSetHelper.CreateMockDbSet(new[] { BuildCategoria(5, "LOC", "Local", true) });
        db.CategoriasProveedores.Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "LOC");
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Local");
        AssertAnonymousProperty(ok.Value!, "Activa", true);
    }

    [Fact]
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una categoria con ese codigo."));

        var result = await controller.Create(new CategoriaProveedorRequest("LOC", "Local"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Codigo requerido."));

        var result = await controller.Create(new CategoriaProveedorRequest("", "Local"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));

        var result = await controller.Create(new CategoriaProveedorRequest("LOC", "Local"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(CategoriasProveedoresController.GetById));
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Categoria no encontrada."));

        var result = await controller.Update(8, new CategoriaProveedorRequest("LOC", "Local"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Ya existe una categoria con ese codigo."));

        var result = await controller.Update(8, new CategoriaProveedorRequest("LOC", "Local"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(8, new CategoriaProveedorRequest("LOC", "Local"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Categoria 8 no encontrada."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateCategoriaProveedorCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static CategoriasProveedoresController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new CategoriasProveedoresController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CategoriaProveedor BuildCategoria(long id, string codigo, string descripcion, bool activa)
    {
        var entity = CategoriaProveedor.Crear(codigo, descripcion, userId: null);
        typeof(CategoriaProveedor).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activa)
            entity.Desactivar(userId: null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}