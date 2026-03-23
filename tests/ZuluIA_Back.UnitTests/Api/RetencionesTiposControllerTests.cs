using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Retenciones.Commands;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;
using ZuluIA_Back.Application.Features.Retenciones.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class RetencionesTiposControllerTests
{
    [Fact]
    public async Task GetAll_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tipos = new[]
        {
            new TipoRetencionDto(1, "Ganancias", "GAN", 1000m, true, 5, null, true, []),
            new TipoRetencionDto(2, "IVA", "IVA", 500m, false, null, 8, true, [])
        };
        mediator.Send(Arg.Any<GetTiposRetencionQuery>(), Arg.Any<CancellationToken>())
            .Returns(tipos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(tipos);
        await mediator.Received(1).Send(
            Arg.Is<GetTiposRetencionQuery>(query => !query.SoloActivos),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<GetTipoRetencionByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((TipoRetencionDto?)null);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var dto = new TipoRetencionDto(
            9,
            "Ganancias",
            "GAN",
            1200m,
            true,
            4,
            7,
            true,
            [new EscalaRetencionDto(3, "Escala 1", 0m, 10000m, 2.5m)]);
        mediator.Send(Arg.Any<GetTipoRetencionByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Calcular_CuandoTieneExito_DevuelveOkConImporte()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CalcularRetencionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(245.75m));
        var controller = CreateController(mediator, db);

        var result = await controller.Calcular(6, 10000m, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "tipoRetencionId", 6L);
        AssertAnonymousProperty(ok.Value!, "baseImponible", 10000m);
        AssertAnonymousProperty(ok.Value!, "importeRetencion", 245.75m);
    }

    [Fact]
    public async Task Calcular_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CalcularRetencionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<decimal>("Tipo de retención inexistente."));
        var controller = CreateController(mediator, db);

        var result = await controller.Calcular(6, 10000m, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRouteYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTipoRetencionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, db);
        var command = new CreateTipoRetencionCommand(
            "Ganancias",
            "GAN",
            1000m,
            true,
            8,
            9,
            [new EscalaRetencionInputDto("Tramo 1", 0m, 5000m, 2m)]);

        var result = await controller.Create(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetTipoRetencionById");
        AssertAnonymousProperty(created.Value!, "id", 21L);
        await mediator.Received(1).Send(
            Arg.Is<CreateTipoRetencionCommand>(request =>
                request.Descripcion == "Ganancias" &&
                request.Regimen == "GAN" &&
                request.MinimoNoImponible == 1000m &&
                request.AcumulaPago &&
                request.TipoComprobanteId == 8 &&
                request.ItemId == 9 &&
                request.Escalas.Count == 1 &&
                request.Escalas[0].Descripcion == "Tramo 1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTipoRetencionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(
            new CreateTipoRetencionCommand(string.Empty, "GAN", 1000m, false, null, null, []),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var controller = CreateController(mediator, db);

        var result = await controller.Update(
            4,
            new UpdateTipoRetencionCommand(5, "Ganancias", "GAN", 1000m, true, null, null, []),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTipoRetencionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el tipo de retención."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(
            5,
            new UpdateTipoRetencionCommand(5, "Ganancias", "GAN", 1500m, true, 3, 4, []),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTipoRetencionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);
        var command = new UpdateTipoRetencionCommand(
            5,
            "Ganancias",
            "GAN",
            1500m,
            true,
            3,
            4,
            [new EscalaRetencionInputDto("Tramo 1", 0m, 5000m, 2m)]);

        var result = await controller.Update(5, command, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<UpdateTipoRetencionCommand>(request =>
                request.Id == 5 &&
                request.Descripcion == "Ganancias" &&
                request.Regimen == "GAN" &&
                request.MinimoNoImponible == 1500m &&
                request.Escalas.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteTipoRetencionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteTipoRetencionCommand>(request => request.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteTipoRetencionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el tipo de retención."));
        var controller = CreateController(mediator, db);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRegimenes_FiltraPorRetencionYOrdenaPorCodigo()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regimenes = MockDbSetHelper.CreateMockDbSet([
            BuildRegimen(3, 10, "B002", "Regimen B", "obs-b"),
            BuildRegimen(1, 10, "A001", "Regimen A", "obs-a"),
            BuildRegimen(2, 11, "A099", "Otro", "obs-x")
        ]);
        db.RetencionesRegimenes.Returns(regimenes);
        var controller = CreateController(mediator, db);

        var result = await controller.GetRegimenes(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Codigo", "A001");
        AssertAnonymousProperty(data[1], "Id", 3L);
        AssertAnonymousProperty(data[1], "Codigo", "B002");
    }

    [Fact]
    public async Task GetRegimenById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regimenes = MockDbSetHelper.CreateMockDbSet(new List<RetencionRegimen>());
        db.RetencionesRegimenes.Returns(regimenes);
        var controller = CreateController(mediator, db);

        var result = await controller.GetRegimenById(10, 55, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRegimenById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var regimen = BuildRegimen(5, 10, "A001", "Regimen A", "obs-a");
        var regimenes = MockDbSetHelper.CreateMockDbSet([regimen]);
        db.RetencionesRegimenes.Returns(regimenes);
        var controller = CreateController(mediator, db);

        var result = await controller.GetRegimenById(10, 5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(regimen);
    }

    [Fact]
    public async Task CreateRegimen_CuandoTieneExito_DevuelveCreatedAtRouteYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateRetencionRegimenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, db);

        var result = await controller.CreateRegimen(
            10,
            new CreateRetencionRegimenRequest(" rg-01 ", "Regimen General", " obs "),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetRetencionRegimenById");
        AssertAnonymousProperty(created.Value!, "Id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<CreateRetencionRegimenCommand>(command =>
                command.RetencionId == 10 &&
                command.Codigo == " rg-01 " &&
                command.Descripcion == "Regimen General" &&
                command.Observacion == " obs "),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRegimen_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateRetencionRegimenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El código es requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.CreateRegimen(
            10,
            new CreateRetencionRegimenRequest(string.Empty, "Regimen General"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateRegimen_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateRetencionRegimenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);
        var request = new UpdateRetencionRegimenRequest(
            true,
            false,
            "NETO",
            100m,
            true,
            50m,
            true,
            10m,
            true,
            500m,
            false,
            "RET",
            25m,
            true,
            400m,
            false,
            2.5m,
            true,
            false,
            1.5m,
            true,
            "obs");

        var result = await controller.UpdateRegimen(10, 15, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateRetencionRegimenCommand>(command =>
                command.RetencionId == 10 &&
                command.RegimenId == 15 &&
                command.ControlTipoComprobante &&
                !command.ControlTipoComprobanteAplica &&
                command.BaseImponibleComposicion == "NETO" &&
                command.NoImponible == 100m &&
                command.Alicuota == 2.5m &&
                command.AlicuotaConvenio == 1.5m &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRegimen_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateRetencionRegimenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Régimen no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateRegimen(
            10,
            15,
            new UpdateRetencionRegimenRequest(false, false, null, null, false, null, false, null, false, null, false, null, null, false, null, false, null, false, false, null, false, null),
            CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteRegimen_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteRetencionRegimenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteRegimen(10, 15, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteRetencionRegimenCommand>(command => command.RetencionId == 10 && command.RegimenId == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRegimen_CuandoFalla_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteRetencionRegimenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Régimen no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteRegimen(10, 15, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static RetencionesTiposController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new RetencionesTiposController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static RetencionRegimen BuildRegimen(long id, long retencionId, string codigo, string descripcion, string? observacion)
    {
        var regimen = RetencionRegimen.Crear(codigo, descripcion, retencionId, observacion);
        regimen.GetType().GetProperty(nameof(RetencionRegimen.Id))!.SetValue(regimen, id);
        regimen.ActualizarParametros(
            controlTipoComprobante: true,
            controlTipoComprobanteAplica: false,
            baseImponibleComposicion: "NETO",
            noImponible: 100m,
            noImponibleAplica: true,
            baseImponiblePorcentaje: 50m,
            baseImponiblePorcentajeAplica: true,
            baseImponibleMinimo: 10m,
            baseImponibleMinimoAplica: true,
            baseImponibleMaximo: 500m,
            baseImponibleMaximoAplica: false,
            retencionComposicion: "RET",
            retencionMinimo: 25m,
            retencionMinimoAplica: true,
            retencionMaximo: 400m,
            retencionMaximoAplica: false,
            alicuota: 2.5m,
            alicuotaAplica: true,
            alicuotaEscalaAplica: false,
            alicuotaConvenio: 1.5m,
            alicuotaConvenioAplica: true,
            observacion: observacion);
        return regimen;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}