using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Configuracion.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class VariablesControllerTests
{
    [Fact]
    public async Task GetAspectos_CuandoFiltraPorPadre_DevuelveOrdenados()
    {
        var controller = CreateController(
            BuildAspectosDbSet(new[]
            {
                BuildAspecto(2, "B", "Beta", 1, 2, 1, "1.2", null),
                BuildAspecto(1, "A", "Alfa", 1, 1, 1, "1.1", null),
                BuildAspecto(3, "C", "Gamma", 2, 1, 1, "2.1", null)
            }),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.GetAspectos(1, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task GetAspectoById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.GetAspectoById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Aspecto 7 no encontrado");
    }

    [Fact]
    public async Task GetAspectoById_CuandoExiste_DevuelveOk()
    {
        var controller = CreateController(
            BuildAspectosDbSet(new[] { BuildAspecto(7, "A", "Alfa", null, 1, 0, "A", "Obs") }),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.GetAspectoById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<Aspecto>().Which.Codigo.Should().Be("A");
    }

    [Fact]
    public async Task CreateAspecto_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateAspectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.CreateAspecto(new CreateAspectoRequest("A", ""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task CreateAspecto_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateAspectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.CreateAspecto(new CreateAspectoRequest("A", "Alfa"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetAspectoById");
        AssertAnonymousProperty(created.Value!, "Id", 21L);
        AssertAnonymousProperty(created.Value!, "Codigo", "A");
    }

    [Fact]
    public async Task UpdateAspecto_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateAspectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Aspecto no encontrado."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.UpdateAspecto(7, new UpdateAspectoRequest("Alfa"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Aspecto no encontrado");
    }

    [Fact]
    public async Task UpdateAspecto_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateAspectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.UpdateAspecto(7, new UpdateAspectoRequest("Alfa"), CancellationToken.None);

        AssertAnonymousProperty(result.Should().BeOfType<OkObjectResult>().Subject.Value!, "Id", 7L);
    }

    [Fact]
    public async Task DeleteAspecto_CuandoHayConflicto_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteAspectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El aspecto tiene variables asociadas."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.DeleteAspecto(7, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("variables asociadas");
    }

    [Fact]
    public async Task DeleteAspecto_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteAspectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.DeleteAspecto(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetVariables_CuandoFiltra_DevuelveOrdenadas()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(new[]
            {
                BuildVariable(2, "B", "Beta", 1, 2, 1, 2, "B", null, null, true),
                BuildVariable(1, "A", "Alfa", 1, 2, 1, 1, "A", null, null, true),
                BuildVariable(3, "C", "Gamma", 9, 2, 1, 1, "C", null, null, true)
            }),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.GetVariables(1, 2, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task GetVariableById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.GetVariableById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Variable 7 no encontrada");
    }

    [Fact]
    public async Task GetVariableById_CuandoExiste_DevuelveOk()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(new[] { BuildVariable(7, "A", "Alfa", 1, 2, 1, 1, "A", "Obs", "Cond", true) }),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.GetVariableById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<Variable>().Which.Codigo.Should().Be("A");
    }

    [Fact]
    public async Task CreateVariable_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.CreateVariable(new CreateVariableRequest("A", ""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task CreateVariable_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(31L));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.CreateVariable(new CreateVariableRequest("A", "Alfa"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetVariableById");
        AssertAnonymousProperty(created.Value!, "Id", 31L);
        AssertAnonymousProperty(created.Value!, "Codigo", "A");
    }

    [Fact]
    public async Task UpdateVariable_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Variable no encontrada."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.UpdateVariable(7, new UpdateVariableRequest("Alfa"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Variable no encontrada");
    }

    [Fact]
    public async Task UpdateVariable_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.UpdateVariable(7, new UpdateVariableRequest("Alfa"), CancellationToken.None);

        AssertAnonymousProperty(result.Should().BeOfType<OkObjectResult>().Subject.Value!, "Id", 7L);
    }

    [Fact]
    public async Task DeleteVariable_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Variable no encontrada."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.DeleteVariable(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Variable no encontrada");
    }

    [Fact]
    public async Task DeleteVariable_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.DeleteVariable(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetOpciones_CuandoHayDatos_DevuelveOrdenadas()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(new[]
            {
                BuildOpcion(2, "B", "Beta", null),
                BuildOpcion(1, "A", "Alfa", null)
            }));

        var result = await controller.GetOpciones(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task GetOpcionById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.GetOpcionById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Opcion de variable 7 no encontrada");
    }

    [Fact]
    public async Task GetOpcionById_CuandoExiste_DevuelveOk()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(new[] { BuildOpcion(7, "A", "Alfa", "Obs") }));

        var result = await controller.GetOpcionById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<OpcionVariable>().Which.Codigo.Should().Be("A");
    }

    [Fact]
    public async Task CreateOpcion_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateOpcionVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.CreateOpcion(new CreateOpcionVariableRequest("A", ""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task CreateOpcion_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateOpcionVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(41L));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.CreateOpcion(new CreateOpcionVariableRequest("A", "Alfa"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetOpcionVariableById");
        AssertAnonymousProperty(created.Value!, "Id", 41L);
        AssertAnonymousProperty(created.Value!, "Codigo", "A");
    }

    [Fact]
    public async Task UpdateOpcion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateOpcionVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Opción no encontrada."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.UpdateOpcion(7, new UpdateOpcionVariableRequest("A", "Alfa"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Opción no encontrada");
    }

    [Fact]
    public async Task UpdateOpcion_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateOpcionVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.UpdateOpcion(7, new UpdateOpcionVariableRequest("A", "Alfa"), CancellationToken.None);

        AssertAnonymousProperty(result.Should().BeOfType<OkObjectResult>().Subject.Value!, "Id", 7L);
    }

    [Fact]
    public async Task DeleteOpcion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteOpcionVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Opción no encontrada."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.DeleteOpcion(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Opción no encontrada");
    }

    [Fact]
    public async Task DeleteOpcion_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteOpcionVariableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()));

        var result = await controller.DeleteOpcion(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetDetalle_CuandoHayDatos_DevuelveFiltradosPorVariable()
    {
        var controller = CreateController(
            BuildAspectosDbSet(Array.Empty<Aspecto>()),
            BuildVariablesDbSet(Array.Empty<Variable>()),
            BuildOpcionesDbSet(Array.Empty<OpcionVariable>()),
            BuildVariablesDetalleDbSet(new[]
            {
                BuildVariableDetalle(2, 7, 11, true, false, 20m, 5m),
                BuildVariableDetalle(1, 7, 10, false, true, 10m, 1m),
                BuildVariableDetalle(3, 8, 12, false, true, 30m, 9m)
            }));

        var result = await controller.GetDetalle(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "VariableId", 7L);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task AddDetalle_CuandoVariableNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddVariableDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Variable no encontrada."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()), BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

        var result = await controller.AddDetalle(7, new VariableDetalleRequest(10, false, true, 10m, 1m), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Variable no encontrada");
    }

    [Fact]
    public async Task AddDetalle_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddVariableDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Valor inválido."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()), BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

        var result = await controller.AddDetalle(7, new VariableDetalleRequest(null, false, true, null, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Valor inválido");
    }

    [Fact]
    public async Task AddDetalle_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddVariableDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(51L));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()), BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

        var result = await controller.AddDetalle(7, new VariableDetalleRequest(10, false, true, 10m, 1m), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(VariablesController.GetDetalle));
        AssertAnonymousProperty(created.Value!, "Id", 51L);
    }

    [Fact]
    public async Task UpdateDetalle_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateVariableDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Detalle no encontrado."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()), BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

        var result = await controller.UpdateDetalle(7, 9, new VariableDetalleRequest(10, false, true, 10m, 1m), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Detalle no encontrado");
    }

    [Fact]
    public async Task UpdateDetalle_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateVariableDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()), BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

        var result = await controller.UpdateDetalle(7, 9, new VariableDetalleRequest(10, false, true, 10m, 1m), CancellationToken.None);

        AssertAnonymousProperty(result.Should().BeOfType<OkObjectResult>().Subject.Value!, "Id", 9L);
    }

    [Fact]
    public async Task DeleteDetalle_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteVariableDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Detalle no encontrado."));
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()), BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

        var result = await controller.DeleteDetalle(7, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Detalle no encontrado");
    }

    [Fact]
    public async Task DeleteDetalle_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteVariableDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildAspectosDbSet(Array.Empty<Aspecto>()), BuildVariablesDbSet(Array.Empty<Variable>()), BuildOpcionesDbSet(Array.Empty<OpcionVariable>()), BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

        var result = await controller.DeleteDetalle(7, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static VariablesController CreateController(DbSet<Aspecto> aspectos, DbSet<Variable> variables, DbSet<OpcionVariable> opciones)
        => CreateController(Substitute.For<IMediator>(), aspectos, variables, opciones, BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

    private static VariablesController CreateController(DbSet<Aspecto> aspectos, DbSet<Variable> variables, DbSet<OpcionVariable> opciones, DbSet<VariableDetalle> detalle)
        => CreateController(Substitute.For<IMediator>(), aspectos, variables, opciones, detalle);

    private static VariablesController CreateController(IMediator mediator, DbSet<Aspecto> aspectos, DbSet<Variable> variables, DbSet<OpcionVariable> opciones)
        => CreateController(mediator, aspectos, variables, opciones, BuildVariablesDetalleDbSet(Array.Empty<VariableDetalle>()));

    private static VariablesController CreateController(IMediator mediator, DbSet<Aspecto> aspectos, DbSet<Variable> variables, DbSet<OpcionVariable> opciones, DbSet<VariableDetalle> detalle)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.Aspectos.Returns(aspectos);
        db.Variables.Returns(variables);
        db.OpcionesVariable.Returns(opciones);
        db.VariablesDetalle.Returns(detalle);

        return new VariablesController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static DbSet<Aspecto> BuildAspectosDbSet(IEnumerable<Aspecto> items)
    {
        var dbSet = MockDbSetHelper.CreateMockDbSet(items.ToArray());
        dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var keys = callInfo.Arg<object[]>();
                var id = (long)keys[0];
                var item = dbSet.FirstOrDefault(x => x.Id == id);
                return new ValueTask<Aspecto?>(item);
            });
        return dbSet;
    }

    private static DbSet<Variable> BuildVariablesDbSet(IEnumerable<Variable> items)
    {
        var dbSet = MockDbSetHelper.CreateMockDbSet(items.ToArray());
        dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var keys = callInfo.Arg<object[]>();
                var id = (long)keys[0];
                var item = dbSet.FirstOrDefault(x => x.Id == id);
                return new ValueTask<Variable?>(item);
            });
        return dbSet;
    }

    private static DbSet<OpcionVariable> BuildOpcionesDbSet(IEnumerable<OpcionVariable> items)
    {
        var dbSet = MockDbSetHelper.CreateMockDbSet(items.ToArray());
        dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var keys = callInfo.Arg<object[]>();
                var id = (long)keys[0];
                var item = dbSet.FirstOrDefault(x => x.Id == id);
                return new ValueTask<OpcionVariable?>(item);
            });
        return dbSet;
    }

    private static DbSet<VariableDetalle> BuildVariablesDetalleDbSet(IEnumerable<VariableDetalle> items)
    {
        return MockDbSetHelper.CreateMockDbSet(items.ToArray());
    }

    private static Aspecto BuildAspecto(long id, string codigo, string descripcion, long? padreId, int orden, int nivel, string? codigoEstructura, string? observacion)
    {
        var entity = Aspecto.Crear(codigo, descripcion, padreId, orden, nivel, codigoEstructura, observacion);
        SetEntityId(entity, id);
        return entity;
    }

    private static Variable BuildVariable(long id, string codigo, string descripcion, long? aspectoId, long? tipoComprobanteId, int nivel, int orden, string? codigoEstructura, string? observacion, string? condicionante, bool editable)
    {
        var entity = Variable.Crear(codigo, descripcion, 1, tipoComprobanteId, aspectoId, nivel, orden, codigoEstructura, observacion, condicionante, editable);
        SetEntityId(entity, id);
        return entity;
    }

    private static OpcionVariable BuildOpcion(long id, string codigo, string descripcion, string? observaciones)
    {
        var entity = OpcionVariable.Crear(codigo, descripcion, observaciones);
        SetEntityId(entity, id);
        return entity;
    }

    private static VariableDetalle BuildVariableDetalle(long id, long variableId, long? opcionVariableId, bool aplicaPuntajePenalizacion, bool visualizarOpcion, decimal? porcentajeIncidencia, decimal? valorObjetivo)
    {
        var entity = VariableDetalle.Crear(variableId, opcionVariableId, aplicaPuntajePenalizacion, visualizarOpcion, porcentajeIncidencia, valorObjetivo);
        SetEntityId(entity, id);
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