using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CubosControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenaPorDescripcion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet([
            BuildCubo(1, "Zeta", ambienteId: 2, esSistema: true, usuarioAltaId: 90),
            BuildCubo(2, "Alfa", ambienteId: 2, esSistema: false, usuarioAltaId: 50),
            BuildCubo(3, "Beta", ambienteId: 2, esSistema: false, usuarioAltaId: 90),
            BuildCubo(4, "Gamma", ambienteId: 1, esSistema: true, usuarioAltaId: 10)
        ]);
        db.Cubos.Returns(cubos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(90, 2, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 3L);
        AssertAnonymousProperty(data[0], "Descripcion", "Beta");
        AssertAnonymousProperty(data[1], "Id", 1L);
        AssertAnonymousProperty(data[1], "Descripcion", "Zeta");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet(new List<Cubo>());
        db.Cubos.Returns(cubos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(44, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveCuboConCamposYFiltrosOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet([
            BuildCubo(10, "Ventas", ambienteId: 2, menuCuboId: 5, formatoId: 7, usuarioAltaId: 9, cuboOrigenId: 3, observacion: "obs")
        ]);
        db.Cubos.Returns(cubos);
        var camposDbSet = MockDbSetHelper.CreateMockDbSet([
            BuildCampo(2, 10, "cliente", "Cliente", orden: 2, ubicacion: 1, posicion: 2, visible: true, calculado: false, filtro: null, campoPadreId: null, tipoOrden: 2, funcionAgregado: null, varName: "cli"),
            BuildCampo(1, 10, "total", "Total", orden: 1, ubicacion: 2, posicion: 1, visible: true, calculado: true, filtro: ">0", campoPadreId: null, tipoOrden: 1, funcionAgregado: 4, varName: "tot")
        ]);
        db.CubosCampos.Returns(camposDbSet);
        var filtrosDbSet = MockDbSetHelper.CreateMockDbSet([
            BuildFiltro(3, 10, "estado='A'", orden: 2, operador: 2),
            BuildFiltro(1, 10, "anio=2026", orden: 1, operador: 1)
        ]);
        db.CubosFiltros.Returns(filtrosDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var cubo = ok.Value!.GetType().GetProperty("Cubo")!.GetValue(ok.Value)!;
        AssertAnonymousProperty(cubo, "Id", 10L);
        AssertAnonymousProperty(cubo, "Descripcion", "Ventas");
        AssertAnonymousProperty(cubo, "Observacion", "obs");

        var campos = ok.Value!.GetType().GetProperty("Campos")!.GetValue(ok.Value)
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        campos.Should().HaveCount(2);
        AssertAnonymousProperty(campos[0], "Id", 1L);
        AssertAnonymousProperty(campos[0], "Descripcion", "Total");
        AssertAnonymousProperty(campos[1], "Id", 2L);

        var filtros = ok.Value!.GetType().GetProperty("Filtros")!.GetValue(ok.Value)
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        filtros.Should().HaveCount(2);
        AssertAnonymousProperty(filtros[0], "Id", 1L);
        AssertAnonymousProperty(filtros[0], "Filtro", "anio=2026");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCuboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(
            new CuboRequest("Ventas", "select * from ventas", "obs", 2, 5, 3, true, 7, 9),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CubosController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 12L);
        await mediator.Received(1).Send(
            Arg.Is<CreateCuboCommand>(command =>
                command.Descripcion == "Ventas" &&
                command.OrigenDatos == "select * from ventas" &&
                command.Observacion == "obs" &&
                command.AmbienteId == 2 &&
                command.MenuCuboId == 5 &&
                command.CuboOrigenId == 3 &&
                command.EsSistema == true &&
                command.FormatoId == 7 &&
                command.UsuarioAltaId == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCuboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CuboRequest(string.Empty), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCuboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Cubo no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(5, new CuboRequest("Ventas"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCuboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La descripción es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(5, new CuboRequest(string.Empty), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCuboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(
            5,
            new CuboRequest("Ventas actualizadas", "select 1", "obs", 3, 8, 99, false, 11, 13),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateCuboCommand>(command =>
                command.Id == 5 &&
                command.Descripcion == "Ventas actualizadas" &&
                command.OrigenDatos == "select 1" &&
                command.Observacion == "obs" &&
                command.AmbienteId == 3 &&
                command.MenuCuboId == 8 &&
                command.FormatoId == 11),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCuboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteCuboCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCuboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Cubo no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCampos_DevuelveOrdenadosPorOrden()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var camposDbSet = MockDbSetHelper.CreateMockDbSet([
            BuildCampo(2, 10, "cliente", "Cliente", orden: 2),
            BuildCampo(1, 10, "total", "Total", orden: 1),
            BuildCampo(3, 11, "otro", "Otro", orden: 1)
        ]);
        db.CubosCampos.Returns(camposDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCampos(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Descripcion", "Total");
        AssertAnonymousProperty(data[1], "Id", 2L);
    }

    [Fact]
    public async Task AddCampo_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddCuboCampoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        var controller = CreateController(mediator, db);

        var result = await controller.AddCampo(
            10,
            new CuboCampoRequest("total", "Total", 2, 1, true, false, ">0", 4, 3, 2, 5),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CubosController.GetCampos));
        AssertAnonymousProperty(created.Value!, "Id", 14L);
        await mediator.Received(1).Send(
            Arg.Is<AddCuboCampoCommand>(command =>
                command.CuboId == 10 &&
                command.SourceName == "total" &&
                command.Descripcion == "Total" &&
                command.Ubicacion == 2 &&
                command.Posicion == 1 &&
                command.Visible == true &&
                command.Calculado == false &&
                command.Filtro == ">0" &&
                command.CampoPadreId == 4 &&
                command.Orden == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddCampo_CuandoNoExisteCubo_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddCuboCampoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Cubo no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.AddCampo(10, new CuboCampoRequest("total"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddCampo_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddCuboCampoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("SourceName es requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.AddCampo(10, new CuboCampoRequest(string.Empty), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateCampo_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCuboCampoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateCampo(
            10,
            14,
            new CuboCampoRequest("total", "Total", 2, 1, true, true, ">0", 4, 3, 2, 5),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 14L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateCuboCampoCommand>(command =>
                command.CuboId == 10 &&
                command.CampoId == 14 &&
                command.Descripcion == "Total" &&
                command.Ubicacion == 2 &&
                command.Posicion == 1 &&
                command.Visible == true &&
                command.Calculado == true &&
                command.Filtro == ">0" &&
                command.CampoPadreId == 4 &&
                command.Orden == 3 &&
                command.TipoOrden == 2 &&
                command.FuncionAgregado == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCampo_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCuboCampoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Campo no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateCampo(10, 14, new CuboCampoRequest("total"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteCampo_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCuboCampoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteCampo(10, 14, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteCuboCampoCommand>(command => command.CuboId == 10 && command.CampoId == 14),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCampo_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCuboCampoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Campo no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteCampo(10, 14, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetFiltros_DevuelveOrdenadosPorOrden()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var filtrosDbSet = MockDbSetHelper.CreateMockDbSet([
            BuildFiltro(2, 10, "estado='A'", orden: 2, operador: 2),
            BuildFiltro(1, 10, "anio=2026", orden: 1, operador: 1),
            BuildFiltro(3, 11, "otro=1", orden: 1, operador: 1)
        ]);
        db.CubosFiltros.Returns(filtrosDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetFiltros(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Filtro", "anio=2026");
        AssertAnonymousProperty(data[1], "Id", 2L);
    }

    [Fact]
    public async Task AddFiltro_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddCuboFiltroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(22L));
        var controller = CreateController(mediator, db);

        var result = await controller.AddFiltro(10, new CuboFiltroRequest("anio=2026", 2, 3), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CubosController.GetFiltros));
        AssertAnonymousProperty(created.Value!, "Id", 22L);
        await mediator.Received(1).Send(
            Arg.Is<AddCuboFiltroCommand>(command =>
                command.CuboId == 10 &&
                command.Filtro == "anio=2026" &&
                command.Operador == 2 &&
                command.Orden == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddFiltro_CuandoNoExisteCubo_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddCuboFiltroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Cubo no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.AddFiltro(10, new CuboFiltroRequest("anio=2026"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddFiltro_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddCuboFiltroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El filtro es requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.AddFiltro(10, new CuboFiltroRequest(string.Empty), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateFiltro_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCuboFiltroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateFiltro(10, 22, new CuboFiltroRequest("anio=2027", 1, 4), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 22L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateCuboFiltroCommand>(command =>
                command.CuboId == 10 &&
                command.FiltroId == 22 &&
                command.Filtro == "anio=2027" &&
                command.Operador == 1 &&
                command.Orden == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateFiltro_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCuboFiltroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Filtro no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateFiltro(10, 22, new CuboFiltroRequest("anio=2027"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteFiltro_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCuboFiltroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteFiltro(10, 22, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteCuboFiltroCommand>(command => command.CuboId == 10 && command.FiltroId == 22),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteFiltro_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCuboFiltroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Filtro no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteFiltro(10, 22, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static CubosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new CubosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Cubo BuildCubo(
        long id,
        string descripcion,
        int ambienteId = 1,
        long? menuCuboId = null,
        string? origenDatos = null,
        string? observacion = null,
        bool esSistema = false,
        long? formatoId = null,
        long? usuarioAltaId = null,
        long? cuboOrigenId = null)
    {
        var cubo = Cubo.Crear(descripcion, origenDatos, observacion, ambienteId, menuCuboId, cuboOrigenId, esSistema, formatoId, usuarioAltaId);
        cubo.GetType().GetProperty(nameof(Cubo.Id))!.SetValue(cubo, id);
        return cubo;
    }

    private static CuboCampo BuildCampo(
        long id,
        long cuboId,
        string sourceName,
        string? descripcion,
        int orden,
        int? ubicacion = null,
        int? posicion = null,
        bool visible = true,
        bool calculado = false,
        string? filtro = null,
        long? campoPadreId = null,
        int? tipoOrden = null,
        int? funcionAgregado = null,
        string? varName = null)
    {
        var campo = CuboCampo.Crear(cuboId, sourceName, descripcion, ubicacion, posicion, visible, calculado, filtro, campoPadreId, orden);
        campo.GetType().GetProperty(nameof(CuboCampo.Id))!.SetValue(campo, id);
        campo.Actualizar(descripcion, ubicacion, posicion, visible, calculado, filtro, campoPadreId, orden, tipoOrden, funcionAgregado);
        if (varName is not null)
            campo.GetType().GetProperty(nameof(CuboCampo.VarName))!.SetValue(campo, varName);
        return campo;
    }

    private static CuboFiltro BuildFiltro(long id, long cuboId, string filtro, int orden, int operador = 1)
    {
        var cuboFiltro = CuboFiltro.Crear(cuboId, filtro, operador, orden);
        cuboFiltro.GetType().GetProperty(nameof(CuboFiltro.Id))!.SetValue(cuboFiltro, id);
        return cuboFiltro;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}