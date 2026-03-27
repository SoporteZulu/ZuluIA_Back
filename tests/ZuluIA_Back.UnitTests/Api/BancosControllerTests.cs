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
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class BancosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var bancos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildBanco(2, "Santander"),
            BuildBanco(1, "Banco Nacion")
        });
        db.Bancos.Returns(bancos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Banco Nacion");
        AssertAnonymousProperty(items[1], "Descripcion", "Santander");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var bancos = MockDbSetHelper.CreateMockDbSet<Banco>();
        db.Bancos.Returns(bancos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var bancos = MockDbSetHelper.CreateMockDbSet(new[] { BuildBanco(5, "Galicia") });
        db.Bancos.Returns(bancos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Galicia");
    }

    [Fact]
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un banco con esa descripcion."));

        var result = await controller.Create(new BancoRequest("Galicia"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Descripcion requerida."));

        var result = await controller.Create(new BancoRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var bancos = MockDbSetHelper.CreateMockDbSet(new[] { BuildBanco(8, "Patagonia") });
        db.Bancos.Returns(bancos);
        var controller = CreateController(mediator, db);
        mediator.Send(Arg.Any<CreateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(8L));

        var result = await controller.Create(new BancoRequest("Patagonia"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(BancosController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 8L);
        AssertAnonymousProperty(created.Value!, "Descripcion", "Patagonia");
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UpdateBancoResult>("Banco no encontrado."));

        var result = await controller.Update(7, new BancoRequest("Nuevo"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UpdateBancoResult>("Ya existe un banco con esa descripcion."));

        var result = await controller.Update(7, new BancoRequest("Nuevo"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new UpdateBancoResult(7, "Nuevo")));

        var result = await controller.Update(7, new BancoRequest("Nuevo"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Nuevo");
        await mediator.Received(1).Send(
            Arg.Is<UpdateBancoCommand>(command =>
                command.Id == 7 &&
                command.Descripcion == "Nuevo"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoHayRelacionados_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No puede eliminarse porque tiene registros relacionados."));

        var result = await controller.Delete(3, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Banco 3 no encontrado."));

        var result = await controller.Delete(3, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeleteBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Delete(3, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    private static BancosController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new BancosController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Banco BuildBanco(long id, string descripcion)
    {
        var entity = Banco.Crear(descripcion);
        typeof(Banco).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}