using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Documentos.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class MesaEntradaControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYExcluyeArchivadosPorDefecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMesaEntrada(1, 2, 3, 4, new DateOnly(2026, 3, 20), EstadoMesaEntrada.Pendiente, 7, "ME-1", "Primero"),
            BuildMesaEntrada(2, 2, 3, 4, new DateOnly(2026, 3, 21), EstadoMesaEntrada.Archivado, 7, "ME-2", "Archivado"),
            BuildMesaEntrada(3, 2, 3, 4, new DateOnly(2026, 3, 22), EstadoMesaEntrada.Pendiente, 99, "ME-3", "Otro asignado")
        });
        db.MesasEntrada.Returns(mesas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(2, 4, "pendiente", 7, false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "EstadoFlow", "Pendiente");
        AssertAnonymousProperty(items[0], "AsignadoA", 7L);
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMesaEntrada(25, 1, 2, 3, new DateOnly(2026, 3, 20), EstadoMesaEntrada.Pendiente, 8, "ME-25", "Asunto principal", new DateOnly(2026, 3, 25), "Obs")
        });
        db.MesasEntrada.Returns(mesas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(25, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 25L);
        AssertAnonymousProperty(ok.Value!, "NroDocumento", "ME-25");
        AssertAnonymousProperty(ok.Value!, "Asunto", "Asunto principal");
        AssertAnonymousProperty(ok.Value!, "EstadoFlow", "Pendiente");
        AssertAnonymousProperty(ok.Value!, "AsignadoA", 8L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        db.MesasEntrada.Returns(mesas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("error create"));

        var result = await controller.Create(new CrearMesaEntradaRequest(1, 2, 3, "ME-1", "Asunto", new DateOnly(2026, 3, 20), null, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("error create");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(25L));

        var result = await controller.Create(new CrearMesaEntradaRequest(1, 2, 3, "ME-1", "Asunto", new DateOnly(2026, 3, 20), null, null), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(MesaEntradaController.GetById));
    }

    [Fact]
    public async Task Asignar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AssignMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<MesaEntradaAsignacionResult>("Registro 10 no encontrado."));

        var result = await controller.Asignar(10, new AsignarMesaEntradaRequest(7), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Asignar_CuandoTieneExito_DevuelveOkConAsignacion()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AssignMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new MesaEntradaAsignacionResult(10, 7)));

        var result = await controller.Asignar(10, new AsignarMesaEntradaRequest(7), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 10L);
        AssertAnonymousProperty(ok.Value!, "AsignadoA", 7L);
        await mediator.Received(1).Send(
            Arg.Is<AssignMesaEntradaCommand>(command => command.Id == 10 && command.UsuarioId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CambiarEstado_CuandoEstadoFlowEsInvalido_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ChangeMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<MesaEntradaEstadoResult>("EstadoFlow inválido."));

        var result = await controller.CambiarEstado(10, new CambiarEstadoMesaEntradaRequest(2, "XXX", null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CambiarEstado_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ChangeMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<MesaEntradaEstadoResult>("Registro 10 no encontrado."));

        var result = await controller.CambiarEstado(10, new CambiarEstadoMesaEntradaRequest(2, "Resuelto", "ok"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CambiarEstado_CuandoTieneExito_DevuelveOkConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ChangeMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new MesaEntradaEstadoResult(10, "Resuelto", 2)));

        var result = await controller.CambiarEstado(10, new CambiarEstadoMesaEntradaRequest(2, "Resuelto", "ok"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 10L);
        AssertAnonymousProperty(ok.Value!, "EstadoFlow", "Resuelto");
        AssertAnonymousProperty(ok.Value!, "EstadoId", 2L);
        await mediator.Received(1).Send(
            Arg.Is<ChangeMesaEntradaEstadoCommand>(command =>
                command.Id == 10 &&
                command.EstadoId == 2 &&
                command.EstadoFlow == "Resuelto" &&
                command.Observacion == "ok"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Archivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ArchiveMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Registro 10 no encontrado."));

        var result = await controller.Archivar(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Archivar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ArchiveMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Archivar(10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ArchiveMesaEntradaCommand>(command => command.Id == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anular_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CancelMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Registro 10 no encontrado."));

        var result = await controller.Anular(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CancelMesaEntradaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Anular(10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<CancelMesaEntradaCommand>(command => command.Id == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTipos_CuandoHayDatos_DevuelveOkConItems()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tipos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipo(2, "REC", "Recibo", true),
            BuildTipo(1, "FAC", "Factura", true)
        });
        db.MesasEntradaTipos.Returns(tipos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTipos(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "FAC");
        AssertAnonymousProperty(items[0], "Descripcion", "Factura");
        AssertAnonymousProperty(items[0], "Activo", true);
    }

    [Fact]
    public async Task GetTipos_CuandoFiltraPorActivo_DevuelveSoloCoincidencias()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tipos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipo(2, "REC", "Recibo", false),
            BuildTipo(1, "FAC", "Factura", true)
        });
        db.MesasEntradaTipos.Returns(tipos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTipos(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Activo", true);
    }

    [Fact]
    public async Task CreateTipo_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("tipo duplicado"));

        var result = await controller.CreateTipo(new MesaEntradaTipoRequest("FAC", "Factura"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("tipo duplicado");
    }

    [Fact]
    public async Task CreateTipo_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(41L));

        var result = await controller.CreateTipo(new MesaEntradaTipoRequest("FAC", "Factura"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(MesaEntradaController.GetTipos));
        await mediator.Received(1).Send(
            Arg.Is<CreateMesaEntradaTipoCommand>(command =>
                command.Codigo == "FAC" &&
                command.Descripcion == "Factura"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTipo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo 10 no encontrado."));

        var result = await controller.UpdateTipo(10, new MesaEntradaTipoUpdateRequest("Factura"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateTipo_CuandoTieneExito_DevuelveOkConIdYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.UpdateTipo(10, new MesaEntradaTipoUpdateRequest("Factura"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 10L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateMesaEntradaTipoCommand>(command =>
                command.Id == 10 &&
                command.Descripcion == "Factura"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesactivarTipo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo 10 no encontrado."));

        var result = await controller.DesactivarTipo(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DesactivarTipo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.DesactivarTipo(10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateMesaEntradaTipoCommand>(command => command.Id == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivarTipo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.ActivarTipo(10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateMesaEntradaTipoCommand>(command => command.Id == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivarTipo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMesaEntradaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo 10 no encontrado."));

        var result = await controller.ActivarTipo(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetEstados_CuandoHayDatos_DevuelveOkConItems()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var estados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildEstado(2, "RES", "Resuelto", true, true),
            BuildEstado(1, "PEN", "Pendiente", false, true)
        });
        db.MesasEntradaEstados.Returns(estados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetEstados(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "PEN");
        AssertAnonymousProperty(items[0], "Descripcion", "Pendiente");
        AssertAnonymousProperty(items[0], "EsFinal", false);
        AssertAnonymousProperty(items[0], "Activo", true);
    }

    [Fact]
    public async Task GetEstados_CuandoFiltraPorActivo_DevuelveSoloCoincidencias()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var estados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildEstado(2, "RES", "Resuelto", true, false),
            BuildEstado(1, "PEN", "Pendiente", false, true)
        });
        db.MesasEntradaEstados.Returns(estados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetEstados(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Activo", true);
    }

    [Fact]
    public async Task CreateEstado_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("estado duplicado"));

        var result = await controller.CreateEstado(new MesaEntradaEstadoRequest("PEN", "Pendiente", false), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("estado duplicado");
    }

    [Fact]
    public async Task CreateEstado_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(51L));

        var result = await controller.CreateEstado(new MesaEntradaEstadoRequest("PEN", "Pendiente", false), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(MesaEntradaController.GetEstados));
        await mediator.Received(1).Send(
            Arg.Is<CreateMesaEntradaEstadoCommand>(command =>
                command.Codigo == "PEN" &&
                command.Descripcion == "Pendiente" &&
                command.EsFinal == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateEstado_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Estado 10 no encontrado."));

        var result = await controller.UpdateEstado(10, new MesaEntradaEstadoUpdateRequest("Pendiente", false), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateEstado_CuandoTieneExito_DevuelveOkConIdYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.UpdateEstado(10, new MesaEntradaEstadoUpdateRequest("Pendiente", false), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 10L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateMesaEntradaEstadoCommand>(command =>
                command.Id == 10 &&
                command.Descripcion == "Pendiente" &&
                command.EsFinal == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesactivarEstado_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Estado 10 no encontrado."));

        var result = await controller.DesactivarEstado(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DesactivarEstado_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.DesactivarEstado(10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateMesaEntradaEstadoCommand>(command => command.Id == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivarEstado_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.ActivarEstado(10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateMesaEntradaEstadoCommand>(command => command.Id == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivarEstado_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMesaEntradaEstadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Estado 10 no encontrado."));

        var result = await controller.ActivarEstado(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static MesaEntradaController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new MesaEntradaController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static MesaEntradaTipo BuildTipo(long id, string codigo, string descripcion, bool activo)
    {
        var entity = MesaEntradaTipo.Crear(codigo, descripcion, userId: null);
        SetId(entity, id);
        if (!activo)
            entity.Desactivar(userId: null);

        return entity;
    }

    private static MesaEntrada BuildMesaEntrada(
        long id,
        long sucursalId,
        long tipoId,
        long? estadoId,
        DateOnly fechaIngreso,
        EstadoMesaEntrada estadoFlow,
        long? asignadoA,
        string nroDocumento,
        string asunto,
        DateOnly? fechaVencimiento = null,
        string? observacion = null)
    {
        var entity = MesaEntrada.Crear(sucursalId, tipoId, 5, nroDocumento, asunto, fechaIngreso, fechaVencimiento, observacion, userId: null);
        SetId(entity, id);

        if (estadoId.HasValue)
            entity.CambiarEstado(estadoId.Value, estadoFlow, observacion, userId: null);

        if (asignadoA.HasValue)
            entity.AsignarResponsable(asignadoA.Value, userId: null);

        if (estadoFlow == EstadoMesaEntrada.Archivado)
            entity.Archivar(userId: null);
        else if (estadoFlow == EstadoMesaEntrada.Anulado)
            entity.Anular(userId: null);

        return entity;
    }

    private static MesaEntradaEstado BuildEstado(long id, string codigo, string descripcion, bool esFinal, bool activo)
    {
        var entity = MesaEntradaEstado.Crear(codigo, descripcion, esFinal, userId: null);
        SetId(entity, id);
        if (!activo)
            entity.Desactivar(userId: null);

        return entity;
    }

    private static void SetId<T>(T entity, long id) where T : class
    {
        typeof(T).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}