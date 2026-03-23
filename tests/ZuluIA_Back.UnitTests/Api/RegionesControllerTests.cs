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
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class RegionesControllerTests
{
    [Fact]
    public async Task GetAll_CuandoSoloIntegradoras_DevuelveFiltradasYOrdenadas()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regiones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildRegion(2, "B", "Beta", null, 2, 0, "B", true, null),
            BuildRegion(1, "A", "Alfa", null, 1, 0, "A", true, null),
            BuildRegion(3, "C", "Gamma", 1, 1, 1, "A.1", false, null)
        });
        db.Regiones.Returns(regiones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task GetHijos_CuandoExisten_DevuelveSoloHijosDirectos()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regiones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildRegion(1, "A", "Alfa", null, 1, 0, "A", true, null),
            BuildRegion(2, "A1", "Alfa 1", 1, 2, 1, "A.1", false, null),
            BuildRegion(3, "A2", "Alfa 2", 1, 1, 1, "A.2", false, null),
            BuildRegion(4, "B1", "Beta 1", 9, 1, 1, "B.1", false, null)
        });
        db.Regiones.Returns(regiones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetHijos(1, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A2");
        AssertAnonymousProperty(items[1], "Codigo", "A1");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regiones = MockDbSetHelper.CreateMockDbSet(Array.Empty<Region>());
        regiones.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<Region?>((Region?)null));
        db.Regiones.Returns(regiones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Región 7 no encontrada");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConEntidad()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regiones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildRegion(7, "A", "Alfa", null, 1, 0, "A", true, "Obs")
        });
        regiones.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var keys = callInfo.Arg<object[]>();
                var id = (long)keys[0];
                var region = regiones.FirstOrDefault(x => x.Id == id);
                return new ValueTask<Region?>(region);
            });
        db.Regiones.Returns(regiones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var region = ok.Value.Should().BeOfType<Region>().Subject;
        region.Codigo.Should().Be("A");
        region.Descripcion.Should().Be("Alfa");
    }

    [Fact]
    public async Task GetByCodigo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regiones = MockDbSetHelper.CreateMockDbSet(Array.Empty<Region>());
        db.Regiones.Returns(regiones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetByCodigo("x", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Región 'x' no encontrada");
    }

    [Fact]
    public async Task GetByCodigo_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regiones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildRegion(7, "ABC", "Alfa", null, 1, 0, "ABC", true, null)
        });
        db.Regiones.Returns(regiones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetByCodigo("abc", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var region = ok.Value.Should().BeOfType<Region>().Subject;
        region.Id.Should().Be(7);
        region.Codigo.Should().Be("ABC");
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una región con ese código."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateRegionRequest("A", "Alfa"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe una región con ese código");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateRegionRequest("A", "Alfa"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetRegionById");
        AssertAnonymousProperty(created.Value!, "Id", 21L);
        AssertAnonymousProperty(created.Value!, "Codigo", "A");
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Región no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new UpdateRegionRequest("Alfa"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Región no encontrada");
    }

    [Fact]
    public async Task Update_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La descripción es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new UpdateRegionRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new UpdateRegionRequest("Alfa"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
    }

    [Fact]
    public async Task Delete_CuandoHaySubregiones_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La región tiene sub-regiones asociadas."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("sub-regiones asociadas");
    }

    [Fact]
    public async Task Delete_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Región no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Región no encontrada");
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteRegionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteRegionCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static RegionesController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new RegionesController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static Region BuildRegion(long id, string codigo, string descripcion, long? regionIntegradoraId, int orden, int nivel, string? codigoEstructura, bool esIntegradora, string? observacion)
    {
        var entity = Region.Crear(codigo, descripcion, regionIntegradoraId, orden, nivel, codigoEstructura, esIntegradora, observacion);
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