using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class TarjetasControllerTests
{
    [Fact]
    public async Task GetAll_CuandoFiltraPorActiva_DevuelveElementosEsperados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tarjetas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTarjetaTipo(2, "MC", "Mastercard", false, true),
            BuildTarjetaTipo(1, "VI", "Visa", false, true),
            BuildTarjetaTipo(3, "DEB", "Debito", true, false)
        });
        db.TarjetasTipos.Returns(tarjetas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "MC");
        AssertAnonymousProperty(items[1], "Codigo", "VI");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tarjetas = MockDbSetHelper.CreateMockDbSet(Array.Empty<TarjetaTipo>());
        db.TarjetasTipos.Returns(tarjetas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConItem()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tarjetas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTarjetaTipo(7, "VI", "Visa", false, true)
        });
        db.TarjetasTipos.Returns(tarjetas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Codigo", "VI");
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Visa");
        AssertAnonymousProperty(ok.Value!, "EsDebito", false);
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una tarjeta con ese código."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new TarjetaTipoRequest("VI", "Visa", false), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe una tarjeta con ese código");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(25L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new TarjetaTipoRequest("VI", "Visa", false), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TarjetasController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 25L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarjeta no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new TarjetaTipoRequest("VI", "Visa", false), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Tarjeta no encontrada");
    }

    [Fact]
    public async Task Update_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Ya existe una tarjeta con ese código."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new TarjetaTipoRequest("VI", "Visa", false), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe una tarjeta con ese código");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new TarjetaTipoRequest("VI", "Visa", false), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarjeta no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Tarjeta no encontrada");
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateTarjetaTipoCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarjeta no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Tarjeta no encontrada");
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateTarjetaTipoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateTarjetaTipoCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static TarjetasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new TarjetasController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static TarjetaTipo BuildTarjetaTipo(long id, string codigo, string descripcion, bool esDebito, bool activa)
    {
        var entity = TarjetaTipo.Crear(codigo, descripcion, esDebito, null);
        SetEntityId(entity, id);
        if (!activa)
            entity.Desactivar(null);
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