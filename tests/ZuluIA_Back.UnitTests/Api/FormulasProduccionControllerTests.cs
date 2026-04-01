using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.Commands;
using ZuluIA_Back.Application.Features.Produccion.DTOs;
using ZuluIA_Back.Application.Features.Produccion.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class FormulasProduccionControllerTests
{
    [Fact]
    public async Task GetAll_DevuelveOkYMandaFiltroCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        IReadOnlyList<FormulaProduccionDto> data =
        [
            new FormulaProduccionDto { Id = 1, Codigo = "F001", Descripcion = "Pan", Activo = true }
        ];
        mediator.Send(Arg.Any<GetFormulasProduccionQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetAll(false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetFormulasProduccionQuery>(query => query.SoloActivas == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.GetByIdConIngredientesAsync(99, Arg.Any<CancellationToken>())
            .Returns((FormulaProduccion?)null);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveDetalleEnriquecidoYOrdenado()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var formula = BuildFormulaProduccion(5, "F001", "Pan", 100, 2.5m, 1, true, "horno",
            [
                BuildIngrediente(12, 5, 201, 1.2m, 1, false, 2),
                BuildIngrediente(11, 5, 200, 0.8m, 1, true, 1)
            ]);
        repo.GetByIdConIngredientesAsync(5, Arg.Any<CancellationToken>())
            .Returns(formula);
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildItem(100, "PROD", "Producto final"),
            BuildItem(200, "HAR", "Harina"),
            BuildItem(201, "LEV", "Levadura")
        ]);
        db.Items.Returns(items);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "F001");
        AssertAnonymousProperty(ok.Value!, "ItemResultadoId", 100L);
        AssertAnonymousProperty(ok.Value!, "ItemResultadoCodigo", "PROD");
        AssertAnonymousProperty(ok.Value!, "ItemResultadoDescripcion", "Producto final");

        var ingredientes = ok.Value!.GetType().GetProperty("Ingredientes")!.GetValue(ok.Value)
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        ingredientes.Should().HaveCount(2);
        AssertAnonymousProperty(ingredientes[0], "Id", 11L);
        AssertAnonymousProperty(ingredientes[0], "ItemCodigo", "HAR");
        AssertAnonymousProperty(ingredientes[0], "Orden", (short)1);
        AssertAnonymousProperty(ingredientes[1], "Id", 12L);
        AssertAnonymousProperty(ingredientes[1], "ItemDescripcion", "Levadura");
        AssertAnonymousProperty(ingredientes[1], "Orden", (short)2);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new CreateFormulaProduccionCommand("F001", "Pan", 100, 2.5m, 1, null, []);
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El código ya existe."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Create(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new CreateFormulaProduccionCommand("F001", "Pan", 100, 2.5m, 1, null, []);
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Create(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetFormulaById");
        AssertAnonymousProperty(created.Value!, "id", 15L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateFormulaProduccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la fórmula con ID 8."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Update(8, new UpdateFormulaRequest("Pan integral", 3.5m, "obs"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateFormulaProduccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Update(8, new UpdateFormulaRequest("Pan integral", 3.5m, "obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Fórmula actualizada correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<UpdateFormulaProduccionCommand>(command => command.Id == 8 && command.Descripcion == "Pan integral" && command.CantidadResultado == 3.5m && command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateFormulaProduccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la fórmula con ID 7."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateFormulaProduccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Fórmula desactivada correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<DeactivateFormulaProduccionCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateFormulaProduccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la fórmula con ID 7."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IFormulaProduccionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateFormulaProduccionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Activar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Fórmula activada correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<ActivateFormulaProduccionCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static FormulasProduccionController CreateController(IMediator mediator, IFormulaProduccionRepository repo, IApplicationDbContext db)
    {
        return new FormulasProduccionController(
            mediator,
            repo,
            new ZuluIA_Back.Application.Features.Produccion.Services.FormulaProduccionHistorialService(
                db,
                Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ZuluIA_Back.Domain.Entities.Produccion.FormulaProduccionHistorial>>(),
                Substitute.For<ICurrentUserService>()),
            db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static FormulaProduccion BuildFormulaProduccion(long id, string codigo, string descripcion, long itemResultadoId, decimal cantidadResultado, long? unidadMedidaId, bool activo, string? observacion, IReadOnlyList<FormulaIngrediente> ingredientes)
    {
        var formula = FormulaProduccion.Crear(codigo, descripcion, itemResultadoId, cantidadResultado, unidadMedidaId, observacion, 1);
        formula.GetType().GetProperty(nameof(FormulaProduccion.Id))!.SetValue(formula, id);
        foreach (var ingrediente in ingredientes)
            formula.AgregarIngrediente(ingrediente);

        if (!activo)
            formula.Desactivar(1);

        return formula;
    }

    private static FormulaIngrediente BuildIngrediente(long id, long formulaId, long itemId, decimal cantidad, long? unidadMedidaId, bool esOpcional, short orden)
    {
        var ingrediente = FormulaIngrediente.Crear(formulaId, itemId, cantidad, unidadMedidaId, esOpcional, orden);
        ingrediente.GetType().GetProperty(nameof(FormulaIngrediente.Id))!.SetValue(ingrediente, id);
        return ingrediente;
    }

    private static Item BuildItem(long id, string codigo, string descripcion)
    {
        var item = Item.Crear(codigo, descripcion, 1, 1, 1, true, false, false, true, 0m, 0m, null, 0m, null, null, null, null, null, 1);
        item.GetType().GetProperty(nameof(Item.Id))!.SetValue(item, id);
        return item;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}