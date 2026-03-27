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

public class ContratosControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYExcluyeAnuladosPorDefecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var contratos = MockDbSetHelper.CreateMockDbSet([
            BuildContrato(1, 10, "VIGENTE", false, new DateOnly(2026, 1, 1), 12),
            BuildContrato(3, 10, "VIGENTE", false, new DateOnly(2026, 3, 1), 6),
            BuildContrato(2, 10, "VIGENTE", true, new DateOnly(2026, 2, 1), 8),
            BuildContrato(4, 11, "VENCIDO", false, new DateOnly(2026, 4, 1), 0)
        ]);
        db.Contratos.Returns(contratos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(10, "vigente", false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 3L);
        AssertAnonymousProperty(data[1], "Id", 1L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var contratos = MockDbSetHelper.CreateMockDbSet(new List<Contrato>());
        db.Contratos.Returns(contratos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveDetalleConItems()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var contrato = BuildContrato(5, 10, "VIGENTE", false, new DateOnly(2026, 3, 1), 12, "obs");
        contrato.AgregarDetalle(100, "Servicio A", 2m, 50m, 21m, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), new DateOnly(2026, 3, 10), 1, 15, "ab123cd");
        contrato.AgregarDetalle(null, "Servicio B", 1m, 75m, null, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), new DateOnly(2026, 4, 10), 1, 20, null);
        var contratos = MockDbSetHelper.CreateMockDbSet([contrato]);
        db.Contratos.Returns(contratos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Observacion", "obs");
        var detalles = ok.Value!.GetType().GetProperty("Detalles")!.GetValue(ok.Value)
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        detalles.Should().HaveCount(2);
        AssertAnonymousProperty(detalles[0], "Descripcion", "Servicio A");
        AssertAnonymousProperty(detalles[0], "Dominio", "AB123CD");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateContratoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El tercero es requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(BuildCreateRequest(0), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRouteYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateContratoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, db);
        var request = BuildCreateRequest(10);

        var result = await controller.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetContratoById");
        AssertAnonymousProperty(created.Value!, "id", 12L);
        await mediator.Received(1).Send(
            Arg.Is<CreateContratoCommand>(command =>
                command.TerceroId == 10 &&
                command.Detalles.Count == 1 &&
                command.Detalles.First().Descripcion == "Servicio A" &&
                command.Detalles.First().Dominio == "ab123cd"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateContratoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Contrato no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new ActualizarContratoRequest(2, 3, new DateOnly(2026, 12, 31), 1, 12, 500m, "obs"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateContratoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new ActualizarContratoRequest(2, 3, new DateOnly(2026, 12, 31), 1, 12, 500m, "obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 8L);
    }

    [Fact]
    public async Task Anular_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AnularContratoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AnularContratoResult>("Contrato no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(7, new AnularContratoRequest("motivo"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOkConEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AnularContratoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new AnularContratoResult(7, "ANULADO")));
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(7, new AnularContratoRequest("motivo"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 7L);
        AssertAnonymousProperty(ok.Value!, "estado", "ANULADO");
    }

    [Fact]
    public async Task RegistrarFacturacion_CuandoTieneExito_DevuelveOkConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<RegistrarFacturacionContratoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new RegistrarFacturacionContratoResult(7, 3, "VIGENTE")));
        var controller = CreateController(mediator, db);

        var result = await controller.RegistrarFacturacion(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 7L);
        AssertAnonymousProperty(ok.Value!, "cuotasRestantes", 3);
        AssertAnonymousProperty(ok.Value!, "estado", "VIGENTE");
    }

    [Fact]
    public async Task GetVencidos_DevuelveSoloVigentesNoAnuladosConFechaPasada()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var contratos = MockDbSetHelper.CreateMockDbSet([
            BuildContrato(1, 10, "VIGENTE", false, yesterday.AddMonths(-1), 1, fechaVencimiento: yesterday),
            BuildContrato(2, 11, "VIGENTE", false, yesterday.AddMonths(-1), 1, fechaVencimiento: tomorrow),
            BuildContrato(3, 12, "ANULADO", true, yesterday.AddMonths(-1), 1, fechaVencimiento: yesterday)
        ]);
        db.Contratos.Returns(contratos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetVencidos(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(1);
        AssertAnonymousProperty(data[0], "Id", 1L);
    }

    private static ContratosController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ContratosController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CrearContratoRequest BuildCreateRequest(long terceroId)
    {
        return new CrearContratoRequest(
            terceroId,
            2,
            3,
            4,
            5,
            6,
            7,
            1m,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 1, 10),
            1,
            12,
            500m,
            "obs",
            [new ContratoDetalleRequest(100, "Servicio A", 2m, 50m, 21m, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), new DateOnly(2026, 1, 10), 1, 15, "ab123cd")]);
    }

    private static Contrato BuildContrato(long id, long terceroId, string estado, bool anulado, DateOnly fechaDesde, int cuotasRestantes, string? observacion = null, DateOnly? fechaVencimiento = null)
    {
        var contrato = Contrato.Crear(terceroId, 2, 3, 4, 5, 6, 7, 1m, fechaDesde, fechaVencimiento ?? fechaDesde.AddMonths(12), fechaDesde, 1, 12, 500m, observacion);
        contrato.GetType().GetProperty(nameof(Contrato.Id))!.SetValue(contrato, id);
        contrato.GetType().GetProperty(nameof(Contrato.CuotasRestantes))!.SetValue(contrato, cuotasRestantes);
        if (estado == "VENCIDO")
            contrato.MarcarVencido();
        if (anulado)
            contrato.Anular("motivo");
        return contrato;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}