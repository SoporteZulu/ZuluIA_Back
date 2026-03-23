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
using ZuluIA_Back.Domain.Entities.Franquicias;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class GruposEconomicosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoSoloActivos_DevuelveFiltradosYOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var grupos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildGrupo(2, "B", "Beta", true),
            BuildGrupo(3, "C", "Gamma", false),
            BuildGrupo(1, "A", "Alfa", true)
        });
        db.GrupoEconomicos.Returns(grupos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var grupos = MockDbSetHelper.CreateMockDbSet(Array.Empty<GrupoEconomico>());
        grupos.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<GrupoEconomico?>((GrupoEconomico?)null));
        db.GrupoEconomicos.Returns(grupos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Grupo economico 9 no encontrado");
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<CreateGrupoEconomicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un grupo economico con codigo 'GE'."));

        var result = await controller.Create(new CreateGrupoEconomicoRequest("GE", "Grupo"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRouteYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<CreateGrupoEconomicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));

        var result = await controller.Create(new CreateGrupoEconomicoRequest("GE", "Grupo"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetGrupoEconomicoById");
        AssertAnonymousProperty(created.Value!, "Id", 12L);
        await mediator.Received(1).Send(
            Arg.Is<CreateGrupoEconomicoCommand>(x => x.Codigo == "GE" && x.Descripcion == "Grupo"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<UpdateGrupoEconomicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Grupo economico 7 no encontrado."));

        var result = await controller.Update(7, new UpdateGrupoEconomicoRequest("Nuevo"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<DeleteGrupoEconomicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteGrupoEconomicoCommand>(x => x.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<ActivateGrupoEconomicoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateGrupoEconomicoCommand>(x => x.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static GruposEconomicosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new GruposEconomicosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static GrupoEconomico BuildGrupo(long id, string codigo, string descripcion, bool activo)
    {
        var entity = GrupoEconomico.Crear(codigo, descripcion, null);
        if (!activo)
            entity.Desactivar(null);
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