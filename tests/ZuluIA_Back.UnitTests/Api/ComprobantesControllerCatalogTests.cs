using System.Collections;
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

public class ComprobantesControllerCatalogTests
{
    [Fact]
    public async Task GetTiposEntrega_CuandoHayDatos_DevuelveOkOrdenado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tiposEntrega = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipoEntrega(2, "RET", "Retiro", 1, "R"),
            BuildTipoEntrega(1, "ENV", "Envio", 1, "E")
        });
        db.TiposEntrega.Returns(tiposEntrega);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTiposEntrega(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "ENV");
        AssertAnonymousProperty(items[1], "Codigo", "RET");
    }

    [Fact]
    public async Task CreateTipoEntrega_CuandoFallaPorDuplicado_DevuelveConflictConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateTipoEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un tipo de entrega con ese codigo."));

        var result = await controller.CreateTipoEntrega(new CreateTipoEntregaRequest("ENV", "Envio", 1, "E"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe un tipo de entrega con ese código");
    }

    [Fact]
    public async Task CreateTipoEntrega_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateTipoEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));

        var result = await controller.CreateTipoEntrega(new CreateTipoEntregaRequest("ENV", "Envio", 1, "E"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(ComprobantesController.GetTiposEntrega));
    }

    [Fact]
    public async Task UpdateTipoEntrega_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateTipoEntregaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo de entrega no encontrado."));

        var result = await controller.UpdateTipoEntrega(10, new UpdateTipoEntregaRequest("Nuevo", 1, "N"), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetDetalleCostos_CuandoHayDatos_DevuelveOkConSoloItemsDelComprobante()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildComprobanteItem(21, 5),
            BuildComprobanteItem(22, 6)
        });
        var detalles = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildDetalleCosto(31, 21, 100, false),
            BuildDetalleCosto(32, 22, 101, true)
        });
        db.ComprobantesItems.Returns(items);
        db.ComprobantesDetallesCostos.Returns(detalles);
        var controller = CreateController(mediator, db);

        var result = await controller.GetDetalleCostos(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        list.Should().ContainSingle();
        AssertAnonymousProperty(list[0], "Id", 31L);
        AssertAnonymousProperty(list[0], "ComprobanteItemId", 21L);
        AssertAnonymousProperty(list[0], "CentroCostoId", 100L);
        AssertAnonymousProperty(list[0], "Procesado", false);
    }

    [Fact]
    public async Task AddDetalleCosto_CuandoItemNoPerteneceAlComprobante_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AddComprobanteDetalleCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El item no pertenece al comprobante indicado."));

        var result = await controller.AddDetalleCosto(5, new AddDetalleCostoRequest(21, 100), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("ítem no pertenece al comprobante indicado");
    }

    [Fact]
    public async Task AddDetalleCosto_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AddComprobanteDetalleCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(41L));

        var result = await controller.AddDetalleCosto(5, new AddDetalleCostoRequest(21, 100), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(ComprobantesController.GetDetalleCostos));
    }

    [Fact]
    public async Task ProcesarDetalleCosto_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ProcesarComprobanteDetalleCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>("Detalle no encontrado."));

        var result = await controller.ProcesarDetalleCosto(5, 31, true, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ProcesarDetalleCosto_CuandoTieneExito_DevuelveOkConEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ProcesarComprobanteDetalleCostoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        var result = await controller.ProcesarDetalleCosto(5, 31, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 31L);
        AssertAnonymousProperty(ok.Value!, "Procesado", true);
    }

    private static ComprobantesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new ComprobantesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static TipoEntrega BuildTipoEntrega(long id, string codigo, string descripcion, long? tipoComprobanteId, string? prefijo)
    {
        var entity = TipoEntrega.Crear(codigo, descripcion, tipoComprobanteId, prefijo);
        typeof(TipoEntrega).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static ComprobanteItem BuildComprobanteItem(long id, long comprobanteId)
    {
        var entity = ComprobanteItem.Crear(comprobanteId, 1, "Item", 1, 0, 100, 0, 1, 21, null, 1);
        typeof(ComprobanteItem).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static ComprobanteDetalleCosto BuildDetalleCosto(long id, long comprobanteItemId, long centroCostoId, bool procesado)
    {
        var entity = ComprobanteDetalleCosto.Crear(comprobanteItemId, centroCostoId);
        typeof(ComprobanteDetalleCosto).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (procesado)
            entity.MarcarProcesado();
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}