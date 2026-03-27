using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Franquicias.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Franquicias;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class FranquiciasVariablesUsuariosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoFiltraPorPlanTrabajo_DevuelveSoloCoincidencias()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var asignaciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildAsignacion(1, 10, 100, 1000, "A"),
            BuildAsignacion(2, 10, 101, 1001, "B"),
            BuildAsignacion(3, 11, 100, 1000, "C")
        });
        var usuarios = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildUsuario(100, "ana", "Ana"),
            BuildUsuario(101, "bruno", "Bruno")
        });
        var variables = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildVariable(1000, "V1", "Variable 1"),
            BuildVariable(1001, "V2", "Variable 2")
        });
        db.FranquiciasVariablesXUsuarios.Returns(asignaciones);
        db.Usuarios.Returns(usuarios);
        db.Variables.Returns(variables);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(10, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "PlanTrabajoId", 10L);
        AssertAnonymousProperty(items[1], "PlanTrabajoId", 10L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var asignaciones = MockDbSetHelper.CreateMockDbSet(Array.Empty<FranquiciaVariableXUsuario>());
        db.FranquiciasVariablesXUsuarios.Returns(asignaciones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(10, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<CreateFranquiciaVariableXUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una asignacion para ese plan, usuario y variable."));

        var result = await controller.Create(10, new FranquiciaVariableUsuarioRequest(100, 1000, "A"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRouteYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<CreateFranquiciaVariableXUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));

        var result = await controller.Create(10, new FranquiciaVariableUsuarioRequest(100, 1000, "A"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtRouteResult>();
        await mediator.Received(1).Send(
            Arg.Is<CreateFranquiciaVariableXUsuarioCommand>(x => x.PlanTrabajoId == 10 && x.UsuarioId == 100 && x.VariableId == 1000 && x.Valor == "A"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<UpdateFranquiciaVariableXUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(10, 5, new UpdateFranquiciaVariableUsuarioRequest("B"), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<UpdateFranquiciaVariableXUsuarioCommand>(x => x.Id == 5 && x.Valor == "B"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<DeleteFranquiciaVariableXUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Delete(10, 5, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteFranquiciaVariableXUsuarioCommand>(x => x.Id == 5),
            Arg.Any<CancellationToken>());
    }

    private static FranquiciasVariablesUsuariosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new FranquiciasVariablesUsuariosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static FranquiciaVariableXUsuario BuildAsignacion(long id, long planTrabajoId, long usuarioId, long variableId, string valor)
    {
        var entity = FranquiciaVariableXUsuario.Crear(planTrabajoId, usuarioId, variableId, valor, null);
        SetEntityId(entity, id);
        return entity;
    }

    private static Usuario BuildUsuario(long id, string userName, string? nombreCompleto)
    {
        var entity = Usuario.Crear(userName, nombreCompleto, null, null, null);
        SetEntityId(entity, id);
        return entity;
    }

    private static Variable BuildVariable(long id, string codigo, string descripcion)
    {
        var entity = Variable.Crear(codigo, descripcion);
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