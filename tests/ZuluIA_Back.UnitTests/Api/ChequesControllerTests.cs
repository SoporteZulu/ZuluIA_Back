using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.Commands;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Application.Features.Cheques.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ChequesControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<ChequeDto>(
            [
                new ChequeDto
                {
                    Id = 1,
                    CajaId = 5,
                    TerceroId = 10,
                    NroCheque = "0001",
                    Banco = "Nacion",
                    FechaEmision = new DateOnly(2026, 3, 1),
                    FechaVencimiento = new DateOnly(2026, 3, 30),
                    Importe = 100m,
                    MonedaId = 1,
                    Estado = "Cartera"
                }
            ],
            2,
            15,
            31);
        mediator.Send(Arg.Any<GetChequesPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator: mediator);

        var result = await controller.GetAll(2, 15, 5, 10, "depositado", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<ChequeDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(15);
        payload.TotalCount.Should().Be(31);

        await mediator.Received(1).Send(
            Arg.Is<GetChequesPagedQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 15 &&
                q.CajaId == 5 &&
                q.TerceroId == 10 &&
                q.Estado == EstadoCheque.Depositado &&
                q.Desde == new DateOnly(2026, 3, 1) &&
                q.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCartera_CuandoHayCheques_DevuelveFiltradosYOrdenados()
    {
        var db = BuildDb(
            cheques:
            [
                BuildCheque(1, 5, "0002", "Nacion", 200m, 1, new DateOnly(2026, 3, 10), new DateOnly(2026, 4, 5), EstadoCheque.Cartera, 10),
                BuildCheque(2, 5, "0001", "Galicia", 100m, 1, new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 20), EstadoCheque.Cartera, 10),
                BuildCheque(3, 5, "0003", "Provincia", 150m, 1, new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 25), EstadoCheque.Depositado, 10),
                BuildCheque(4, 6, "0004", "Macro", 175m, 1, new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 18), EstadoCheque.Cartera, 10)
            ]);
        var controller = CreateController(db: db);

        var result = await controller.GetCartera(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "Estado", EstadoCheque.Cartera.ToString());
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El importe del cheque debe ser mayor a 0."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Create(BuildCreateChequeCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("mayor a 0");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Create(BuildCreateChequeCommand(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 21L);
    }

    [Fact]
    public async Task Depositar_CuandoFalla_DevuelveBadRequestYMandaAccionCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Solo se pueden depositar cheques en cartera."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Depositar(7, new DepositarChequeRequest(new DateOnly(2026, 3, 21), new DateOnly(2026, 3, 25)), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<CambiarEstadoChequeCommand>(c =>
                c.Id == 7 &&
                c.Accion == AccionCheque.Depositar &&
                c.Fecha == new DateOnly(2026, 3, 21) &&
                c.FechaAcreditacion == new DateOnly(2026, 3, 25) &&
                c.Observacion == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Depositar_CuandoTieneExito_DevuelveOkYMandaAccionCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Depositar(7, new DepositarChequeRequest(new DateOnly(2026, 3, 21), new DateOnly(2026, 3, 25)), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<CambiarEstadoChequeCommand>(c =>
                c.Id == 7 &&
                c.Accion == AccionCheque.Depositar &&
                c.Fecha == new DateOnly(2026, 3, 21) &&
                c.FechaAcreditacion == new DateOnly(2026, 3, 25) &&
                c.Observacion == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Acreditar_CuandoTieneExito_DevuelveOkYMandaAccionCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Acreditar(7, new AcreditarChequeRequest(new DateOnly(2026, 3, 25)), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<CambiarEstadoChequeCommand>(c =>
                c.Id == 7 &&
                c.Accion == AccionCheque.Acreditar &&
                c.Fecha == new DateOnly(2026, 3, 25) &&
                c.FechaAcreditacion == null &&
                c.Observacion == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Acreditar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El cheque no está en estado depositado."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Acreditar(7, new AcreditarChequeRequest(new DateOnly(2026, 3, 25)), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Rechazar_CuandoFalla_DevuelveBadRequestYMandaObservacion()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El cheque ya está rechazado."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Rechazar(7, new RechazarChequeRequest("firma inválida"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<CambiarEstadoChequeCommand>(c =>
                c.Id == 7 &&
                c.Accion == AccionCheque.Rechazar &&
                c.Fecha == null &&
                c.FechaAcreditacion == null &&
                c.Observacion == "firma inválida"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rechazar_CuandoTieneExito_DevuelveOkYMandaObservacion()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Rechazar(7, new RechazarChequeRequest("firma inválida"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<CambiarEstadoChequeCommand>(c =>
                c.Id == 7 &&
                c.Accion == AccionCheque.Rechazar &&
                c.Fecha == null &&
                c.FechaAcreditacion == null &&
                c.Observacion == "firma inválida"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Entregar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Entregar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Entregar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CambiarEstadoChequeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El cheque no está en cartera."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Entregar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private static ChequesController CreateController(IMediator? mediator = null, IApplicationDbContext? db = null)
    {
        var controller = new ChequesController(mediator ?? Substitute.For<IMediator>(), db ?? BuildDb())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(IEnumerable<Cheque>? cheques = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var chequesDbSet = MockDbSetHelper.CreateMockDbSet(cheques);
        db.Cheques.Returns(chequesDbSet);
        return db;
    }

    private static Cheque BuildCheque(
        long id,
        long cajaId,
        string nroCheque,
        string banco,
        decimal importe,
        long monedaId,
        DateOnly fechaEmision,
        DateOnly fechaVencimiento,
        EstadoCheque estado,
        long? terceroId)
    {
        var entity = Cheque.Crear(cajaId, terceroId, nroCheque, banco, fechaEmision, fechaVencimiento, importe, monedaId, null, 1);
        SetProperty(entity, nameof(Cheque.Id), id);
        SetProperty(entity, nameof(Cheque.Estado), estado);
        return entity;
    }

    private static CreateChequeCommand BuildCreateChequeCommand()
    {
        return new CreateChequeCommand(
            5,
            10,
            "0001",
            "Nacion",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 30),
            100m,
            1,
            "Cheque de prueba");
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}