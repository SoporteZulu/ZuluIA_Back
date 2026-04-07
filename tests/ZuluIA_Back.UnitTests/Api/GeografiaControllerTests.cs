using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

public class GeografiaControllerTests
{
    [Fact]
    public async Task GetPaises_CuandoHayDatos_DevuelveOrdenadosPorDescripcion()
    {
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(new[]
            {
                BuildPais(2, "BR", "Brasil"),
                BuildPais(1, "AR", "Argentina")
            }),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()));

        var result = await controller.GetPaises(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Argentina");
        AssertAnonymousProperty(items[1], "Descripcion", "Brasil");
    }

    [Fact]
    public async Task GetProvincias_CuandoFiltraPorPais_DevuelveSoloCoincidentes()
    {
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(new[]
            {
                BuildProvincia(2, 1, "CBA", "Cordoba"),
                BuildProvincia(1, 1, "BA", "Buenos Aires"),
                BuildProvincia(3, 2, "SP", "Sao Paulo")
            }),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()));

        var result = await controller.GetProvincias(1, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Buenos Aires");
        AssertAnonymousProperty(items[1], "Descripcion", "Cordoba");
    }

    [Fact]
    public async Task GetLocalidades_CuandoFiltraPorProvinciaYSearch_DevuelveTop100Ordenado()
    {
        var localidades = Enumerable.Range(1, 101)
            .Select(index => BuildLocalidad(index, 1, $"Villa {index:000}", $"CP{index:000}"))
            .Concat(new[]
            {
                BuildLocalidad(1000, 2, "Villa Externa", "CPX"),
                BuildLocalidad(1001, 1, "Pueblo", "CPP")
            })
            .ToArray();

        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(localidades),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()));

        var result = await controller.GetLocalidades(1, "villa", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(100);
        AssertAnonymousProperty(items[0], "Descripcion", "Villa 001");
        AssertAnonymousProperty(items[^1], "Descripcion", "Villa 100");
    }

    [Fact]
    public async Task GetLocalidades_CuandoSearchEsNulo_DevuelveTop100Ordenado()
    {
        var localidades = Enumerable.Range(1, 101)
            .Select(index => BuildLocalidad(index, 1, $"Localidad {index:000}", $"CP{index:000}"))
            .Concat(new[]
            {
                BuildLocalidad(200, 2, "Otra localidad", "CPX")
            })
            .ToArray();

        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(localidades),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()));

        var result = await controller.GetLocalidades(1, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(100);
        AssertAnonymousProperty(items[0], "Descripcion", "Localidad 001");
        AssertAnonymousProperty(items[^1], "Descripcion", "Localidad 100");
    }

    [Fact]
    public async Task GetBarrios_CuandoFiltraPorLocalidadYSearch_DevuelveCoincidentes()
    {
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(new[]
            {
                BuildBarrio(2, 1, "Centro"),
                BuildBarrio(1, 1, "Altos del Centro"),
                BuildBarrio(3, 2, "Centro Norte"),
                BuildBarrio(4, 1, "Sur")
            }));

        var result = await controller.GetBarrios(1, "centro", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Altos del Centro");
        AssertAnonymousProperty(items[1], "Descripcion", "Centro");
    }

    [Fact]
    public async Task GetBarrios_CuandoSearchEsNulo_DevuelveTop100Ordenado()
    {
        var barrios = Enumerable.Range(1, 101)
            .Select(index => BuildBarrio(index, 1, $"Barrio {index:000}"))
            .Concat(new[]
            {
                BuildBarrio(200, 2, "Barrio externo")
            })
            .ToArray();

        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(barrios));

        var result = await controller.GetBarrios(1, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(100);
        AssertAnonymousProperty(items[0], "Descripcion", "Barrio 001");
        AssertAnonymousProperty(items[^1], "Descripcion", "Barrio 100");
    }

    [Fact]
    public async Task CreatePais_CuandoTieneExito_DevuelveCreatedAtRouteYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePaisCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(7L));
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.CreatePais(new PaisRequest("AR", "Argentina"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtRouteResult>();
        await mediator.Received(1).Send(
            Arg.Is<CreatePaisCommand>(x => x.Codigo == "AR" && x.Descripcion == "Argentina"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeletePais_CuandoTieneProvincias_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePaisCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede eliminar un pais con provincias asociadas."));
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.DeletePais(1, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task DeletePais_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePaisCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.DeletePais(1, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeletePaisCommand>(x => x.Id == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProvincia_CuandoTieneExito_DevuelveCreatedAtRouteYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateProvinciaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(8L));
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.CreateProvincia(new ProvinciaRequest(1, "BA", "Buenos Aires"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtRouteResult>();
        await mediator.Received(1).Send(
            Arg.Is<CreateProvinciaCommand>(x => x.PaisId == 1 && x.Codigo == "BA" && x.Descripcion == "Buenos Aires"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteProvincia_CuandoTieneLocalidades_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteProvinciaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede eliminar una provincia con localidades asociadas."));
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.DeleteProvincia(1, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CreateLocalidad_CuandoTieneExito_DevuelveCreatedAtRouteYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateLocalidadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.CreateLocalidad(new LocalidadRequest(2, "Cordoba", "5000"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtRouteResult>();
        await mediator.Received(1).Send(
            Arg.Is<CreateLocalidadCommand>(x => x.ProvinciaId == 2 && x.Descripcion == "Cordoba" && x.CodigoPostal == "5000"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteLocalidad_CuandoTieneBarrios_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteLocalidadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede eliminar una localidad con barrios asociados."));
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.DeleteLocalidad(1, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CreateBarrio_CuandoTieneExito_DevuelveCreatedAtRouteYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateBarrioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.CreateBarrio(new BarrioRequest(3, "Centro"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtRouteResult>();
        await mediator.Received(1).Send(
            Arg.Is<CreateBarrioCommand>(x => x.LocalidadId == 3 && x.Descripcion == "Centro"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteBarrio_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteBarrioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Pais>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Provincia>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Localidad>()),
            MockDbSetHelper.CreateMockDbSet(Array.Empty<Barrio>()),
            mediator);

        var result = await controller.DeleteBarrio(5, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteBarrioCommand>(x => x.Id == 5),
            Arg.Any<CancellationToken>());
    }

    private static GeografiaController CreateController(
        DbSet<Pais> paises,
        DbSet<Provincia> provincias,
        DbSet<Localidad> localidades,
        DbSet<Barrio> barrios,
        IMediator? mediator = null)
    {
        mediator ??= Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        db.Paises.Returns(paises);
        db.Provincias.Returns(provincias);
        db.Localidades.Returns(localidades);
        db.Barrios.Returns(barrios);

        return new GeografiaController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static Pais BuildPais(long id, string codigo, string descripcion)
    {
        var entity = Pais.Crear(codigo, descripcion);
        SetEntityId(entity, id);
        return entity;
    }

    private static Provincia BuildProvincia(long id, long paisId, string codigo, string descripcion)
    {
        var entity = Provincia.Crear(paisId, codigo, descripcion);
        SetEntityId(entity, id);
        return entity;
    }

    private static Localidad BuildLocalidad(long id, long provinciaId, string descripcion, string? codigoPostal)
    {
        var entity = Localidad.Crear(provinciaId, descripcion, codigoPostal);
        SetEntityId(entity, id);
        return entity;
    }

    private static Barrio BuildBarrio(long id, long localidadId, string descripcion)
    {
        var entity = Barrio.Crear(localidadId, descripcion);
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