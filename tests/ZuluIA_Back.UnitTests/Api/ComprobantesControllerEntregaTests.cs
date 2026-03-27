using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerEntregaTests
{
    [Fact]
    public async Task GetEntrega_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var entregas = MockDbSetHelper.CreateMockDbSet<ComprobanteEntrega>(Array.Empty<ComprobanteEntrega>());
        db.ComprobantesEntregas.Returns(entregas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetEntrega(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetEntrega_CuandoExiste_DevuelveOkConEntidad()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var entrega = BuildEntrega(11, 7, new DateOnly(2026, 3, 20), "Cliente Uno", "Calle 123");
        var entregas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            entrega,
            BuildEntrega(12, 8, new DateOnly(2026, 3, 21), "Otro", "Calle 456")
        });
        db.ComprobantesEntregas.Returns(entregas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetEntrega(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(entrega);
    }

    [Fact]
    public async Task CreateEntrega_CuandoComprobanteNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateComprobanteEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Comprobante no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.CreateEntrega(7, BuildRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateEntrega_CuandoYaExiste_DevuelveConflictConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateComprobanteEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El comprobante ya tiene datos de entrega. Use PUT para actualizar."));
        var controller = CreateController(mediator);

        var result = await controller.CreateEntrega(7, BuildRequest(), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya tiene datos de entrega");
    }

    [Fact]
    public async Task CreateEntrega_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateComprobanteEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El comprobante es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.CreateEntrega(7, BuildRequest(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("comprobante es requerido");
    }

    [Fact]
    public async Task CreateEntrega_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateComprobanteEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.CreateEntrega(7, BuildRequest(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ComprobantesController.GetEntrega));
        created.RouteValues!["id"].Should().Be(7L);
        AssertAnonymousProperty(created.Value!, "Id", 21L);
    }

    [Fact]
    public async Task UpdateEntrega_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateComprobanteEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Entrega no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateEntrega(7, BuildRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateEntrega_CuandoTieneExito_DevuelveOkConIdRecuperado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateComprobanteEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var db = Substitute.For<IApplicationDbContext>();
        var entrega = BuildEntrega(21, 7, new DateOnly(2026, 3, 20), "Cliente Uno", "Calle 123");
        var entregas = MockDbSetHelper.CreateMockDbSet(new[] { entrega });
        db.ComprobantesEntregas.Returns(entregas);
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateEntrega(7, BuildRequest(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 21L);
    }

    private static ComprobantesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new ComprobantesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CreateEntregaRequest BuildRequest()
        => new(
            new DateOnly(2026, 3, 20),
            "Cliente Uno",
            "Calle 123",
            10,
            20,
            30,
            "5000",
            "351111111",
            "351222222",
            "351333333",
            "cliente@example.com",
            "Entregar por la tarde",
            40,
            50,
            60);

    private static ComprobanteEntrega BuildEntrega(long id, long comprobanteId, DateOnly fecha, string razonSocial, string domicilio)
    {
        var entity = ComprobanteEntrega.Crear(
            comprobanteId,
            fecha,
            razonSocial,
            domicilio,
            10,
            20,
            30,
            "5000",
            "351111111",
            "351222222",
            "351333333",
            "cliente@example.com",
            "Entregar por la tarde",
            40,
            50,
            60);
        typeof(ComprobanteEntrega).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}