using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Application.Features.Usuarios.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class UsuariosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUsuariosPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<UsuarioListDto>(
            [
                new UsuarioListDto { Id = 1, UserName = "ada", NombreCompleto = "Ada Lovelace", Email = "ada@example.com", Activo = true },
                new UsuarioListDto { Id = 2, UserName = "grace", NombreCompleto = "Grace Hopper", Email = "grace@example.com", Activo = false }
            ], 2, 5, 12));
        var controller = CreateController(mediator);

        var result = await controller.GetAll(2, 5, "a", true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var paged = ok.Value.Should().BeOfType<PagedResult<UsuarioListDto>>().Subject;
        paged.Items.Should().HaveCount(2);
        paged.TotalPages.Should().Be(3);
        await mediator.Received(1).Send(
            Arg.Is<GetUsuariosPagedQuery>(q => q.Page == 2 && q.PageSize == 5 && q.Search == "a" && q.SoloActivos == true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUsuarioByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((UsuarioDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUsuarioByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(new UsuarioDto
            {
                Id = 7,
                UserName = "ada",
                NombreCompleto = "Ada Lovelace",
                Email = "ada@example.com",
                Activo = true,
                SucursalIds = [1, 2]
            });
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<UsuarioDto>().Subject;
        dto.Id.Should().Be(7);
        dto.SucursalIds.Should().Equal(1, 2);
    }

    [Fact]
    public async Task GetMenu_CuandoHayDatos_DevuelveOkConItems()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetMenuUsuarioQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<MenuItemDto>
            {
                new() { Id = 1, Descripcion = "Ventas", Nivel = 1, Orden = 1, Activo = true, Hijos = [] }
            });
        var controller = CreateController(mediator);

        var result = await controller.GetMenu(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<MenuItemDto>>().Subject;
        items.Should().ContainSingle();
        items[0].Descripcion.Should().Be("Ventas");
    }

    [Fact]
    public async Task GetPermisos_CuandoHayDatos_DevuelveOkConPermisos()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPermisosUsuarioQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<PermisoDto>
            {
                new() { SeguridadId = 3, Identificador = "USR_EDIT", Descripcion = "Editar usuarios", Valor = true, AplicaSeguridadPorUsuario = true }
            });
        var controller = CreateController(mediator);

        var result = await controller.GetPermisos(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<PermisoDto>>().Subject;
        items.Should().ContainSingle();
        items[0].Identificador.Should().Be("USR_EDIT");
    }

    [Fact]
    public async Task GetParametros_CuandoHayDatos_DevuelveOkConProyeccion()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IParametroUsuarioRepository>();
        repo.GetByUsuarioAsync(7, Arg.Any<CancellationToken>())
            .Returns([
                BuildParametroUsuario(1, 7, "tema", "claro"),
                BuildParametroUsuario(2, 7, "zona", "AR")
            ]);
        var controller = CreateController(mediator);

        var result = await controller.GetParametros(7, repo, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "UsuarioId", 7L);
        AssertAnonymousProperty(items[0], "Clave", "TEMA");
        AssertAnonymousProperty(items[0], "Valor", "claro");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El nombre de usuario ya existe."));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateUsuarioCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("nombre de usuario ya existe");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateUsuarioCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetUsuarioById");
        AssertAnonymousProperty(created.Value!, "id", 15L);
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequestSinInvocarMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateUsuarioCommand(8), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ID de la URL no coincide");
        await mediator.DidNotReceive().Send(Arg.Any<UpdateUsuarioCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el usuario con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateUsuarioCommand(7), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el usuario con ID 7");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateUsuarioCommand(7), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el usuario con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el usuario con ID 7");
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetMenu_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SetMenuUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El usuario no existe."));
        var controller = CreateController(mediator);

        var result = await controller.SetMenu(7, new SetMenuRequest([1, 2, 3]), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("usuario no existe");
    }

    [Fact]
    public async Task SetMenu_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SetMenuUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.SetMenu(7, new SetMenuRequest([1, 2, 3]), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetPermiso_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SetPermisoUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Permiso invalido."));
        var controller = CreateController(mediator);

        var result = await controller.SetPermiso(7, 3, new SetPermisoRequest(true), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Permiso invalido");
    }

    [Fact]
    public async Task SetPermiso_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SetPermisoUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.SetPermiso(7, 3, new SetPermisoRequest(true), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetParametro_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SetParametroUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La clave es requerida."));
        var controller = CreateController(mediator);

        var result = await controller.SetParametro(7, new SetParametroRequest("", "x"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("clave es requerida");
    }

    [Fact]
    public async Task SetParametro_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SetParametroUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.SetParametro(7, new SetParametroRequest("tema", "oscuro"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static UsuariosController CreateController(IMediator mediator)
    {
        var controller = new UsuariosController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CreateUsuarioCommand BuildCreateUsuarioCommand()
        => new("ada", "Ada Lovelace", "ada@example.com", Guid.Empty, "Secret123!", [1, 2]);

    private static UpdateUsuarioCommand BuildUpdateUsuarioCommand(long id)
        => new(id, "Ada Lovelace", "ada@example.com", [1, 2]);

    private static ParametroUsuario BuildParametroUsuario(long id, long usuarioId, string clave, string? valor)
    {
        var entity = ParametroUsuario.Crear(usuarioId, clave, valor);
        SetEntityId(entity, id);
        return entity;
    }

    private static void SetEntityId(object entity, long id)
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var property = type.GetProperty("Id");
            if (property is not null)
            {
                property.SetValue(entity, id);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("No se pudo localizar la propiedad Id.");
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}