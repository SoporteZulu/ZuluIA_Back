using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Proyectos.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Proyectos;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ProyectosControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenDescendente()
    {
        Proyecto[] proyectos =
        [
            BuildProyecto(1, "PRJ001", "Proyecto uno", 10, 20, "activo", false, new DateOnly(2026, 3, 1), 6, false),
            BuildProyecto(3, "PRJ003", "Proyecto tres", 10, 20, "activo", false, new DateOnly(2026, 3, 3), 8, true),
            BuildProyecto(2, "PRJ002", "Proyecto dos", 10, 21, "finalizado", false, new DateOnly(2026, 3, 2), 7, false),
            BuildProyecto(4, "PRJ004", "Proyecto cuatro", 11, 20, "activo", false, new DateOnly(2026, 3, 4), 9, false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(proyectos: proyectos));

        var result = await controller.GetAll(10, 20, "activo", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 3L);
        AssertAnonymousProperty(items[0], "Codigo", "PRJ003");
        AssertAnonymousProperty(items[1], "Id", 1L);
        AssertAnonymousProperty(items[1], "Estado", "activo");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        Proyecto[] proyectos =
        [
            BuildProyecto(5, "PRJ005", "Proyecto detallado", 10, 20, "finalizado", false, new DateOnly(2026, 3, 5), 12, true, "obs")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(proyectos: proyectos));

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "PRJ005");
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Proyecto detallado");
        AssertAnonymousProperty(ok.Value!, "SoloPadre", true);
        AssertAnonymousProperty(ok.Value!, "Observacion", "obs");
    }

    [Fact]
    public async Task GetComprobantes_FiltraDeshabilitadasYProyecto()
    {
        ComprobanteProyecto[] links =
        [
            BuildComprobanteProyecto(1, 100, 5, 50m, 1000m, 1, false, "uno"),
            BuildComprobanteProyecto(2, 101, 5, 25m, 500m, 2, true, "dos"),
            BuildComprobanteProyecto(3, 102, 6, 75m, 1500m, 3, false, "tres")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(comprobantesProyectos: links));

        var result = await controller.GetComprobantes(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "ComprobanteId", 100L);
        AssertAnonymousProperty(items[0], "Porcentaje", 50m);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La sucursal es requerida."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Create(BuildCreateRequest(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La sucursal es requerida.");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(mediator, BuildDb());
        var request = BuildCreateRequest();

        var result = await controller.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ProyectosController.GetById));
        created.RouteValues!["id"].Should().Be(9L);
        AssertAnonymousProperty(created.Value!, "id", 9L);
        await mediator.Received(1).Send(
            Arg.Is<CreateProyectoCommand>(command =>
                command.Codigo == request.Codigo &&
                command.Descripcion == request.Descripcion &&
                command.SucursalId == request.SucursalId &&
                command.TerceroId == request.TerceroId &&
                command.FechaInicio == request.FechaInicio &&
                command.FechaFin == request.FechaFin &&
                command.TotalCuotas == request.TotalCuotas &&
                command.SoloPadre == request.SoloPadre &&
                command.Observacion == request.Observacion),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el proyecto con ID 5."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Update(5, BuildUpdateRequest(), CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "No se encontró el proyecto con ID 5.");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaIdDeRuta()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());
        var request = BuildUpdateRequest();

        var result = await controller.Update(7, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 7L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateProyectoCommand>(command =>
                command.Id == 7 &&
                command.Descripcion == request.Descripcion &&
                command.FechaInicio == request.FechaInicio &&
                command.FechaFin == request.FechaFin &&
                command.TotalCuotas == request.TotalCuotas &&
                command.SoloPadre == request.SoloPadre &&
                command.Observacion == request.Observacion),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Finalizar")]
    [InlineData("Anular")]
    [InlineData("Reactivar")]
    public async Task Lifecycle_CuandoNoExiste_DevuelveNotFound(string action)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Result.Failure("No se encontró el proyecto con ID 8."));
        var controller = CreateController(mediator, BuildDb());

        var result = action switch
        {
            "Finalizar" => await controller.Finalizar(8, CancellationToken.None),
            "Anular" => await controller.Anular(8, CancellationToken.None),
            _ => await controller.Reactivar(8, CancellationToken.None)
        };

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Theory]
    [InlineData("Finalizar")]
    [InlineData("Anular")]
    [InlineData("Reactivar")]
    public async Task Lifecycle_CuandoTieneExito_DevuelveOk(string action)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = action switch
        {
            "Finalizar" => await controller.Finalizar(8, CancellationToken.None),
            "Anular" => await controller.Anular(8, CancellationToken.None),
            _ => await controller.Reactivar(8, CancellationToken.None)
        };

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Reactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ReactivarProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Reactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ReactivarProyectoCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AsignarComprobante_CuandoNoExisteProyecto_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AsignarComprobanteProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("No se encontró el proyecto con ID 9."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AsignarComprobante(9, new AsignarComprobanteProyectoRequest(100, 50m, 1000m, 1, "obs"), CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "No se encontró el proyecto con ID 9.");
    }

    [Fact]
    public async Task AsignarComprobante_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AsignarComprobanteProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, BuildDb());
        var request = new AsignarComprobanteProyectoRequest(100, 50m, 1000m, 1, "obs");

        var result = await controller.AsignarComprobante(9, request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ProyectosController.GetById));
        created.RouteValues!["id"].Should().Be(9L);
        AssertAnonymousProperty(created.Value!, "id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<AsignarComprobanteProyectoCommand>(command =>
                command.ProyectoId == 9 &&
                command.ComprobanteId == 100 &&
                command.Porcentaje == 50m &&
                command.Importe == 1000m &&
                command.NroCuota == 1 &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesasignarComprobante_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DesasignarComprobanteProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la asignación del comprobante para el proyecto."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DesasignarComprobante(9, 15, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "No se encontró la asignación del comprobante para el proyecto.");
    }

    [Fact]
    public async Task DesasignarComprobante_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DesasignarComprobanteProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DesasignarComprobante(9, 15, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    private static ProyectosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ProyectosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(IEnumerable<Proyecto>? proyectos = null, IEnumerable<ComprobanteProyecto>? comprobantesProyectos = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var proyectosDbSet = MockDbSetHelper.CreateMockDbSet(proyectos);
        var comprobantesDbSet = MockDbSetHelper.CreateMockDbSet(comprobantesProyectos);
        db.Proyectos.Returns(proyectosDbSet);
        db.ComprobantesProyectos.Returns(comprobantesDbSet);
        return db;
    }

    private static CreateProyectoRequest BuildCreateRequest()
    {
        return new CreateProyectoRequest("PRJ001", "Proyecto nuevo", 10, 20, new DateOnly(2026, 3, 1), new DateOnly(2026, 12, 31), 12, true, "obs");
    }

    private static UpdateProyectoRequest BuildUpdateRequest()
    {
        return new UpdateProyectoRequest("Proyecto actualizado", new DateOnly(2026, 4, 1), new DateOnly(2026, 11, 30), 10, false, "obs up");
    }

    private static Proyecto BuildProyecto(long id, string codigo, string descripcion, long sucursalId, long? terceroId, string estado, bool anulada, DateOnly? fechaInicio, int totalCuotas, bool soloPadre, string? observacion = null)
    {
        var entity = Proyecto.Crear(codigo, descripcion, sucursalId, terceroId, fechaInicio, fechaInicio?.AddMonths(6), totalCuotas, soloPadre, observacion);
        typeof(Proyecto).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (estado == "finalizado")
            entity.Finalizar();
        else if (estado == "anulado" || anulada)
            entity.Anular();

        return entity;
    }

    private static ComprobanteProyecto BuildComprobanteProyecto(long id, long comprobanteId, long proyectoId, decimal porcentaje, decimal importe, int nroCuota, bool deshabilitada, string? observacion)
    {
        var entity = ComprobanteProyecto.Crear(comprobanteId, proyectoId, porcentaje, importe, nroCuota, observacion);
        typeof(ComprobanteProyecto).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (deshabilitada)
            entity.Deshabilitar();

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}