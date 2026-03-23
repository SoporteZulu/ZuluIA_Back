using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;
using ZuluIA_Back.Application.Features.Extras.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class BusquedasControllerTests
{
    [Fact]
    public async Task GetAll_SinUsuarioId_DevuelveSoloGlobalesDelModuloNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var busquedas = MockDbSetHelper.CreateMockDbSet([
            BuildBusqueda(1, "Global A", "ventas", "{}", null, true),
            BuildBusqueda(2, "Privada", "ventas", "{}", 7, false),
            BuildBusqueda(3, "Otro modulo", "compras", "{}", null, true)
        ]);
        db.Busquedas.Returns(busquedas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll("  VENTAS  ", null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<List<BusquedaDto>>().Subject;
        data.Should().HaveCount(1);
        data[0].Id.Should().Be(1);
        data[0].Nombre.Should().Be("Global A");
    }

    [Fact]
    public async Task GetAll_ConUsuarioId_DevuelveGlobalesYDelUsuario()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var busquedas = MockDbSetHelper.CreateMockDbSet([
            BuildBusqueda(1, "Global A", "ventas", "{}", null, true),
            BuildBusqueda(2, "Mia", "ventas", "{}", 7, false),
            BuildBusqueda(3, "Ajena", "ventas", "{}", 9, false)
        ]);
        db.Busquedas.Returns(busquedas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll("ventas", 7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<List<BusquedaDto>>().Subject;
        data.Should().HaveCount(2);
        data.Select(item => item.Id).Should().BeEquivalentTo([1L, 2L]);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateBusquedaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Nombre requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateBusquedaRequest("", "ventas", "{}", 7, false), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConIdYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateBusquedaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));
        var controller = CreateController(mediator, db);
        var request = new CreateBusquedaRequest("Favorita", "ventas", "{\"q\":1}", 7, false);

        var result = await controller.Create(request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 10L);
        await mediator.Received(1).Send(
            Arg.Is<CreateBusquedaCommand>(command =>
                command.Nombre == "Favorita" &&
                command.Modulo == "ventas" &&
                command.FiltrosJson == "{\"q\":1}" &&
                command.UsuarioId == 7 &&
                !command.EsGlobal),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateBusquedaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Búsqueda no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(4, new UpdateBusquedaRequest("Nueva", "{}", true), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateBusquedaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(4, new UpdateBusquedaRequest("Nueva", "{}", true), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Búsqueda actualizada correctamente.");
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteBusquedaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Búsqueda no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteBusquedaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Búsqueda eliminada correctamente.");
    }

    private static BusquedasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new BusquedasController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Busqueda BuildBusqueda(long id, string nombre, string modulo, string filtrosJson, long? usuarioId, bool esGlobal)
    {
        var entity = Busqueda.Crear(nombre, modulo, filtrosJson, usuarioId, esGlobal);
        SetProperty(entity, nameof(Busqueda.Id), id);
        SetProperty(entity, nameof(Busqueda.CreatedAt), new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero));
        SetProperty(entity, nameof(Busqueda.UpdatedAt), new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero));
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