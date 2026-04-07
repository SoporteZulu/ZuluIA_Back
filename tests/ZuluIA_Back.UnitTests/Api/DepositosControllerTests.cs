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

public class DepositosControllerTests
{
    [Fact]
    public async Task GetBySucursal_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        IReadOnlyList<DepositoDto> data = [new DepositoDto { Id = 4, SucursalId = 7, Descripcion = "Principal", EsDefault = true, Activo = true }];
        mediator.Send(Arg.Any<GetDepositosBySucursalQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator, db);

        var result = await controller.GetBySucursal(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetDepositosBySucursalQuery>(query => query.SucursalId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDefault_CuandoExiste_DevuelveOkConProyeccion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var depositos = MockDbSetHelper.CreateMockDbSet([
            BuildDeposito(1, 5, "Secundario", false, true),
            BuildDeposito(2, 5, "Principal", true, true),
            BuildDeposito(3, 5, "Inactivo", true, false)
        ]);
        db.Depositos.Returns(depositos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetDefault(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 2L);
        AssertAnonymousProperty(ok.Value!, "SucursalId", 5L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Principal");
        AssertAnonymousProperty(ok.Value!, "EsDefault", true);
    }

    [Fact]
    public async Task GetDefault_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var depositos = MockDbSetHelper.CreateMockDbSet([
            BuildDeposito(1, 5, "Secundario", false, true)
        ]);
        db.Depositos.Returns(depositos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetDefault(5, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new CreateDepositoCommand(5, "", false);
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es obligatoria."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new CreateDepositoCommand(5, "Principal", true);
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 10L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateDepositoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el depósito con ID 9."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(9, new UpdateDepositoRequest("Principal", true), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateDepositoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(9, new UpdateDepositoRequest("Principal", true), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Depósito actualizado correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<UpdateDepositoCommand>(command => command.Id == 9 && command.Descripcion == "Principal" && command.EsDefault),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoNoSePuedeDesactivar_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteDepositoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede desactivar un depósito que tiene stock disponible."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(6, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteDepositoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el depósito con ID 6."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(6, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteDepositoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(6, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Depósito desactivado correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<DeleteDepositoCommand>(command => command.Id == 6),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateDepositoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el depósito con ID 4."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateDepositoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Depósito activado correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<ActivateDepositoCommand>(command => command.Id == 4),
            Arg.Any<CancellationToken>());
    }

    private static DepositosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new DepositosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Deposito BuildDeposito(long id, long sucursalId, string descripcion, bool esDefault, bool activo)
    {
        var entity = Deposito.Crear(sucursalId, descripcion, esDefault);
        entity.GetType().GetProperty(nameof(Deposito.Id))!.SetValue(entity, id);
        if (!activo)
            entity.Desactivar();

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}