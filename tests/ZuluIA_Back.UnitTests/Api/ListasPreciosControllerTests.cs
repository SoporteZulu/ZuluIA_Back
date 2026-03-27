using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ListasPrecios.Commands;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Application.Features.ListasPrecios.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ListasPreciosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConListas()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetListasPreciosQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ListaPreciosDto>
            {
                new() { Id = 1, Descripcion = "Minorista", MonedaId = 1, Activa = true },
                new() { Id = 2, Descripcion = "Mayorista", MonedaId = 2, Activa = false }
            });
        var controller = CreateController(mediator);

        var result = await controller.GetAll(new DateOnly(2026, 3, 20), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<ListaPreciosDto>>().Subject;
        items.Should().HaveCount(2);
        await mediator.Received(1).Send(Arg.Is<GetListasPreciosQuery>(q => q.Fecha == new DateOnly(2026, 3, 20)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetListaPreciosByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((ListaPreciosDetalleDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetListaPreciosByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ListaPreciosDetalleDto
            {
                Id = 7,
                Descripcion = "Minorista",
                MonedaId = 1,
                Activa = true,
                Items = [new ListaPreciosItemDto { ItemId = 10, Precio = 1500m, DescuentoPct = 5m }]
            });
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<ListaPreciosDetalleDto>().Subject;
        dto.Items.Should().ContainSingle();
        dto.Items[0].Precio.Should().Be(1500m);
    }

    [Fact]
    public async Task GetPrecioItem_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPrecioItemQuery>(), Arg.Any<CancellationToken>())
            .Returns((ListaPreciosItemDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetPrecioItem(7, 10, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPrecioItem_CuandoExiste_DevuelveOkConItem()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPrecioItemQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ListaPreciosItemDto { ItemId = 10, Precio = 1500m, DescuentoPct = 5m });
        var controller = CreateController(mediator);

        var result = await controller.GetPrecioItem(7, 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<ListaPreciosItemDto>().Subject;
        dto.ItemId.Should().Be(10);
        dto.Precio.Should().Be(1500m);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetListaPreciosById");
        AssertAnonymousProperty(created.Value!, "id", 15L);
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequestSinInvocarMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(8), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ID de la URL no coincide");
        await mediator.DidNotReceive().Send(Arg.Any<UpdateListaPreciosCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la lista con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(7), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró la lista con ID 7");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(7), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la lista con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró la lista con ID 7");
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Activar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la lista con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró la lista con ID 7");
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateListaPreciosCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertItem_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpsertItemEnListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El precio debe ser mayor a cero."));
        var controller = CreateController(mediator);

        var result = await controller.UpsertItem(7, new UpsertItemEnListaRequest(10, 0m, 5m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("precio debe ser mayor a cero");
    }

    [Fact]
    public async Task UpsertItem_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpsertItemEnListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.UpsertItem(7, new UpsertItemEnListaRequest(10, 1500m, 5m), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RemoveItem_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemoveItemDeListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El item no está en la lista."));
        var controller = CreateController(mediator);

        var result = await controller.RemoveItem(7, 10, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("item no está en la lista");
    }

    [Fact]
    public async Task RemoveItem_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemoveItemDeListaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.RemoveItem(7, 10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetPersonas_CuandoHayDatos_DevuelveOkFiltrado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var personas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildListaPrecioPersona(1, 7, 21),
            BuildListaPrecioPersona(2, 7, 22),
            BuildListaPrecioPersona(3, 8, 23)
        });
        db.ListasPreciosPersonas.Returns(personas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetPersonas(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "ListaPreciosId", 7L);
        AssertAnonymousProperty(items[1], "PersonaId", 22L);
    }

    [Fact]
    public async Task AddPersona_CuandoYaExiste_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPersonaAListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La persona ya está asignada a esta lista."));
        var controller = CreateController(mediator);

        var result = await controller.AddPersona(7, 21, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está asignada");
    }

    [Fact]
    public async Task AddPersona_CuandoFallaPorOtraCausa_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPersonaAListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La persona es requerida."));
        var controller = CreateController(mediator);

        var result = await controller.AddPersona(7, 0, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("persona es requerida");
    }

    [Fact]
    public async Task AddPersona_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddPersonaAListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(31L));
        var controller = CreateController(mediator);

        var result = await controller.AddPersona(7, 21, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 31L);
    }

    [Fact]
    public async Task RemovePersona_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemovePersonaDeListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La persona no está asignada a esta lista."));
        var controller = CreateController(mediator);

        var result = await controller.RemovePersona(7, 21, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("no está asignada a esta lista");
    }

    [Fact]
    public async Task RemovePersona_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemovePersonaDeListaPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.RemovePersona(7, 21, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static ListasPreciosController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new ListasPreciosController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CreateListaPreciosCommand BuildCreateCommand()
        => new("Minorista", 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

    private static UpdateListaPreciosCommand BuildUpdateCommand(long id)
        => new(id, "Minorista", 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

    private static ListaPrecioPersona BuildListaPrecioPersona(long id, long listaId, long personaId)
    {
        var entity = ListaPrecioPersona.Crear(listaId, personaId);
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