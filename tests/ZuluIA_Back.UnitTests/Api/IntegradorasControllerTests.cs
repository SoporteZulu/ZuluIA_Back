using System.Collections;
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
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class IntegradorasControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var integradoras = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildIntegradora(2, "SAP", "Sap", "ERP", "https://sap", "{}", true),
            BuildIntegradora(1, "AFIP", "Afip", "FISCAL", "https://afip", "{}", true)
        });
        db.Integradoras.Returns(integradoras);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "AFIP");
        AssertAnonymousProperty(items[1], "Codigo", "SAP");
    }

    [Fact]
    public async Task GetAll_AplicaFiltrosPorTipoSistemaYActiva()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var integradoras = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildIntegradora(1, "AFIP", "Afip", "FISCAL", "https://afip", "{}", true),
            BuildIntegradora(2, "SAP", "Sap", "ERP", "https://sap", "{}", true),
            BuildIntegradora(3, "WSFE", "Wsfe", "FISCAL", "https://wsfe", "{}", false)
        });
        db.Integradoras.Returns(integradoras);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(" fiscal ", true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "TipoSistema", "FISCAL");
        AssertAnonymousProperty(items[0], "Activa", true);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var integradoras = MockDbSetHelper.CreateMockDbSet<Integradora>();
        db.Integradoras.Returns(integradoras);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var integradoras = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildIntegradora(5, "AFIP", "Afip", "FISCAL", "https://afip", "{}", true)
        });
        db.Integradoras.Returns(integradoras);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "AFIP");
        AssertAnonymousProperty(ok.Value!, "Nombre", "Afip");
        AssertAnonymousProperty(ok.Value!, "TipoSistema", "FISCAL");
        AssertAnonymousProperty(ok.Value!, "Activa", true);
    }

    [Fact]
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una integradora con ese codigo."));

        var result = await controller.Create(new CrearIntegradoraRequest("AFIP", "Afip", "FISCAL", null, null, null), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Codigo requerido."));

        var result = await controller.Create(new CrearIntegradoraRequest("", "Afip", "FISCAL", null, null, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));

        var result = await controller.Create(new CrearIntegradoraRequest("AFIP", "Afip", "FISCAL", null, null, null), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetIntegradoraById");
        AssertAnonymousProperty(created.Value!, "Id", 9L);
        await mediator.Received(1).Send(
            Arg.Is<CreateIntegradoraCommand>(command =>
                command.Codigo == "AFIP" &&
                command.Nombre == "Afip" &&
                command.TipoSistema == "FISCAL" &&
                command.UrlEndpoint == null &&
                command.ApiKey == null &&
                command.Configuracion == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Integradora no encontrada."));

        var result = await controller.Update(8, new ActualizarIntegradoraRequest("Afip", "FISCAL", null, null), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Nombre requerido."));

        var result = await controller.Update(8, new ActualizarIntegradoraRequest("", "FISCAL", null, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConIdYUsaPayload()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(8, new ActualizarIntegradoraRequest("Afip", "FISCAL", "https://api", "{\"mode\":\"prod\"}"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateIntegradoraCommand>(command =>
                command.Id == 8 &&
                command.Nombre == "Afip" &&
                command.TipoSistema == "FISCAL" &&
                command.UrlEndpoint == "https://api" &&
                command.Configuracion == "{\"mode\":\"prod\"}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RotarApiKey_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<RotateIntegradoraApiKeyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Integradora no encontrada."));

        var result = await controller.RotarApiKey(8, new RotarApiKeyRequest("nueva-key"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RotarApiKey_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<RotateIntegradoraApiKeyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("NuevaApiKey requerida."));

        var result = await controller.RotarApiKey(8, new RotarApiKeyRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RotarApiKey_CuandoTieneExito_DevuelveOkYUsaNuevaClave()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<RotateIntegradoraApiKeyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.RotarApiKey(8, new RotarApiKeyRequest("nueva-key"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<RotateIntegradoraApiKeyCommand>(command =>
                command.Id == 8 &&
                command.NuevaApiKey == "nueva-key"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateIntegradoraCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Integradora no encontrada."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Integradora no encontrada."));

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaId()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateIntegradoraCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    private static IntegradorasController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new IntegradorasController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Integradora BuildIntegradora(long id, string codigo, string nombre, string tipoSistema, string? urlEndpoint, string? configuracion, bool activa)
    {
        var entity = Integradora.Crear(codigo, nombre, tipoSistema, urlEndpoint, apiKey: null, configuracion, userId: null);
        typeof(Integradora).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activa)
            entity.Desactivar(userId: null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}