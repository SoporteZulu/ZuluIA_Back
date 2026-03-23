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

public class ProcedimientosControllerTests
{
    [Fact]
    public async Task GetAll_SinUsuarioId_DevuelveSoloGlobalesDeProcedimientos()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var busquedas = MockDbSetHelper.CreateMockDbSet([
            BuildBusqueda(1, "Global A", "procedimientos", "{}", null, true),
            BuildBusqueda(2, "Privada", "procedimientos", "{}", 7, false),
            BuildBusqueda(3, "Otro modulo", "ventas", "{}", null, true)
        ]);
        db.Busquedas.Returns(busquedas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(1);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Nombre", "Global A");
    }

    [Fact]
    public async Task GetAll_ConUsuarioIdEIncluirGlobales_DevuelveGlobalesYDelUsuario()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var busquedas = MockDbSetHelper.CreateMockDbSet([
            BuildBusqueda(1, "Global A", "procedimientos", "{}", null, true),
            BuildBusqueda(2, "Mia", "procedimientos", "{}", 7, false),
            BuildBusqueda(3, "Ajena", "procedimientos", "{}", 9, false)
        ]);
        db.Busquedas.Returns(busquedas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(7, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        data.Select(item => (long)item.GetType().GetProperty("Id")!.GetValue(item)!).Should().BeEquivalentTo([1L, 2L]);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var busquedas = MockDbSetHelper.CreateMockDbSet([
            BuildBusqueda(1, "Global A", "procedimientos", "{}", null, true)
        ]);
        db.Busquedas.Returns(busquedas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var busquedas = MockDbSetHelper.CreateMockDbSet([
            BuildBusqueda(5, "Proc", "procedimientos", "{\"x\":1}", 7, false)
        ]);
        db.Busquedas.Returns(busquedas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Nombre", "Proc");
        AssertAnonymousProperty(ok.Value!, "DefinicionJson", "{\"x\":1}");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateProcedimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Nombre requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new ProcedimientoCreateRequest("", "{}", 7, false), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateProcedimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new ProcedimientoCreateRequest("Proc", "{}", 7, false), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ProcedimientosController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 10L);
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateProcedimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Procedimiento no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(4, new ProcedimientoUpdateRequest("Nueva", "{}", true), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateProcedimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(4, new ProcedimientoUpdateRequest("Nueva", "{}", true), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 4L);
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteProcedimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Procedimiento no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteProcedimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static ProcedimientosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ProcedimientosController(mediator, db)
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