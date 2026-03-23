using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class SeguridadControllerTests
{
    [Fact]
    public async Task GetAll_DevuelvePermisosOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ISeguridadRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildSeguridad(2, "Z_MODULO", "Modulo Z", true),
            BuildSeguridad(1, "A_MODULO", "Modulo A", false)
        ]);
        db.Seguridad.Returns(items);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Identificador", "A_MODULO");
        AssertAnonymousProperty(data[0], "Descripcion", "Modulo A");
        AssertAnonymousProperty(data[0], "AplicaSeguridadPorUsuario", false);
        AssertAnonymousProperty(data[1], "Id", 2L);
        AssertAnonymousProperty(data[1], "AplicaSeguridadPorUsuario", true);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ISeguridadRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateSeguridadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Identificador requerido."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Create(new CreateSeguridadRequest("", "Desc", true), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ISeguridadRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateSeguridadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Create(new CreateSeguridadRequest("MODULO", "Desc", true), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 9L);
        await mediator.Received(1).Send(
            Arg.Is<CreateSeguridadCommand>(command =>
                command.Identificador == "MODULO" &&
                command.Descripcion == "Desc" &&
                command.AplicaSeguridadPorUsuario),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Verificar_DevuelveEstadoDelPermiso()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ISeguridadRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.TienePermisoAsync(7, "MODULO", Arg.Any<CancellationToken>())
            .Returns(true);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Verificar(7, "MODULO", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "usuarioId", 7L);
        AssertAnonymousProperty(ok.Value!, "identificador", "MODULO");
        AssertAnonymousProperty(ok.Value!, "tienePermiso", true);
        await repo.Received(1).TienePermisoAsync(7, "MODULO", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Verificar_CuandoNoTienePermiso_DevuelveFalse()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ISeguridadRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.TienePermisoAsync(8, "OTRO", Arg.Any<CancellationToken>())
            .Returns(false);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Verificar(8, "OTRO", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "usuarioId", 8L);
        AssertAnonymousProperty(ok.Value!, "identificador", "OTRO");
        AssertAnonymousProperty(ok.Value!, "tienePermiso", false);
    }

    [Fact]
    public async Task GetPermisosUsuario_DevuelveMapaCompleto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<ISeguridadRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var permisos = new Dictionary<string, bool>
        {
            ["MODULO_A"] = true,
            ["MODULO_B"] = false
        };
        repo.GetPermisosUsuarioAsync(5, Arg.Any<CancellationToken>())
            .Returns(permisos);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetPermisosUsuario(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(permisos);
        await repo.Received(1).GetPermisosUsuarioAsync(5, Arg.Any<CancellationToken>());
    }

    private static SeguridadController CreateController(IMediator mediator, ISeguridadRepository repo, IApplicationDbContext db)
    {
        return new SeguridadController(mediator, repo, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Seguridad BuildSeguridad(long id, string identificador, string? descripcion, bool aplicaSeguridadPorUsuario)
    {
        var entity = Seguridad.Crear(identificador, descripcion, aplicaSeguridadPorUsuario);
        entity.GetType().GetProperty(nameof(Seguridad.Id))!.SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}