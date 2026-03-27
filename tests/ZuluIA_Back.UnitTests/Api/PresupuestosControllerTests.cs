using System.Collections;
using System.Security.Claims;
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
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PresupuestosControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosExcluyeEliminadosYPagina()
    {
        var activo1 = BuildPresupuesto(1, 10, 20, new DateOnly(2026, 3, 1), "PENDIENTE", 100m, null, "uno");
        var activo2 = BuildPresupuesto(2, 10, 20, new DateOnly(2026, 3, 3), "PENDIENTE", 200m, null, "dos");
        var otroEstado = BuildPresupuesto(3, 10, 20, new DateOnly(2026, 3, 4), "APROBADO", 300m, null, "tres");
        var otroTercero = BuildPresupuesto(4, 10, 21, new DateOnly(2026, 3, 5), "PENDIENTE", 400m, null, "cuatro");
        var eliminado = BuildPresupuesto(5, 10, 20, new DateOnly(2026, 3, 6), "PENDIENTE", 500m, DateTimeOffset.UtcNow, "cinco");
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb([activo1, activo2, otroEstado, otroTercero, eliminado]));

        var result = await controller.GetAll(1, 10, 10, 20, "PENDIENTE", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "page", 1);
        AssertAnonymousProperty(ok.Value!, "pageSize", 10);
        AssertAnonymousProperty(ok.Value!, "totalCount", 2);
        AssertAnonymousProperty(ok.Value!, "totalPages", 1);
        var items = ((IEnumerable)ok.Value!.GetType().GetProperty("items")!.GetValue(ok.Value)!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[1], "Id", 1L);
        AssertAnonymousProperty(items[0], "Estado", "PENDIENTE");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConMensaje()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetById(99, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Presupuesto 99 no encontrado.");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveDetalleConItemsOrdenados()
    {
        var presupuesto = BuildPresupuesto(7, 10, 20, new DateOnly(2026, 3, 10), "APROBADO", 242m, null, "obs");
        AddItem(presupuesto, BuildPresupuestoItem(1, 7, 100, "Segundo", 1m, 50m, 0m, 2));
        AddItem(presupuesto, BuildPresupuestoItem(2, 7, 101, "Primero", 2m, 100m, 10m, 1));
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb([presupuesto]));

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "Estado", "APROBADO");
        AssertAnonymousProperty(ok.Value!, "Observacion", "obs");
        var items = ((IEnumerable)ok.Value!.GetType().GetProperty("Items")!.GetValue(ok.Value)!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Primero");
        AssertAnonymousProperty(items[0], "Orden", (short)1);
        AssertAnonymousProperty(items[1], "Descripcion", "Segundo");
    }

    [Fact]
    public async Task GetReimpresion_CuandoNoExiste_DevuelveNotFoundConMensaje()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetReimpresion(99, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Presupuesto 99 no encontrado.");
    }

    [Fact]
    public async Task GetReimpresion_CuandoExiste_DevuelvePayloadDedicado()
    {
        var presupuesto = BuildPresupuesto(7, 10, 20, new DateOnly(2026, 3, 10), "APROBADO", 242m, null, "obs");
        AddItem(presupuesto, BuildPresupuestoItem(1, 7, 100, "Segundo", 1m, 50m, 0m, 2));
        AddItem(presupuesto, BuildPresupuestoItem(2, 7, 101, "Primero", 2m, 100m, 10m, 1));
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb([presupuesto]));
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var result = await controller.GetReimpresion(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<PresupuestoReimpresionResponse>();
        var payload = (PresupuestoReimpresionResponse)ok.Value!;
        payload.EsReimpresion.Should().BeTrue();
        payload.GeneradoEn.Should().BeOnOrAfter(before);
        AssertAnonymousProperty(payload.Documento, "Id", 7L);
        AssertAnonymousProperty(payload.Documento, "Estado", "APROBADO");
        var items = ((IEnumerable)payload.Documento.GetType().GetProperty("Items")!.GetValue(payload.Documento)!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Primero");
        AssertAnonymousProperty(items[1], "Descripcion", "Segundo");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePresupuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La cotización debe ser mayor a 0."));
        var controller = CreateController(mediator, BuildDb(), 77);

        var result = await controller.Create(BuildCreateRequest(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La cotización debe ser mayor a 0.");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRouteYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePresupuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var request = BuildCreateRequest();
        var controller = CreateController(mediator, BuildDb(), 77);

        var result = await controller.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetPresupuestoById");
        created.RouteValues!["id"].Should().Be(15L);
        AssertAnonymousProperty(created.Value!, "id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<CreatePresupuestoCommand>(command =>
                command.SucursalId == request.SucursalId &&
                command.TerceroId == request.TerceroId &&
                command.Fecha == request.Fecha &&
                command.FechaVigencia == request.FechaVigencia &&
                command.MonedaId == request.MonedaId &&
                command.Cotizacion == request.Cotizacion &&
                command.Observacion == request.Observacion &&
                command.UserId == 77 &&
                command.Items != null &&
                command.Items.Count == 2 &&
                command.Items.First().ItemId == 100 &&
                command.Items.First().Descripcion == "Producto A" &&
                command.Items.Last().DescuentoPct == 10m),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Aprobar")]
    [InlineData("Rechazar")]
    [InlineData("Delete")]
    public async Task Lifecycle_CuandoNoExiste_DevuelveNotFound(string action)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Result.Failure("Presupuesto 8 no encontrado."));
        var controller = CreateController(mediator, BuildDb(), 77);

        var result = action switch
        {
            "Aprobar" => await controller.Aprobar(8, CancellationToken.None),
            "Rechazar" => await controller.Rechazar(8, CancellationToken.None),
            _ => await controller.Delete(8, CancellationToken.None)
        };

        result.Should().BeAssignableTo<ObjectResult>();
    }

    [Theory]
    [InlineData("Aprobar", "Presupuesto aprobado.")]
    [InlineData("Rechazar", "Presupuesto rechazado.")]
    public async Task Lifecycle_CuandoTieneExito_DevuelveOkConMensaje(string action, string mensaje)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Result.Success());
        var controller = CreateController(mediator, BuildDb(), 77);

        var result = action == "Aprobar"
            ? await controller.Aprobar(8, CancellationToken.None)
            : await controller.Rechazar(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", mensaje);
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePresupuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb(), 77);

        var result = await controller.Delete(8, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    private static PresupuestosController CreateController(IMediator mediator, IApplicationDbContext db, long? userId = null)
    {
        var controller = new PresupuestosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        if (userId.HasValue)
        {
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("sub", userId.Value.ToString())]));
        }

        return controller;
    }

    private static IApplicationDbContext BuildDb(IEnumerable<Presupuesto>? presupuestos = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var presupuestosDbSet = MockDbSetHelper.CreateMockDbSet(presupuestos);
        db.Presupuestos.Returns(presupuestosDbSet);
        return db;
    }

    private static CreatePresupuestoRequest BuildCreateRequest()
    {
        return new CreatePresupuestoRequest(
            10,
            20,
            new DateOnly(2026, 3, 21),
            new DateOnly(2026, 4, 21),
            1,
            1.25m,
            "obs",
            [
                new CreatePresupuestoItemRequest(100, "Producto A", 2m, 50m, 0m),
                new CreatePresupuestoItemRequest(101, "Producto B", 1m, 100m, 10m)
            ]);
    }

    private static Presupuesto BuildPresupuesto(long id, long sucursalId, long terceroId, DateOnly fecha, string estado, decimal total, DateTimeOffset? deletedAt, string? observacion)
    {
        var entity = Presupuesto.Crear(sucursalId, terceroId, fecha, fecha.AddDays(30), 1, 1.5m, observacion, 7);
        SetProperty(entity, nameof(Presupuesto.Id), id);
        SetProperty(entity, nameof(Presupuesto.Estado), estado);
        SetProperty(entity, nameof(Presupuesto.Total), total);
        SetProperty(entity, nameof(Presupuesto.DeletedAt), deletedAt);
        return entity;
    }

    private static PresupuestoItem BuildPresupuestoItem(long id, long presupuestoId, long itemId, string descripcion, decimal cantidad, decimal precioUnitario, decimal descuentoPct, short orden)
    {
        var entity = PresupuestoItem.Crear(presupuestoId, itemId, descripcion, cantidad, precioUnitario, descuentoPct, orden);
        SetProperty(entity, nameof(PresupuestoItem.Id), id);
        return entity;
    }

    private static void AddItem(Presupuesto presupuesto, PresupuestoItem item)
    {
        var field = typeof(Presupuesto).GetField("_items", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var items = (List<PresupuestoItem>)field.GetValue(presupuesto)!;
        items.Add(item);
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}