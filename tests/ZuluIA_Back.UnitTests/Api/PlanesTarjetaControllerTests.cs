using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PlanesTarjetaControllerTests
{
    [Fact]
    public async Task GetAll_CuandoFiltraPorTarjetaYActivo_DevuelveElementosEsperados()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var planes = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildPlanTarjeta(2, 1, "03", "3 cuotas", 3, 12m, 30, true),
            BuildPlanTarjeta(1, 1, "01", "Contado", 1, 0m, 1, true),
            BuildPlanTarjeta(3, 2, "06", "6 cuotas", 6, 18m, 45, true),
            BuildPlanTarjeta(4, 1, "12", "12 cuotas", 12, 24m, 60, false)
        });
        db.PlanesTarjeta.Returns(planes);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(1, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "01");
        AssertAnonymousProperty(items[1], "Codigo", "03");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var planes = MockDbSetHelper.CreateMockDbSet(Array.Empty<PlanTarjeta>());
        db.PlanesTarjeta.Returns(planes);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConItem()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var planes = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildPlanTarjeta(7, 1, "03", "3 cuotas", 3, 12m, 30, true)
        });
        db.PlanesTarjeta.Returns(planes);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "TarjetaTipoId", 1L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "03");
        AssertAnonymousProperty(ok.Value!, "CantidadCuotas", 3);
    }

    [Fact]
    public async Task Create_CuandoNoExisteTarjeta_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("No existe la tarjeta indicada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new PlanTarjetaRequest(9, "03", "3 cuotas", 3, 12m, 30), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No existe la tarjeta indicada");
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un plan con ese código para la tarjeta."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new PlanTarjetaRequest(1, "03", "3 cuotas", 3, 12m, 30), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe un plan con ese código");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(25L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new PlanTarjetaRequest(1, "03", "3 cuotas", 3, 12m, 30), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PlanesTarjetaController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 25L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Plan no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new PlanTarjetaRequest(1, "03", "3 cuotas", 3, 12m, 30), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Plan no encontrado");
    }

    [Fact]
    public async Task Update_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La cantidad de cuotas debe ser mayor a 0."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new PlanTarjetaRequest(1, "03", "3 cuotas", 0, 12m, 30), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("cantidad de cuotas debe ser mayor a 0");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new PlanTarjetaRequest(1, "03", "3 cuotas", 3, 12m, 30), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Plan no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Plan no encontrado");
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivatePlanTarjetaCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Plan no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Plan no encontrado");
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivatePlanTarjetaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivatePlanTarjetaCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static PlanesTarjetaController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new PlanesTarjetaController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static PlanTarjeta BuildPlanTarjeta(long id, long tarjetaTipoId, string codigo, string descripcion, int cuotas, decimal recargo, int diasAcreditacion, bool activo)
    {
        var entity = PlanTarjeta.Crear(tarjetaTipoId, codigo, descripcion, cuotas, recargo, diasAcreditacion, null);
        SetEntityId(entity, id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static void SetEntityId(object entity, long id)
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var property = type.GetProperty("Id");
            if (property is not null)
            {
                property.SetValue(entity, id);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("No se pudo localizar la propiedad Id.");
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}