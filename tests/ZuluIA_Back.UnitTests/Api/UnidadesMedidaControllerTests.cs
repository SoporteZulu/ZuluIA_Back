using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Referencia.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class UnidadesMedidaControllerTests
{
    [Fact]
    public async Task GetAll_CuandoSoloActivas_DevuelveElementosOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var unidades = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildUnidadMedida(2, "KG", "Kilogramo", "kg", 1m, true, null, true),
            BuildUnidadMedida(1, "LT", "Litro", "lt", 1m, true, null, true),
            BuildUnidadMedida(3, "CJ", "Caja", "cj", 12m, false, 1, false)
        });
        db.UnidadesMedida.Returns(unidades);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Kilogramo");
        AssertAnonymousProperty(items[1], "Descripcion", "Litro");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var unidades = MockDbSetHelper.CreateMockDbSet(Array.Empty<UnidadMedida>());
        db.UnidadesMedida.Returns(unidades);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Unidad de medida 7 no encontrada");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var unidades = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildUnidadMedida(7, "KG", "Kilogramo", "kg", 1m, true, null, true)
        });
        db.UnidadesMedida.Returns(unidades);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Codigo", "KG");
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Kilogramo");
        AssertAnonymousProperty(ok.Value!, "EsUnidadBase", true);
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una unidad con ese código."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new UnidadMedidaRequest("KG", "Kilogramo", "kg"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe una unidad con ese código");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new UnidadMedidaRequest("KG", "Kilogramo", "kg"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetUnidadMedidaById");
        AssertAnonymousProperty(created.Value!, "Id", 21L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Unidad no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new UnidadMedidaUpdateRequest("Kilogramo", "kg", 1m, true, null), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La descripción es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new UnidadMedidaUpdateRequest("", "kg", 1m, true, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task Update_CuandoTieneExitoYExisteEntidad_DevuelveOkConDescripcion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var unidades = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildUnidadMedida(7, "KG", "Kilogramo", "kg", 1m, true, null, true)
        });
        db.UnidadesMedida.Returns(unidades);
        mediator.Send(Arg.Any<UpdateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new UnidadMedidaUpdateRequest("Kilogramo", "kg", 1m, true, null), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Kilogramo");
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Unidad no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkConActivaTrueYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "activa", true);
        await mediator.Received(1).Send(
            Arg.Is<ActivateUnidadMedidaCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Unidad no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkConActivaFalseYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateUnidadMedidaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "activa", false);
        await mediator.Received(1).Send(
            Arg.Is<DeactivateUnidadMedidaCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static UnidadesMedidaController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new UnidadesMedidaController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static UnidadMedida BuildUnidadMedida(long id, string codigo, string descripcion, string? disminutivo, decimal multiplicador, bool esUnidadBase, long? unidadBaseId, bool activa)
    {
        var entity = UnidadMedida.Crear(codigo, descripcion, disminutivo, multiplicador, esUnidadBase, unidadBaseId);
        SetEntityId(entity, id);
        if (!activa)
            entity.Desactivar();
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