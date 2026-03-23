using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Configuracion.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PlanillasControllerTests
{
    [Fact]
    public async Task GetPlantillas_OrdenaPorDescripcion()
    {
        PlantillaDiagnostico[] plantillas =
        [
            BuildPlantilla(2, "Beta", new DateTime(2026, 2, 1), "B"),
            BuildPlantilla(1, "Alfa", new DateTime(2026, 1, 1), "A")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(plantillas: plantillas));

        var result = await controller.GetPlantillas(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Descripcion", "Alfa");
        AssertAnonymousProperty(items[1], "Id", 2L);
    }

    [Fact]
    public async Task GetPlantillaById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetPlantillaById(9, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Plantilla 9 no encontrada.");
    }

    [Fact]
    public async Task GetPlantillaById_CuandoExiste_DevuelveOk()
    {
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDb(plantillas: [BuildPlantilla(5, "Checklist", new DateTime(2026, 3, 1), "Obs")])) ;

        var result = await controller.GetPlantillaById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Checklist");
        AssertAnonymousProperty(ok.Value!, "Observaciones", "Obs");
    }

    [Fact]
    public async Task CreatePlantilla_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePlantillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreatePlantilla(new PlantillaRequest(" "), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La descripción es requerida.");
    }

    [Fact]
    public async Task CreatePlantilla_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePlantillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(7L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreatePlantilla(new PlantillaRequest("Checklist"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PlanillasController.GetPlantillaById));
        created.RouteValues!["id"].Should().Be(7L);
        AssertAnonymousProperty(created.Value!, "Id", 7L);
    }

    [Fact]
    public async Task UpdatePlantilla_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlantillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Plantilla 4 no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlantilla(4, new PlantillaRequest("Nueva"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePlantilla_CuandoFallaValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlantillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La descripción es requerida."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlantilla(4, new PlantillaRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdatePlantilla_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlantillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlantilla(4, new PlantillaRequest("Nueva"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 4L);
    }

    [Fact]
    public async Task DeletePlantilla_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlantillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Plantilla 4 no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlantilla(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeletePlantilla_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlantillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlantilla(4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetPlantillaDetalle_FiltraPorPlantilla()
    {
        PlantillaDiagnosticoDetalle[] detalles =
        [
            BuildPlantillaDetalle(1, 5, 10, 30, 100),
            BuildPlantillaDetalle(2, 5, 11, 20, 90),
            BuildPlantillaDetalle(3, 6, 12, 10, 80)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(plantillaDetalles: detalles));

        var result = await controller.GetPlantillaDetalle(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "PlantillaId", 5L);
        AssertAnonymousProperty(items[1], "PlantillaId", 5L);
    }

    [Fact]
    public async Task AddPlantillaDetalle_CuandoNoExistePlantilla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPlantillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Plantilla 5 no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddPlantillaDetalle(5, new PlantillaDetalleRequest(10, 35, 100), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddPlantillaDetalle_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPlantillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddPlantillaDetalle(5, new PlantillaDetalleRequest(10, 35, 100), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PlanillasController.GetPlantillaDetalle));
        created.RouteValues!["id"].Should().Be(5L);
        AssertAnonymousProperty(created.Value!, "Id", 15L);
    }

    [Fact]
    public async Task UpdatePlantillaDetalle_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlantillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Detalle de plantilla no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlantillaDetalle(5, 8, new PlantillaDetalleRequest(10, 25, 90), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePlantillaDetalle_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlantillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlantillaDetalle(5, 8, new PlantillaDetalleRequest(10, 25, 90), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task DeletePlantillaDetalle_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlantillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Detalle de plantilla no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlantillaDetalle(5, 8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeletePlantillaDetalle_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlantillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlantillaDetalle(5, 8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetPlanillas_AplicaFiltrosYOrdenaPorFechaRegistroDescendente()
    {
        PlanillaDiagnostico[] planillas =
        [
            BuildPlanilla(1, 10, 100, new DateTime(2026, 1, 10), new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), "Uno"),
            BuildPlanilla(2, 10, 100, new DateTime(2026, 2, 10), new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), "Dos"),
            BuildPlanilla(3, 11, 100, new DateTime(2026, 3, 10), new DateTime(2026, 3, 1), new DateTime(2026, 3, 31), "Tres"),
            BuildPlanilla(4, 10, 101, new DateTime(2026, 4, 10), new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), "Cuatro")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(planillas: planillas));

        var result = await controller.GetPlanillas(10, 100, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task GetPlanillaById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetPlanillaById(12, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Planilla 12 no encontrada.");
    }

    [Fact]
    public async Task GetPlanillaById_CuandoExiste_DevuelveOk()
    {
        var fecha = new DateTime(2026, 3, 20);
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDb(planillas: [BuildPlanilla(12, 10, 100, fecha, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31), "Obs")])) ;

        var result = await controller.GetPlanillaById(12, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 12L);
        AssertAnonymousProperty(ok.Value!, "ClienteId", 10L);
        AssertAnonymousProperty(ok.Value!, "Observaciones", "Obs");
    }

    [Fact]
    public async Task CreatePlanilla_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePlanillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("ClienteId invalido."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreatePlanilla(new PlanillaRequest(0, 100), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreatePlanilla_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePlanillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(20L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreatePlanilla(new PlanillaRequest(10, 100), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PlanillasController.GetPlanillaById));
        created.RouteValues!["id"].Should().Be(20L);
        AssertAnonymousProperty(created.Value!, "Id", 20L);
    }

    [Fact]
    public async Task UpdatePlanilla_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlanillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Planilla 20 no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlanilla(20, new PlanillaRequest(10, 100), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePlanilla_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlanillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlanilla(20, new PlanillaRequest(10, 100), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 20L);
    }

    [Fact]
    public async Task DeletePlanilla_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlanillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Planilla 20 no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlanilla(20, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeletePlanilla_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlanillaDiagnosticoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlanilla(20, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetPlanillaDetalle_FiltraPorPlanilla()
    {
        PlanillaDiagnosticoDetalle[] detalles =
        [
            BuildPlanillaDetalle(1, 20, 101, 201, 5, 50, 40, 90),
            BuildPlanillaDetalle(2, 20, 102, 202, 7, 70, 30, 95),
            BuildPlanillaDetalle(3, 21, 103, 203, 8, 80, 20, 99)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(planillaDetalles: detalles));

        var result = await controller.GetPlanillaDetalle(20, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "PlanillaId", 20L);
        AssertAnonymousProperty(items[1], "PlanillaId", 20L);
    }

    [Fact]
    public async Task AddPlanillaDetalle_CuandoNoExistePlanilla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPlanillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Planilla 20 no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddPlanillaDetalle(20, new PlanillaDetalleRequest(101, 201, 5, 50, 40, 90), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddPlanillaDetalle_CuandoFallaValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPlanillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El valor es invalido."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddPlanillaDetalle(20, new PlanillaDetalleRequest(101, 201, 5, 50, 40, 90), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddPlanillaDetalle_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPlanillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(25L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddPlanillaDetalle(20, new PlanillaDetalleRequest(101, 201, 5, 50, 40, 90), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PlanillasController.GetPlanillaDetalle));
        created.RouteValues!["id"].Should().Be(20L);
        AssertAnonymousProperty(created.Value!, "Id", 25L);
    }

    [Fact]
    public async Task UpdatePlanillaDetalle_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlanillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Detalle de planilla no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlanillaDetalle(20, 25, new PlanillaDetalleRequest(101, 201, 5, 50, 40, 90), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePlanillaDetalle_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlanillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePlanillaDetalle(20, 25, new PlanillaDetalleRequest(101, 201, 5, 50, 40, 90), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 25L);
    }

    [Fact]
    public async Task DeletePlanillaDetalle_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlanillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Detalle de planilla no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlanillaDetalle(20, 25, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeletePlanillaDetalle_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlanillaDiagnosticoDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeletePlanillaDetalle(20, 25, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static PlanillasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new PlanillasController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<PlantillaDiagnostico>? plantillas = null,
        IEnumerable<PlantillaDiagnosticoDetalle>? plantillaDetalles = null,
        IEnumerable<PlanillaDiagnostico>? planillas = null,
        IEnumerable<PlanillaDiagnosticoDetalle>? planillaDetalles = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var plantillasDbSet = MockDbSetHelper.CreateMockDbSet(plantillas);
        var plantillaDetallesDbSet = MockDbSetHelper.CreateMockDbSet(plantillaDetalles);
        var planillasDbSet = MockDbSetHelper.CreateMockDbSet(planillas);
        var planillaDetallesDbSet = MockDbSetHelper.CreateMockDbSet(planillaDetalles);

        db.PlantillasDiagnostico.Returns(plantillasDbSet);
        db.PlantillasDiagnosticoDetalle.Returns(plantillaDetallesDbSet);
        db.PlanillasDiagnostico.Returns(planillasDbSet);
        db.PlanillasDiagnosticoDetalle.Returns(planillaDetallesDbSet);
        return db;
    }

    private static PlantillaDiagnostico BuildPlantilla(long id, string descripcion, DateTime fechaRegistro, string? observaciones = null)
    {
        var entity = PlantillaDiagnostico.Crear(descripcion, fechaRegistro.AddDays(-10), fechaRegistro.AddDays(10), observaciones);
        SetProperty(entity, nameof(PlantillaDiagnostico.Id), id);
        SetProperty(entity, nameof(PlantillaDiagnostico.FechaRegistro), fechaRegistro);
        return entity;
    }

    private static PlantillaDiagnosticoDetalle BuildPlantillaDetalle(long id, long plantillaId, long? variableDetalleId, decimal porcentajeIncidencia, decimal? valorObjetivo)
    {
        var entity = PlantillaDiagnosticoDetalle.Crear(plantillaId, variableDetalleId, porcentajeIncidencia, valorObjetivo);
        SetProperty(entity, nameof(PlantillaDiagnosticoDetalle.Id), id);
        return entity;
    }

    private static PlanillaDiagnostico BuildPlanilla(
        long id,
        long? clienteId,
        long? plantillaId,
        DateTime fechaRegistro,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? observaciones)
    {
        var entity = PlanillaDiagnostico.Crear(clienteId, plantillaId, 200, 300, 400, fechaRegistro.AddDays(-2), fechaDesde, fechaHasta, observaciones);
        SetProperty(entity, nameof(PlanillaDiagnostico.Id), id);
        SetProperty(entity, nameof(PlanillaDiagnostico.FechaRegistro), fechaRegistro);
        return entity;
    }

    private static PlanillaDiagnosticoDetalle BuildPlanillaDetalle(
        long id,
        long planillaId,
        long? variableDetalleId,
        long? opcionVariableId,
        decimal puntajeVariable,
        decimal valor,
        decimal porcentajeIncidencia,
        decimal? valorObjetivo)
    {
        var entity = PlanillaDiagnosticoDetalle.Crear(planillaId, variableDetalleId, opcionVariableId, puntajeVariable, valor, porcentajeIncidencia, valorObjetivo);
        SetProperty(entity, nameof(PlanillaDiagnosticoDetalle.Id), id);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object? expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}