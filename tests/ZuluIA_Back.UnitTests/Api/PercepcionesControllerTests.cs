using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Impuestos.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PercepcionesControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenaPorTipoYCodigo()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuesto(3, "B", "Beta", "percepcion", 4m, 100m, true),
            BuildImpuesto(1, "A", "Alfa", "percepcion", 3m, 50m, true),
            BuildImpuesto(2, "X", "Otro", "retencion", 2m, 20m, true),
            BuildImpuesto(4, "C", "Inactivo", "percepcion", 1m, 10m, false)
        });
        db.Impuestos.Returns(impuestos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll("percepcion", true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Codigo", "A");
        AssertAnonymousProperty(data[1], "Codigo", "B");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestos = MockDbSetHelper.CreateMockDbSet(new List<Impuesto>());
        db.Impuestos.Returns(impuestos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuesto(5, "PER", "Percepcion", "percepcion", 3.5m, 100m, true, "obs")
        });
        db.Impuestos.Returns(impuestos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "PER");
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Percepcion");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Codigo requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateImpuestoRequest("", "Percepcion", 3m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(7L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreateImpuestoRequest("PER", "Percepcion", 3.5m, 100m, "percepcion", "obs"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PercepcionesController.GetById));
        AssertAnonymousProperty(created.Value!, "id", 7L);
        await mediator.Received(1).Send(
            Arg.Is<CreateImpuestoCommand>(command =>
                command.Codigo == "PER" &&
                command.Descripcion == "Percepcion" &&
                command.Alicuota == 3.5m &&
                command.MinimoBaseCalculo == 100m &&
                command.Tipo == "percepcion" &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Impuesto no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateImpuestoRequest("Desc", 3m, 100m, "percepcion", "obs"), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo inválido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateImpuestoRequest("Desc", 3m, 100m, "otro", "obs"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateImpuestoRequest("Desc", 3m, 100m, "percepcion", "obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 8L);
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Impuesto no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateImpuestoCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Impuesto no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateImpuestoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateImpuestoCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByTercero_DevuelveJoinConObservacionPreferenteDeAsignacion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestosPorPersona = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuestoPorPersona(1, 5, 10, "Desc asig", "Obs asig")
        });
        db.ImpuestosPorPersona.Returns(impuestosPorPersona);
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuesto(5, "PER", "Percepcion", "percepcion", 3m, 100m, true, "Obs imp")
        });
        db.Impuestos.Returns(impuestos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetByTercero(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().ContainSingle();
        AssertAnonymousProperty(data[0], "Codigo", "PER");
        AssertAnonymousProperty(data[0], "Observacion", "Obs asig");
    }

    [Fact]
    public async Task AsignarTercero_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AssignImpuestoTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya esta asignado."));
        var controller = CreateController(mediator, db);

        var result = await controller.AsignarTercero(5, new AsignarImpuestoTerceroRequest(10, "Desc", "Obs"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task AsignarTercero_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AssignImpuestoTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, db);

        var result = await controller.AsignarTercero(5, new AsignarImpuestoTerceroRequest(10, "Desc", "Obs"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PercepcionesController.GetByTercero));
        AssertAnonymousProperty(created.Value!, "id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<AssignImpuestoTerceroCommand>(command =>
                command.ImpuestoId == 5 &&
                command.TerceroId == 10 &&
                command.Descripcion == "Desc" &&
                command.Observacion == "Obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesasignarTercero_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UnassignImpuestoTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DesasignarTercero(5, 10, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetByItem_DevuelveJoinConObservacionPreferenteDeAsignacion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestosPorItem = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuestoPorItem(1, 5, 20, "Desc asig", "Obs asig")
        });
        db.ImpuestosPorItem.Returns(impuestosPorItem);
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuesto(5, "PER", "Percepcion", "percepcion", 3m, 100m, true, "Obs imp")
        });
        db.Impuestos.Returns(impuestos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetByItem(20, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().ContainSingle();
        AssertAnonymousProperty(data[0], "Codigo", "PER");
        AssertAnonymousProperty(data[0], "Observacion", "Obs asig");
    }

    [Fact]
    public async Task AsignarItem_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AssignImpuestoItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(16L));
        var controller = CreateController(mediator, db);

        var result = await controller.AsignarItem(5, new AsignarImpuestoItemRequest(20, "Desc", "Obs"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PercepcionesController.GetByItem));
        AssertAnonymousProperty(created.Value!, "id", 16L);
        await mediator.Received(1).Send(
            Arg.Is<AssignImpuestoItemCommand>(command =>
                command.ImpuestoId == 5 &&
                command.ItemId == 20 &&
                command.Descripcion == "Desc" &&
                command.Observacion == "Obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesasignarItem_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UnassignImpuestoItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.DesasignarItem(5, 20, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Calcular_AplicaSoloImpuestosActivosQueSuperanMinimo()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestosPorPersona = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuestoPorPersona(1, 5, 10, null, null),
            BuildImpuestoPorPersona(2, 6, 10, null, null)
        });
        db.ImpuestosPorPersona.Returns(impuestosPorPersona);
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuesto(5, "A", "Imp A", "percepcion", 10m, 100m, true),
            BuildImpuesto(6, "B", "Imp B", "percepcion", 5m, 1000m, true),
            BuildImpuesto(7, "C", "Imp C", "percepcion", 20m, 10m, false)
        });
        db.Impuestos.Returns(impuestos);
        var controller = CreateController(mediator, db);

        var result = await controller.Calcular(new CalcularPercepcionRequest(10, 500m), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "TerceroId", 10L);
        AssertAnonymousProperty(ok.Value!, "ImporteBase", 500m);
        AssertAnonymousProperty(ok.Value!, "totalPercepciones", 50m);
        var percepciones = ok.Value!.GetType().GetProperty("percepciones")!.GetValue(ok.Value)
            .Should().BeAssignableTo<IEnumerable>().Subject.Cast<object>().ToList();
        percepciones.Should().ContainSingle();
        AssertAnonymousProperty(percepciones[0], "Codigo", "A");
        AssertAnonymousProperty(percepciones[0], "Importe", 50m);
    }

    [Fact]
    public async Task GetSucursales_CuandoImpuestoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestos = MockDbSetHelper.CreateMockDbSet(new List<Impuesto>());
        db.Impuestos.Returns(impuestos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetSucursales(5, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSucursales_CuandoExiste_DevuelveAsignaciones()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[] { BuildImpuesto(5, "PER", "Percepcion", "percepcion", 3m, 100m, true) });
        var asignaciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuestoPorSucursal(1, 5, 10, "Sucursal A", "Obs")
        });
        db.Impuestos.Returns(impuestos);
        db.ImpuestosPorSucursal.Returns(asignaciones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetSucursales(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().ContainSingle();
        AssertAnonymousProperty(data[0], "SucursalId", 10L);
    }

    [Fact]
    public async Task AsignarSucursal_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AssignImpuestoSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(18L));
        var controller = CreateController(mediator, db);

        var result = await controller.AsignarSucursal(5, new AsignarImpuestoSucursalRequest(10, "Sucursal A", "Obs"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PercepcionesController.GetSucursales));
        AssertAnonymousProperty(created.Value!, "id", 18L);
        await mediator.Received(1).Send(
            Arg.Is<AssignImpuestoSucursalCommand>(command =>
                command.ImpuestoId == 5 &&
                command.SucursalId == 10 &&
                command.Descripcion == "Sucursal A" &&
                command.Observacion == "Obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSucursal_CuandoTieneExito_DevuelveOkConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateImpuestoSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new UpdateImpuestoSucursalResult(18, "Sucursal A", "Obs")));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateSucursal(5, 18, new UpdateImpuestoSucursalRequest("Sucursal A", "Obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 18L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Sucursal A");
    }

    [Fact]
    public async Task EliminarSucursal_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteImpuestoSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.EliminarSucursal(5, 18, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetTiposComprobante_CuandoExiste_DevuelveAsignacionesOrdenadas()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var impuestos = MockDbSetHelper.CreateMockDbSet(new[] { BuildImpuesto(5, "PER", "Percepcion", "percepcion", 3m, 100m, true) });
        var asignaciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildImpuestoPorTipoComprobante(2, 5, 20, 2),
            BuildImpuestoPorTipoComprobante(1, 5, 10, 1)
        });
        db.Impuestos.Returns(impuestos);
        db.ImpuestosPorTipoComprobante.Returns(asignaciones);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTiposComprobante(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "TipoComprobanteId", 10L);
        AssertAnonymousProperty(data[1], "TipoComprobanteId", 20L);
    }

    [Fact]
    public async Task AsignarTipoComprobante_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AssignImpuestoTipoComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("ya esta asignado"));
        var controller = CreateController(mediator, db);

        var result = await controller.AsignarTipoComprobante(5, new AsignarImpuestoTipoComprobanteRequest(10, 1), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task EliminarTipoComprobante_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteImpuestoTipoComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.EliminarTipoComprobante(5, 10, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    private static PercepcionesController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new PercepcionesController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Impuesto BuildImpuesto(long id, string codigo, string descripcion, string tipo, decimal alicuota, decimal minimoBaseCalculo, bool activo, string? observacion = null)
    {
        var entity = Impuesto.Crear(codigo, descripcion, alicuota, minimoBaseCalculo, tipo, observacion);
        typeof(Impuesto).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!activo)
            entity.Desactivar();
        return entity;
    }

    private static ImpuestoPorPersona BuildImpuestoPorPersona(long id, long impuestoId, long terceroId, string? descripcion, string? observacion)
    {
        var entity = ImpuestoPorPersona.Crear(impuestoId, terceroId, descripcion, observacion);
        typeof(ImpuestoPorPersona).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static ImpuestoPorItem BuildImpuestoPorItem(long id, long impuestoId, long itemId, string? descripcion, string? observacion)
    {
        var entity = ImpuestoPorItem.Crear(impuestoId, itemId, descripcion, observacion);
        typeof(ImpuestoPorItem).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static ImpuestoPorSucursal BuildImpuestoPorSucursal(long id, long impuestoId, long sucursalId, string? descripcion, string? observacion)
    {
        var entity = ImpuestoPorSucursal.Crear(impuestoId, sucursalId, descripcion, observacion);
        typeof(ImpuestoPorSucursal).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static ImpuestoPorTipoComprobante BuildImpuestoPorTipoComprobante(long id, long impuestoId, long tipoComprobanteId, int orden)
    {
        var entity = ImpuestoPorTipoComprobante.Crear(impuestoId, tipoComprobanteId, orden);
        typeof(ImpuestoPorTipoComprobante).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}