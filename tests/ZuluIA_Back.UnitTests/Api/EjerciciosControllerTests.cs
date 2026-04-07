using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Api;

public class EjerciciosControllerTests
{
    [Fact]
    public async Task GetAll_DevuelveOkConResultadoDelMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        IReadOnlyList<EjercicioDto> data = [new() { Id = 1, Descripcion = "2026" }];
        mediator.Send(Arg.Any<GetEjerciciosQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
    }

    [Fact]
    public async Task GetVigente_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        repo.GetVigenteAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((Ejercicio?)null);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetVigente(null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetVigente_CuandoNoSeInformaFecha_UsaHoyYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        var ejercicio = BuildEjercicio(5, "2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "00.00", false);
        repo.GetVigenteAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(ejercicio);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetVigente(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "2026");
        await repo.Received(1).GetVigenteAsync(DateOnly.FromDateTime(DateTime.Today), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        repo.GetByIdConSucursalesAsync(9, Arg.Any<CancellationToken>())
            .Returns((Ejercicio?)null);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConSucursales()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        var ejercicio = BuildEjercicio(5, "2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "00.00", false);
        ejercicio.AsignarSucursal(3, true);
        repo.GetByIdConSucursalesAsync(5, Arg.Any<CancellationToken>())
            .Returns(ejercicio);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        var sucursales = ((System.Collections.IEnumerable)ok.Value!.GetType().GetProperty("Sucursales")!.GetValue(ok.Value)!).Cast<object>().ToList();
        sucursales.Should().HaveCount(1);
        AssertAnonymousProperty(sucursales[0], "SucursalId", 3L);
        AssertAnonymousProperty(sucursales[0], "UsaContabilidad", true);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        mediator.Send(Arg.Any<CreateEjercicioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Fechas inválidas."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Create(new CreateEjercicioCommand("2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "00.00", [3]), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        mediator.Send(Arg.Any<CreateEjercicioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(8L));
        var controller = CreateController(mediator, repo);

        var result = await controller.Create(new CreateEjercicioCommand("2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "00.00", [3]), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetEjercicioById");
        AssertAnonymousProperty(created.Value!, "id", 8L);
    }

    [Fact]
    public async Task Cerrar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        mediator.Send(Arg.Any<CerrarEjercicioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el ejercicio con ID 4."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Cerrar(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cerrar_CuandoTieneExito_DevuelveOkConMensajeYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        mediator.Send(Arg.Any<CerrarEjercicioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo);

        var result = await controller.Cerrar(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Ejercicio cerrado correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<CerrarEjercicioCommand>(command => command.Id == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reabrir_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        mediator.Send(Arg.Any<ReabrirEjercicioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El ejercicio no está cerrado."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Reabrir(4, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reabrir_CuandoTieneExito_DevuelveOkConMensajeYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        mediator.Send(Arg.Any<ReabrirEjercicioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo);

        var result = await controller.Reabrir(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Ejercicio reabierto correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<ReabrirEjercicioCommand>(command => command.Id == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AsignarSucursal_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEjercicioRepository>();
        mediator.Send(Arg.Any<AsignarSucursalEjercicioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo);

        var result = await controller.AsignarSucursal(4, new AsignarSucursalEjercicioRequest(7, false), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Sucursal asignada al ejercicio correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<AsignarSucursalEjercicioCommand>(command =>
                command.EjercicioId == 4 &&
                command.SucursalId == 7 &&
                !command.UsaContabilidad),
            Arg.Any<CancellationToken>());
    }

    private static EjerciciosController CreateController(IMediator mediator, IEjercicioRepository repo)
    {
        return new EjerciciosController(mediator, repo)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Ejercicio BuildEjercicio(long id, string descripcion, DateOnly fechaInicio, DateOnly fechaFin, string mascara, bool cerrado)
    {
        var entity = Ejercicio.Crear(descripcion, fechaInicio, fechaFin, mascara);
        SetProperty(entity, nameof(Ejercicio.Id), id);
        SetProperty(entity, nameof(Ejercicio.Cerrado), cerrado);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}