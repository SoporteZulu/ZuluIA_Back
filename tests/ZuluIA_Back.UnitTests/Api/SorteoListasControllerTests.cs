using System.Collections;
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
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class SorteoListasControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenDescendente()
    {
        SorteoLista[] sorteos =
        [
            BuildSorteo(1, 10, 20, "Uno", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), true),
            BuildSorteo(3, 10, 20, "Tres", new DateOnly(2026, 3, 3), new DateOnly(2026, 4, 2), true),
            BuildSorteo(2, 10, 21, "Dos", new DateOnly(2026, 3, 2), new DateOnly(2026, 4, 1), false),
            BuildSorteo(4, 11, 20, "Cuatro", new DateOnly(2026, 3, 4), new DateOnly(2026, 4, 3), true)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(sorteos: sorteos));

        var result = await controller.GetAll(10, 20, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 3L);
        AssertAnonymousProperty(items[0], "Descripcion", "Tres");
        AssertAnonymousProperty(items[1], "Id", 1L);
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
        var sorteo = BuildSorteo(5, 10, 20, "Promo Marzo", new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 31), true);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(sorteos: [sorteo]));

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Promo Marzo");
        AssertAnonymousProperty(ok.Value!, "Activa", true);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSorteoListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripcion es obligatoria."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Create(new CrearSorteoListaRequest(10, 20, " ", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La descripcion es obligatoria.");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSorteoListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(8L));
        var controller = CreateController(mediator, BuildDb());
        var request = new CrearSorteoListaRequest(10, 20, "Promo", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        var result = await controller.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetSorteoListaById");
        created.RouteValues!["id"].Should().Be(8L);
        AssertAnonymousProperty(created.Value!, "id", 8L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSorteoListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Sorteo 7 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Update(7, new ActualizarSorteoListaRequest("Promo", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)), CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Sorteo 7 no encontrado.");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSorteoListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Update(7, new ActualizarSorteoListaRequest("Promo", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
    }

    [Fact]
    public async Task Cerrar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CloseSorteoListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Sorteo 9 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Cerrar(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cerrar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CloseSorteoListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Cerrar(9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetParticipantes_OrdenaPorTicket()
    {
        SorteoListaXCliente[] participantes =
        [
            BuildParticipante(1, 5, 100, 20, false),
            BuildParticipante(2, 5, 101, 10, true),
            BuildParticipante(3, 6, 102, 5, false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(participantes: participantes));

        var result = await controller.GetParticipantes(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "NroTicket", 10);
        AssertAnonymousProperty(items[0], "Ganador", true);
        AssertAnonymousProperty(items[1], "NroTicket", 20);
    }

    [Fact]
    public async Task AddParticipante_CuandoNoExisteSorteo_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddParticipanteSorteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Sorteo 5 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddParticipante(5, new InscribirParticipanteRequest(100, 10), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddParticipante_CuandoTicketDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddParticipanteSorteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe ese numero de ticket en el sorteo."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddParticipante(5, new InscribirParticipanteRequest(100, 10), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task AddParticipante_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddParticipanteSorteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddParticipante(5, new InscribirParticipanteRequest(100, 10), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(SorteoListasController.GetParticipantes));
        created.RouteValues!["id"].Should().Be(5L);
        AssertAnonymousProperty(created.Value!, "Id", 12L);
    }

    [Fact]
    public async Task MarcarGanador_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<MarkSorteoParticipanteGanadorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SorteoParticipanteGanadorResult>("Participante no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.MarcarGanador(5, 12, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task MarcarGanador_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<MarkSorteoParticipanteGanadorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SorteoParticipanteGanadorResult(12, true)));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.MarcarGanador(5, 12, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 12L);
        AssertAnonymousProperty(ok.Value!, "Ganador", true);
    }

    [Fact]
    public async Task GetTipos_AplicaFiltroActivoYOrdenaPorCodigo()
    {
        SorteoListaTipo[] tipos =
        [
            BuildTipo(2, "B", "Beta", true),
            BuildTipo(1, "A", "Alfa", true),
            BuildTipo(3, "C", "Gamma", false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(tipos: tipos));

        var result = await controller.GetTipos(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task CreateTipo_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSorteoListaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un tipo con ese codigo."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateTipo(new CrearSorteoTipoRequest("A", "Alfa"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CreateTipo_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSorteoListaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateTipo(new CrearSorteoTipoRequest("A", "Alfa"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(SorteoListasController.GetTipos));
        AssertAnonymousProperty(created.Value!, "Id", 14L);
    }

    [Fact]
    public async Task UpdateTipo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSorteoListaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo 8 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateTipo(8, new ActualizarSorteoTipoRequest("Nuevo"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateTipo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSorteoListaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateTipo(8, new ActualizarSorteoTipoRequest("Nuevo"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task DesactivarTipo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateSorteoListaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DesactivarTipo(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ActivarTipo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateSorteoListaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.ActivarTipo(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static SorteoListasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new SorteoListasController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<SorteoLista>? sorteos = null,
        IEnumerable<SorteoListaXCliente>? participantes = null,
        IEnumerable<SorteoListaTipo>? tipos = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var sorteosDbSet = MockDbSetHelper.CreateMockDbSet(sorteos);
        var participantesDbSet = MockDbSetHelper.CreateMockDbSet(participantes);
        var tiposDbSet = MockDbSetHelper.CreateMockDbSet(tipos);
        db.SorteosLista.Returns(sorteosDbSet);
        db.SorteosListaXCliente.Returns(participantesDbSet);
        db.SorteosListaTipos.Returns(tiposDbSet);
        return db;
    }

    private static SorteoLista BuildSorteo(long id, long sucursalId, long tipoId, string descripcion, DateOnly fechaInicio, DateOnly fechaFin, bool activa)
    {
        var entity = SorteoLista.Crear(sucursalId, tipoId, descripcion, fechaInicio, fechaFin, 7);
        SetProperty(entity, nameof(SorteoLista.Id), id);
        if (!activa)
        {
            entity.Cerrar(7);
        }
        return entity;
    }

    private static SorteoListaXCliente BuildParticipante(long id, long sorteoListaId, long terceroId, int nroTicket, bool ganador)
    {
        var entity = SorteoListaXCliente.Inscribir(sorteoListaId, terceroId, nroTicket);
        SetProperty(entity, nameof(SorteoListaXCliente.Id), id);
        if (ganador)
            entity.MarcarGanador();
        return entity;
    }

    private static SorteoListaTipo BuildTipo(long id, string codigo, string descripcion, bool activo)
    {
        var entity = SorteoListaTipo.Crear(codigo, descripcion, 7);
        SetProperty(entity, nameof(SorteoListaTipo.Id), id);
        if (!activo)
            entity.Desactivar(7);
        return entity;
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