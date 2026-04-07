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

public class EntidadesControllerTests
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
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflictConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe la integradora AFIP."));

        var result = await controller.Create(new EntidadCreateRequest("AFIP", "Afip", "FISCAL", null, null, null), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe una entidad con ese codigo");
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Codigo requerido."));

        var result = await controller.Create(new EntidadCreateRequest("", "Afip", "FISCAL", null, null, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));

        var result = await controller.Create(new EntidadCreateRequest("AFIP", "Afip", "FISCAL", null, null, null), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(EntidadesController.GetById));
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Integradora no encontrada."));

        var result = await controller.Update(8, new EntidadUpdateRequest("Afip", "FISCAL", null, null), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Entidad 8 no encontrada");
    }

    [Fact]
    public async Task Update_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("TipoSistema requerido."));

        var result = await controller.Update(8, new EntidadUpdateRequest("Afip", "", null, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConIdYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        var request = new EntidadUpdateRequest("Afip", "FISCAL", "https://api", "{}");
        mediator.Send(Arg.Any<UpdateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Update(8, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateIntegradoraCommand>(command =>
                command.Id == 8 &&
                command.Nombre == request.Nombre &&
                command.TipoSistema == request.TipoSistema &&
                command.UrlEndpoint == request.UrlEndpoint &&
                command.Configuracion == request.Configuracion),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Integradora no encontrada."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Entidad 8 no encontrada");
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Integradora no encontrada."));

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Entidad 8 no encontrada");
    }

    [Fact]
    public async Task Activar_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No puede activarse."));

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateIntegradoraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static EntidadesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new EntidadesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
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