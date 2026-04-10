using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.DTOs;
using ZuluIA_Back.Application.Features.Stock.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ConteosStockControllerTests
{
    [Fact]
    public async Task GetAll_CuandoFiltraPorEstadoYSearch_DevuelveCoincidenciasOrdenadas()
    {
        var controller = CreateController(db: BuildDb([
            BuildConteo(1, "Central", "A1", "Semanal", new DateOnly(2026, 4, 25), "programado", 1.2m, "Equipo A", "Observacion 1", "Paso 1", ""),
            BuildConteo(2, "Central", "B1", "Mensual", new DateOnly(2026, 4, 20), "observado", 5.1m, "Equipo B", "Observacion 2", "Conciliar", "Nota"),
            BuildConteo(3, "Norte", "C1", "Quincenal", new DateOnly(2026, 4, 22), "observado", 2.1m, "Equipo C", "Observacion 3", "Paso 3", "")
        ]));

        var result = await controller.GetAll("central", "observado", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<ConteoCiclicoDto>>().Subject;
        items.Should().ContainSingle();
        items[0].Id.Should().Be(2);
        items[0].Estado.Should().Be("observado");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateConteoCiclicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Create(
            new UpsertConteoCiclicoRequest("Central", "A1", "Semanal", new DateOnly(2026, 4, 25), "programado", 0m, "Equipo", "Obs", "Paso", "Nota"),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 9L);
    }

    [Fact]
    public async Task Seed_CuandoTieneNuevos_Defaults_DevuelveCantidadProcesada()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SeedConteosCiclicosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(3));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Seed(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "itemsProcesados", 3);
        AssertAnonymousProperty(ok.Value!, "mensaje", "Conteos base sembrados correctamente.");
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateConteoCiclicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el conteo con ID 7."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Update(
            7,
            new UpsertConteoCiclicoRequest("Central", "A1", "Semanal", new DateOnly(2026, 4, 25), "programado", 0m, "Equipo", "Obs", "Paso", "Nota"),
            CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteConteoCiclicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Delete(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Conteo eliminado correctamente.");
    }

    private static ConteosStockController CreateController(IMediator? mediator = null, IApplicationDbContext? db = null)
    {
        return new ConteosStockController(mediator ?? Substitute.For<IMediator>(), db ?? BuildDb([]))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(IEnumerable<ConteoCiclicoPlan> conteos)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.ConteosCiclicos.Returns(MockDbSetHelper.CreateMockDbSet(conteos));
        return db;
    }

    private static ConteoCiclicoPlan BuildConteo(
        long id,
        string deposito,
        string zona,
        string frecuencia,
        DateOnly proximoConteo,
        string estado,
        decimal divergenciaPct,
        string responsable,
        string observacion,
        string nextStep,
        string executionNote)
    {
        var entity = ConteoCiclicoPlan.Crear(
            deposito,
            zona,
            frecuencia,
            proximoConteo,
            estado,
            divergenciaPct,
            responsable,
            observacion,
            nextStep,
            executionNote);
        entity.GetType().GetProperty(nameof(ConteoCiclicoPlan.Id))!.SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
