using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.RRHH.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class EmpleadosAreasPerfilesControllerTests
{
    [Fact]
    public async Task GetAreas_CuandoHayDatos_DevuelveOkOrdenadoYFiltrado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var areas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildEmpleadoXArea(1, 7, 11, 2),
            BuildEmpleadoXArea(2, 7, 12, 1),
            BuildEmpleadoXArea(3, 8, 13, 0)
        });
        db.EmpleadosXArea.Returns(areas);
        var controller = CreateController(mediator, db: db);

        var result = await controller.GetAreas(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "AreaId", 12L);
        AssertAnonymousProperty(items[0], "Orden", 1);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task AddArea_CuandoEmpleadoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddEmpleadoAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Empleado 7 no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.AddArea(7, new AsignarAreaEmpleadoRequest(11, 3), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Empleado 7 no encontrado");
    }

    [Fact]
    public async Task AddArea_CuandoYaExisteAsignacion_DevuelveBadRequestConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddEmpleadoAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El empleado ya esta asignado a esa area."));
        var controller = CreateController(mediator);

        var result = await controller.AddArea(7, new AsignarAreaEmpleadoRequest(11, 3), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está asignado a esa área");
    }

    [Fact]
    public async Task AddArea_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddEmpleadoAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.AddArea(7, new AsignarAreaEmpleadoRequest(11, 3), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(EmpleadosController.GetAreas));
        AssertAnonymousProperty(created.Value!, "Id", 21L);
    }

    [Fact]
    public async Task RemoveArea_CuandoNoExiste_DevuelveNotFoundConMensajeEsperado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemoveEmpleadoAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Asignacion no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.RemoveArea(7, 4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Asignación no encontrada");
    }

    [Fact]
    public async Task RemoveArea_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemoveEmpleadoAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.RemoveArea(7, 4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetPerfilesDeArea_CuandoHayDatos_DevuelveOkOrdenadoYFiltrado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var perfilesAsignados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildEmpleadoXPerfil(1, 3, 101, 2),
            BuildEmpleadoXPerfil(2, 3, 102, 1),
            BuildEmpleadoXPerfil(3, 4, 103, 0)
        });
        db.EmpleadosXPerfil.Returns(perfilesAsignados);
        var controller = CreateController(mediator, db: db);

        var result = await controller.GetPerfilesDeArea(7, 3, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "PerfilId", 102L);
        AssertAnonymousProperty(items[0], "Orden", 1);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task AddPerfil_CuandoNoExisteAsignacion_DevuelveNotFoundConMensajeEsperado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddEmpleadoPerfilCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Asignacion area-empleado no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.AddPerfil(7, 3, new AsignarPerfilRequest(102, 1), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Asignación área-empleado no encontrada");
    }

    [Fact]
    public async Task AddPerfil_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddEmpleadoPerfilCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(31L));
        var controller = CreateController(mediator);

        var result = await controller.AddPerfil(7, 3, new AsignarPerfilRequest(102, 1), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(EmpleadosController.GetPerfilesDeArea));
        AssertAnonymousProperty(created.Value!, "Id", 31L);
    }

    [Fact]
    public async Task RemovePerfil_CuandoNoExiste_DevuelveNotFoundConMensajeEsperado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemoveEmpleadoPerfilCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Asignacion de perfil no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.RemovePerfil(7, 3, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Asignación de perfil no encontrada");
    }

    [Fact]
    public async Task RemovePerfil_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemoveEmpleadoPerfilCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.RemovePerfil(7, 3, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetPerfiles_CuandoHayDatos_DevuelveOkOrdenadoPorCodigo()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var perfiles = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildPerfil(2, "VEN", "Ventas"),
            BuildPerfil(1, "ADM", "Administracion")
        });
        db.Perfiles.Returns(perfiles);
        var controller = CreateController(mediator, db: db);

        var result = await controller.GetPerfiles(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Codigo", "ADM");
        AssertAnonymousProperty(items[1], "Id", 2L);
    }

    private static EmpleadosController CreateController(
        IMediator mediator,
        IEmpleadoRepository? repo = null,
        IApplicationDbContext? db = null)
    {
        var controller = new EmpleadosController(
            mediator,
            repo ?? Substitute.For<IEmpleadoRepository>(),
            db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static EmpleadoXArea BuildEmpleadoXArea(long id, long empleadoId, long areaId, int orden)
    {
        var entity = EmpleadoXArea.Crear(empleadoId, areaId, orden);
        typeof(EmpleadoXArea).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static EmpleadoXPerfil BuildEmpleadoXPerfil(long id, long empleadoAreaId, long perfilId, int orden)
    {
        var entity = EmpleadoXPerfil.Crear(empleadoAreaId, perfilId, orden);
        typeof(EmpleadoXPerfil).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static Perfil BuildPerfil(long id, string codigo, string descripcion)
    {
        var entity = Perfil.Crear(codigo, descripcion);
        typeof(Perfil).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}