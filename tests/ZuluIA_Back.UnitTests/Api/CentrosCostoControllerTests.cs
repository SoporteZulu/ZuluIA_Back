using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CentrosCostoControllerTests
{
    [Fact]
    public async Task GetAll_PorDefecto_DevuelveSoloActivosOrdenadosPorCodigo()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var centros = MockDbSetHelper.CreateMockDbSet([
            BuildCentroCosto(2, "B-200", "Administracion", true),
            BuildCentroCosto(1, "A-100", "Ventas", true),
            BuildCentroCosto(3, "C-300", "Deposito", false)
        ]);
        db.CentrosCosto.Returns(centros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(ct: CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Codigo", "A-100");
        AssertAnonymousProperty(data[1], "Id", 2L);
    }

    [Fact]
    public async Task GetAll_ConSoloActivosNull_DevuelveTodos()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var centros = MockDbSetHelper.CreateMockDbSet([
            BuildCentroCosto(1, "A-100", "Ventas", true),
            BuildCentroCosto(2, "B-200", "Deposito", false)
        ]);
        db.CentrosCosto.Returns(centros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un centro de costo con ese codigo."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateCentroCostoRequest("ADM", "Administracion"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El codigo es obligatorio."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateCentroCostoRequest(string.Empty, "Administracion"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkYEnviaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateCentroCostoRequest("ADM", "Administracion"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<CreateCentroCostoCommand>(command => command.Codigo == "ADM" && command.Descripcion == "Administracion"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el centro de costo."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateCentroCostoRequest("Operaciones"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateCentroCostoRequest("Operaciones"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Centro de costo actualizado correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<UpdateCentroCostoCommand>(command => command.Id == 8 && command.Descripcion == "Operaciones"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoNoPuedeDesactivar_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede desactivar porque tiene movimientos asociados."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(5, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el centro de costo."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(5, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Centro de costo desactivado correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<DeleteCentroCostoCommand>(command => command.Id == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el centro de costo."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateCentroCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Centro de costo activado correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<ActivateCentroCostoCommand>(command => command.Id == 9),
            Arg.Any<CancellationToken>());
    }

    private static CentrosCostoController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new CentrosCostoController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CentroCosto BuildCentroCosto(long id, string codigo, string descripcion, bool activo)
    {
        var entity = CentroCosto.Crear(codigo, descripcion);
        entity.GetType().GetProperty(nameof(CentroCosto.Id))!.SetValue(entity, id);
        if (!activo)
            entity.Desactivar();

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}