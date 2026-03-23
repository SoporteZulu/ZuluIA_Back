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

public class ChequerasControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenadosPorIdDesc()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var chequeras = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildChequera(2, 5, "Banco Dos", "002", 100, 150, 110, true, "Segunda"),
            BuildChequera(3, 5, "Banco Tres", "003", 200, 250, 205, true, "Tercera"),
            BuildChequera(1, 5, "Banco Uno", "001", 1, 50, 10, false, "Primera")
        });
        db.Chequeras.Returns(chequeras);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(3);
        AssertAnonymousProperty(items[0], "Id", 3L);
        AssertAnonymousProperty(items[1], "Id", 2L);
        AssertAnonymousProperty(items[2], "Id", 1L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var chequeras = MockDbSetHelper.CreateMockDbSet<Chequera>();
        db.Chequeras.Returns(chequeras);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var chequeras = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildChequera(5, 7, "Banco Uno", "001", 1, 50, 10, true, "Observacion")
        });
        db.Chequeras.Returns(chequeras);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "CajaId", 7L);
        AssertAnonymousProperty(ok.Value!, "Banco", "Banco Uno");
        AssertAnonymousProperty(ok.Value!, "NroCuenta", "001");
        AssertAnonymousProperty(ok.Value!, "UltimoChequeUsado", 10);
        AssertAnonymousProperty(ok.Value!, "Activa", true);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Rango de numeración inválido."));
        var controller = CreateController(mediator);

        var result = await controller.Create(new CreateChequeraRequest(7, "Banco Uno", "001", 50, 1, "Obs"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(mediator);

        var result = await controller.Create(new CreateChequeraRequest(7, "Banco Uno", "001", 1, 50, "Obs"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ChequerasController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 9L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Chequera no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Update(8, new UpdateChequeraRequest("Banco", "002", "Obs"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(8, new UpdateChequeraRequest("Banco", "002", "Obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task UsarCheque_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UsarChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UsarChequeResult>("Chequera no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.UsarCheque(8, 15, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UsarCheque_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UsarChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UsarChequeResult>("Número fuera del rango de la chequera."));
        var controller = CreateController(mediator);

        var result = await controller.UsarCheque(8, 150, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UsarCheque_CuandoTieneExito_DevuelveOkConResultado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UsarChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new UsarChequeResult(8, 15)));
        var controller = CreateController(mediator);

        var result = await controller.UsarCheque(8, 15, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        AssertAnonymousProperty(ok.Value!, "UltimoChequeUsado", 15);
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Chequera no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateChequeraCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateChequeraCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateChequeraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Chequera no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static ChequerasController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new ChequerasController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Chequera BuildChequera(
        long id,
        long cajaId,
        string banco,
        string nroCuenta,
        int nroDesde,
        int nroHasta,
        int ultimoChequeUsado,
        bool activa,
        string? observacion)
    {
        var entity = Chequera.Crear(cajaId, banco, nroCuenta, nroDesde, nroHasta, observacion, userId: null);
        typeof(Chequera).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        typeof(Chequera)
            .GetProperty(nameof(Chequera.UltimoChequeUsado))!
            .SetValue(entity, ultimoChequeUsado);

        if (!activa)
            entity.Desactivar(userId: null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}