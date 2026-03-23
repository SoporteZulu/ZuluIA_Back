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
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class EstadosProveedoresControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var estados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildEstado(2, "BLO", "Bloqueado", true, true),
            BuildEstado(1, "ACT", "Activo", false, true)
        });
        db.EstadosProveedores.Returns(estados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "ACT");
        AssertAnonymousProperty(items[0], "Bloquea", false);
        AssertAnonymousProperty(items[1], "Codigo", "BLO");
        AssertAnonymousProperty(items[1], "Bloquea", true);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var estados = MockDbSetHelper.CreateMockDbSet<EstadoProveedor>();
        db.EstadosProveedores.Returns(estados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var estados = MockDbSetHelper.CreateMockDbSet(new[] { BuildEstado(5, "ACT", "Activo", false, true) });
        db.EstadosProveedores.Returns(estados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "ACT");
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Activo");
        AssertAnonymousProperty(ok.Value!, "Bloquea", false);
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Failure<long>("Ya existe un estado con ese codigo."));

        var result = await controller.Create(new EstadoProveedorRequest("ACT", "Activo", false), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Failure<long>("Codigo requerido."));

        var result = await controller.Create(new EstadoProveedorRequest("", "Activo", false), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Success(9L));

        var result = await controller.Create(new EstadoProveedorRequest("ACT", "Activo", false), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(EstadosProveedoresController.GetById));
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Failure("Estado no encontrado."));

        var result = await controller.Update(8, new EstadoProveedorRequest("ACT", "Activo", false), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Failure("Ya existe un estado con ese codigo."));

        var result = await controller.Update(8, new EstadoProveedorRequest("ACT", "Activo", false), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Success());

        var result = await controller.Update(8, new EstadoProveedorRequest("ACT", "Activo", false), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Failure("Estado 8 no encontrado."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Success());

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateEstadoProveedorCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Failure("Estado 8 no encontrado."));

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(ZuluIA_Back.Domain.Common.Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateEstadoProveedorCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    private static EstadosProveedoresController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new EstadosProveedoresController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static EstadoProveedor BuildEstado(long id, string codigo, string descripcion, bool bloquea, bool activo)
    {
        var entity = EstadoProveedor.Crear(codigo, descripcion, bloquea, userId: null);
        typeof(EstadoProveedor).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activo)
            entity.Desactivar(userId: null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}