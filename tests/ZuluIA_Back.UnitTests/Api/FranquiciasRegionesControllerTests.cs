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
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class FranquiciasRegionesControllerTests
{
    [Fact]
    public async Task GetAll_CuandoFiltraPorSucursal_DevuelveSoloCoincidencias()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var relaciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildFranquiciaRegion(1, 10, 100, 1000),
            BuildFranquiciaRegion(2, 10, 101, null),
            BuildFranquiciaRegion(3, 11, 102, null)
        });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildSucursal(10, "Sucursal A"),
            BuildSucursal(11, "Sucursal B")
        });
        var regiones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildRegion(100, "R1", "Region 1"),
            BuildRegion(101, "R2", "Region 2"),
            BuildRegion(102, "R3", "Region 3")
        });
        var grupos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildGrupo(1000, "GE", "Grupo")
        });
        db.FranquiciasXRegiones.Returns(relaciones);
        db.Sucursales.Returns(sucursales);
        db.Regiones.Returns(regiones);
        db.GrupoEconomicos.Returns(grupos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(10, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "SucursalId", 10L);
        AssertAnonymousProperty(items[1], "SucursalId", 10L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var relaciones = MockDbSetHelper.CreateMockDbSet(Array.Empty<FranquiciaXRegion>());
        db.FranquiciasXRegiones.Returns(relaciones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Asignacion franquicia-region 9 no encontrada");
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<CreateFranquiciaXRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una asignacion para esa sucursal y region."));

        var result = await controller.Create(new FranquiciaRegionRequest(10, 100, 1000), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRouteYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<CreateFranquiciaXRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));

        var result = await controller.Create(new FranquiciaRegionRequest(10, 100, 1000), CancellationToken.None);

        result.Should().BeOfType<CreatedAtRouteResult>();
        await mediator.Received(1).Send(
            Arg.Is<CreateFranquiciaXRegionCommand>(x => x.SucursalId == 10 && x.RegionId == 100 && x.GrupoEconomicoId == 1000),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<UpdateFranquiciaXRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Asignacion franquicia-region 5 no encontrada."));

        var result = await controller.Update(5, new FranquiciaRegionRequest(10, 100, 1000), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<UpdateFranquiciaXRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(5, new FranquiciaRegionRequest(10, 100, 1000), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateFranquiciaXRegionCommand>(x => x.Id == 5 && x.SucursalId == 10 && x.RegionId == 100 && x.GrupoEconomicoId == 1000),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IApplicationDbContext>());
        mediator.Send(Arg.Any<DeleteFranquiciaXRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Delete(5, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteFranquiciaXRegionCommand>(x => x.Id == 5),
            Arg.Any<CancellationToken>());
    }

    private static FranquiciasRegionesController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new FranquiciasRegionesController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static FranquiciaXRegion BuildFranquiciaRegion(long id, long sucursalId, long regionId, long? grupoId)
    {
        var entity = FranquiciaXRegion.Crear(sucursalId, regionId, grupoId);
        SetEntityId(entity, id);
        return entity;
    }

    private static GrupoEconomico BuildGrupo(long id, string codigo, string descripcion)
    {
        var entity = GrupoEconomico.Crear(codigo, descripcion, null);
        SetEntityId(entity, id);
        return entity;
    }

    private static Region BuildRegion(long id, string codigo, string descripcion)
    {
        var entity = Region.Crear(codigo, descripcion, null, 0, 0, null, false, null);
        SetEntityId(entity, id);
        return entity;
    }

    private static Sucursal BuildSucursal(long id, string razonSocial)
    {
        var entity = Sucursal.Crear(razonSocial, $"30{id:000000000}", 1, 1, 1, false, null);
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