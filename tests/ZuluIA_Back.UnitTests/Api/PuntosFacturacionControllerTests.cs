using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PuntosFacturacionControllerTests
{
    [Fact]
    public async Task GetBySucursal_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var puntos = new List<PuntoFacturacionListDto>
        {
            new() { Id = 1, SucursalId = 10, Numero = 1, Descripcion = "PV 1", Activo = true }
        };
        mediator.Send(Arg.Any<GetPuntosFacturacionQuery>(), Arg.Any<CancellationToken>())
            .Returns(puntos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetBySucursal(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(puntos);
        await mediator.Received(1).Send(
            Arg.Is<GetPuntosFacturacionQuery>(query => query.SucursalId == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTipos_DevuelveOrdenadosPorDescripcion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tiposDbSet = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipoPuntoFacturacion(2, "Manual", false),
            BuildTipoPuntoFacturacion(1, "Controlador", true)
        });
        db.TiposPuntoFacturacion.Returns(tiposDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTipos(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Descripcion", "Controlador");
        AssertAnonymousProperty(data[1], "Id", 2L);
    }

    [Fact]
    public async Task GetProximoNumero_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var dto = new ProximoNumeroDto
        {
            PuntoFacturacionId = 10,
            TipoComprobanteId = 20,
            Prefijo = 1,
            ProximoNumero = 123,
            NumeroFormateado = "0001-00000123"
        };
        mediator.Send(Arg.Any<GetProximoNumeroQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator, db);

        var result = await controller.GetProximoNumero(10, 20, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<GetProximoNumeroQuery>(query => query.PuntoFacturacionId == 10 && query.TipoComprobanteId == 20),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreatePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El número de punto de facturación debe ser mayor a 0."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CreatePuntoFacturacionCommand(10, 2, 0, "PV"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConIdYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreatePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, db);
        var command = new CreatePuntoFacturacionCommand(10, 2, 1, "PV 1");

        var result = await controller.Create(command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 12L);
        await mediator.Received(1).Send(
            Arg.Is<CreatePuntoFacturacionCommand>(request =>
                request.SucursalId == 10 &&
                request.TipoId == 2 &&
                request.Numero == 1 &&
                request.Descripcion == "PV 1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdatePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el punto de facturacion."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdatePuntoFacturacionRequest(2, "PV"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdatePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Descripción requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdatePuntoFacturacionRequest(2, ""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdatePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdatePuntoFacturacionRequest(2, "PV"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("actualizado correctamente");
        await mediator.Received(1).Send(
            Arg.Is<UpdatePuntoFacturacionCommand>(command =>
                command.Id == 8 &&
                command.TipoId == 2 &&
                command.Descripcion == "PV"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeletePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("desactivado correctamente");
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivatePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el punto de facturacion."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivatePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("activado correctamente");
        await mediator.Received(1).Send(
            Arg.Is<ActivatePuntoFacturacionCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetConfiguracionFiscal_DevuelveListaDelPunto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var configuracionesDbSet = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildConfiguracionFiscal(1, 10, 20, 2, true),
            BuildConfiguracionFiscal(2, 11, 21, 3, false)
        });
        db.ConfiguracionesFiscales.Returns(configuracionesDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetConfiguracionFiscal(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().ContainSingle();
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "TipoComprobanteId", 20L);
    }

    [Fact]
    public async Task AddConfiguracionFiscal_CuandoNoExistePunto_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddConfiguracionFiscalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Punto de facturacion no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.AddConfiguracionFiscal(10, BuildConfiguracionRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddConfiguracionFiscal_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddConfiguracionFiscalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        var controller = CreateController(mediator, db);
        var request = BuildConfiguracionRequest();

        var result = await controller.AddConfiguracionFiscal(10, request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PuntosFacturacionController.GetConfiguracionFiscal));
        AssertAnonymousProperty(created.Value!, "Id", 14L);
        await mediator.Received(1).Send(
            Arg.Is<AddConfiguracionFiscalCommand>(command =>
                command.PuntoFacturacionId == 10 &&
                command.TipoComprobanteId == 20 &&
                command.TecnologiaId == 30 &&
                command.InterfazFiscalId == 40 &&
                command.CopiasDefault == 3 &&
                command.Online),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateConfiguracionFiscal_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateConfiguracionFiscalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Configuracion no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateConfiguracionFiscal(10, 14, BuildConfiguracionRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateConfiguracionFiscal_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateConfiguracionFiscalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateConfiguracionFiscal(10, 14, BuildConfiguracionRequest(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 14L);
    }

    [Fact]
    public async Task DeleteConfiguracionFiscal_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteConfiguracionFiscalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteConfiguracionFiscal(10, 14, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetTiposComprobante_CuandoPuntoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var puntosDbSet = MockDbSetHelper.CreateMockDbSet(new List<PuntoFacturacion>());
        db.PuntosFacturacion.Returns(puntosDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTiposComprobante(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetTiposComprobante_CuandoExiste_DevuelveAsignacionesOrdenadas()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var puntosDbSet = MockDbSetHelper.CreateMockDbSet(new[] { BuildPuntoFacturacion(10, 1, 2, 1, "PV", true) });
        var tiposDbSet = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipoComprobantePuntoFacturacion(2, 10, 30, 5, false, 2),
            BuildTipoComprobantePuntoFacturacion(1, 10, 20, 4, true, 1)
        });
        db.PuntosFacturacion.Returns(puntosDbSet);
        db.TiposComprobantesPuntoFacturacion.Returns(tiposDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTiposComprobante(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "TipoComprobanteId", 20L);
        AssertAnonymousProperty(data[1], "TipoComprobanteId", 30L);
    }

    [Fact]
    public async Task AddTipoComprobante_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddTipoComprobantePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El tipo de comprobante ya esta asignado a este punto de facturacion."));
        var controller = CreateController(mediator, db);

        var result = await controller.AddTipoComprobante(10, BuildTipoComprobanteRequest(), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task AddTipoComprobante_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AddTipoComprobantePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, db);
        var request = BuildTipoComprobanteRequest();

        var result = await controller.AddTipoComprobante(10, request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PuntosFacturacionController.GetTiposComprobante));
        AssertAnonymousProperty(created.Value!, "Id", 21L);
        await mediator.Received(1).Send(
            Arg.Is<AddTipoComprobantePuntoFacturacionCommand>(command =>
                command.PuntoFacturacionId == 10 &&
                command.TipoComprobanteId == 20 &&
                command.NumeroComprobanteProximo == 8 &&
                command.Editable == false &&
                command.CantidadCopias == 2 &&
                command.VistaPrevia == true &&
                command.ImprimirControladorFiscal == true &&
                command.PermitirSeleccionMoneda == true &&
                command.VarianteNroUnico == 4 &&
                command.MascaraMoneda == "$ ###"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTipoComprobante_CuandoNoExisteActual_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tiposDbSet = MockDbSetHelper.CreateMockDbSet(new List<TipoComprobantePuntoFacturacion>());
        db.TiposComprobantesPuntoFacturacion.Returns(tiposDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateTipoComprobante(10, 21, BuildTipoComprobanteRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateTipoComprobante_CuandoTieneExito_DevuelveOkYMandaCommandConFallbacks()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tiposDbSet = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipoComprobantePuntoFacturacion(21, 10, 20, 5, true, 1)
        });
        db.TiposComprobantesPuntoFacturacion.Returns(tiposDbSet);
        mediator.Send(Arg.Any<UpdateTipoComprobantePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateTipoComprobante(10, 21, new TipoComprobantePuntoFacturacionRequest(20), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 21L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateTipoComprobantePuntoFacturacionCommand>(command =>
                command.PuntoFacturacionId == 10 &&
                command.TipoComprobantePuntoFacturacionId == 21 &&
                command.NumeroComprobanteProximo == 5 &&
                command.Editable == true &&
                command.CantidadCopias == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteTipoComprobante_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteTipoComprobantePuntoFacturacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteTipoComprobante(10, 21, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    private static PuntosFacturacionController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new PuntosFacturacionController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static TipoPuntoFacturacion BuildTipoPuntoFacturacion(long id, string descripcion, bool porDefecto)
    {
        var entity = (TipoPuntoFacturacion)Activator.CreateInstance(typeof(TipoPuntoFacturacion), nonPublic: true)!;
        typeof(TipoPuntoFacturacion).BaseType!.GetProperty("Id")!.SetValue(entity, id);
        typeof(TipoPuntoFacturacion).GetProperty(nameof(TipoPuntoFacturacion.Descripcion))!.SetValue(entity, descripcion);
        typeof(TipoPuntoFacturacion).GetProperty(nameof(TipoPuntoFacturacion.PorDefecto))!.SetValue(entity, porDefecto);
        return entity;
    }

    private static PuntoFacturacion BuildPuntoFacturacion(long id, long sucursalId, long tipoId, short numero, string? descripcion, bool activo)
    {
        var entity = PuntoFacturacion.Crear(sucursalId, tipoId, numero, descripcion, null);
        typeof(PuntoFacturacion).BaseType!.GetProperty("Id")!.SetValue(entity, id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static ConfiguracionFiscal BuildConfiguracionFiscal(long id, long puntoFacturacionId, long tipoComprobanteId, int copiasDefault, bool online)
    {
        var entity = ConfiguracionFiscal.Crear(puntoFacturacionId, tipoComprobanteId, 30, 40, 1, "COM1", copiasDefault, "clave", "c:/tmp", "c:/bkp", 5, 10, online);
        typeof(ConfiguracionFiscal).BaseType!.GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    private static TipoComprobantePuntoFacturacion BuildTipoComprobantePuntoFacturacion(long id, long puntoFacturacionId, long tipoComprobanteId, long numeroProximo, bool editable, int cantidadCopias)
    {
        var entity = TipoComprobantePuntoFacturacion.Crear(puntoFacturacionId, tipoComprobanteId, numeroProximo, editable, 3, 4, 50, cantidadCopias, true, false, true, 2, "$ ##");
        typeof(TipoComprobantePuntoFacturacion).BaseType!.GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    private static ConfiguracionFiscalRequest BuildConfiguracionRequest()
    {
        return new ConfiguracionFiscalRequest(20, 30, 40, 1, "COM1", 3, "clave", "c:/tmp", "c:/bkp", 5, 10, true);
    }

    private static TipoComprobantePuntoFacturacionRequest BuildTipoComprobanteRequest()
    {
        return new TipoComprobantePuntoFacturacionRequest(20, 8, false, 3, 4, 50, 2, true, true, true, 4, "$ ###");
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}