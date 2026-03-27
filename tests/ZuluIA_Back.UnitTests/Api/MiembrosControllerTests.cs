using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Miembros.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class MiembrosControllerTests
{
    [Fact]
    public async Task GetAll_SinIncluirInactivos_DevuelveSoloClientesActivosOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var terceros = MockDbSetHelper.CreateMockDbSet([
            BuildMiembro(2, "B002", "Zulu Dos", "202", activo: true, esCliente: true),
            BuildMiembro(1, "A001", "Alpha Uno", "101", activo: true, esCliente: true),
            BuildMiembro(3, "C003", "Inactivo", "303", activo: false, esCliente: true),
            BuildMiembro(4, "D004", "No Cliente", "404", activo: true, esCliente: false)
        ]);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(false, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "MiembroId", 1L);
        AssertAnonymousProperty(data[0], "Nombre", "Alpha Uno");
        AssertAnonymousProperty(data[1], "MiembroId", 2L);
    }

    [Fact]
    public async Task GetAll_ConSearch_FiltraPorLegajoNombreODocumento()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var terceros = MockDbSetHelper.CreateMockDbSet([
            BuildMiembro(1, "A001", "Alpha Uno", "101", activo: true, esCliente: true),
            BuildMiembro(2, "B002", "Zulu Dos", "202", activo: true, esCliente: true)
        ]);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(false, " 202 ", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(1);
        AssertAnonymousProperty(data[0], "MiembroId", 2L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var terceros = MockDbSetHelper.CreateMockDbSet([
            BuildMiembro(1, "A001", "Alpha Uno", "101", activo: true, esCliente: true)
        ]);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tercero = BuildMiembro(5, "A005", "Alpha Cinco", "505", activo: true, esCliente: true);
        SetProperty(tercero, nameof(Tercero.NombreFantasia), "Fantasia Cinco");
        SetProperty(tercero, nameof(Tercero.Celular), "555-55");
        SetProperty(tercero, nameof(Tercero.Web), "https://example.test");
        var terceros = MockDbSetHelper.CreateMockDbSet([tercero]);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "MiembroId", 5L);
        AssertAnonymousProperty(ok.Value!, "Nombre", "Alpha Cinco");
        AssertAnonymousProperty(ok.Value!, "NombreFantasia", "Fantasia Cinco");
        AssertAnonymousProperty(ok.Value!, "Celular", "555-55");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Legajo requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new MiembroCreateRequest("", "Nuevo", 1, "123", 1, 3), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new MiembroCreateRequest("M010", "Nuevo", 1, "123", 1, 3), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(MiembrosController.GetById));
        AssertAnonymousProperty(created.Value!, "miembroId", 10L);
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Miembro no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El miembro ya está inactivo."));
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(4, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateMiembroCommand>(command => command.Id == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Miembro no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El miembro ya está activo."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(4, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateMiembroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static MiembrosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new MiembrosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Tercero BuildMiembro(long id, string legajo, string nombre, string nroDocumento, bool activo, bool esCliente)
    {
        var entity = Tercero.Crear(legajo, nombre, 1, nroDocumento, 1, esCliente, !esCliente, false, 3, null);
        SetProperty(entity, nameof(Tercero.Id), id);
        SetProperty(entity, nameof(Tercero.Activo), activo);
        SetProperty(entity, nameof(Tercero.Email), $"{legajo.ToLower()}@test.local");
        SetProperty(entity, nameof(Tercero.Telefono), "444-44");
        SetProperty(entity, nameof(Tercero.UpdatedAt), new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero));
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