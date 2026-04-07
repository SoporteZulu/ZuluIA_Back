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

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}