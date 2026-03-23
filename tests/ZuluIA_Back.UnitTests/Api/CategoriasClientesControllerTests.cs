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

public class CategoriasClientesControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildCategoria(2, "VIP", "Premium", true),
            BuildCategoria(1, "MIN", "Minorista", true)
        });
        db.CategoriasClientes.Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "MIN");
        AssertAnonymousProperty(items[1], "Codigo", "VIP");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = MockDbSetHelper.CreateMockDbSet<CategoriaCliente>();
        db.CategoriasClientes.Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var categorias = MockDbSetHelper.CreateMockDbSet(new[] { BuildCategoria(5, "VIP", "Premium", true) });
        db.CategoriasClientes.Returns(categorias);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "VIP");
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Premium");
        AssertAnonymousProperty(ok.Value!, "Activa", true);
    }

    [Fact]
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una categoria con ese codigo."));

        var result = await controller.Create(new CategoriaClienteRequest("VIP", "Premium"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Codigo requerido."));

        var result = await controller.Create(new CategoriaClienteRequest("", "Premium"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));

        var result = await controller.Create(new CategoriaClienteRequest("VIP", "Premium"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(CategoriasClientesController.GetById));
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Categoria no encontrada."));

        var result = await controller.Update(8, new CategoriaClienteRequest("VIP", "Premium"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Ya existe una categoria con ese codigo."));

        var result = await controller.Update(8, new CategoriaClienteRequest("VIP", "Premium"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(8, new CategoriaClienteRequest("VIP", "Premium"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Categoria 8 no encontrada."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateCategoriaClienteCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateCategoriaClienteCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    private static CategoriasClientesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new CategoriasClientesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CategoriaCliente BuildCategoria(long id, string codigo, string descripcion, bool activa)
    {
        var entity = CategoriaCliente.Crear(codigo, descripcion, userId: null);
        typeof(CategoriaCliente).BaseType!
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