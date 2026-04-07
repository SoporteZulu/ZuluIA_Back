using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PlanCuentasControllerTests
{
    [Fact]
    public async Task GetArbol_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        IReadOnlyList<PlanCuentaDto> payload =
        [
            new PlanCuentaDto { Id = 1, EjercicioId = 2026, CodigoCuenta = "1", Denominacion = "Activo", OrdenNivel = "001", Subcuentas = [] }
        ];
        mediator.Send(Arg.Any<GetPlanCuentasQuery>(), Arg.Any<CancellationToken>())
            .Returns(payload);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetArbol(2026, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(payload);
        await mediator.Received(1).Send(
            Arg.Is<GetPlanCuentasQuery>(query => query.EjercicioId == 2026 && query.SoloImputables),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPlano_CuandoSoloImputables_UsaRepoImputablesYDevuelveProjection()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        repo.GetImputablesAsync(2026, Arg.Any<CancellationToken>())
            .Returns([
                BuildPlanCuenta(2, 2026, null, "1.1.1", "Caja", 3, "001001001", true, "A", 'D')
            ]);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetPlano(2026, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "CodigoCuenta", "1.1.1");
        AssertAnonymousProperty(items[0], "Imputable", true);
        await repo.Received(1).GetImputablesAsync(2026, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPlano_CuandoNoEsSoloImputables_UsaRepoByEjercicio()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        repo.GetByEjercicioAsync(2026, Arg.Any<CancellationToken>())
            .Returns([
                BuildPlanCuenta(1, 2026, null, "1", "Activo", 1, "001", false, null, null)
            ]);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetPlano(2026, false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "CodigoCuenta", "1");
        AssertAnonymousProperty(items[0], "Imputable", false);
        await repo.Received(1).GetByEjercicioAsync(2026, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Buscar_CuandoTerminoEsMuyCorto_DevuelveBadRequest()
    {
        var controller = CreateController(Substitute.For<IMediator>(), Substitute.For<IPlanCuentasRepository>(), BuildDb());

        var result = await controller.Buscar(2026, "a", true, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "El término de búsqueda debe tener al menos 2 caracteres.");
    }

    [Fact]
    public async Task Buscar_AplicaFiltrosYOrdenaPorCodigoCuenta()
    {
        var cuentas =
            new[]
            {
                BuildPlanCuenta(3, 2026, null, "1.1.2", "Caja dolares", 3, "001001002", true, "A", 'D'),
                BuildPlanCuenta(2, 2026, null, "1.1.1", "Caja pesos", 3, "001001001", true, "A", 'D'),
                BuildPlanCuenta(4, 2026, null, "1.2.1", "Banco", 3, "001002001", false, "A", 'D'),
                BuildPlanCuenta(5, 2025, null, "1.1.0", "Caja vieja", 3, "001001000", true, "A", 'D')
            };
        var controller = CreateController(Substitute.For<IMediator>(), Substitute.For<IPlanCuentasRepository>(), BuildDb(planCuentas: cuentas));

        var result = await controller.Buscar(2026, " caja ", true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "CodigoCuenta", "1.1.1");
        AssertAnonymousProperty(items[1], "Id", 3L);
        AssertAnonymousProperty(items[1], "CodigoCuenta", "1.1.2");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        mediator.Send(Arg.Any<CreatePlanCuentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe la cuenta '1.1.1' en este ejercicio."));
        var controller = CreateController(mediator, repo, db);
        var command = BuildCreateCommand();

        var result = await controller.Create(command, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "Ya existe la cuenta '1.1.1' en este ejercicio.");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        mediator.Send(Arg.Any<CreatePlanCuentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(11L));
        var controller = CreateController(mediator, repo, db);
        var command = BuildCreateCommand();

        var result = await controller.Create(command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 11L);
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        mediator.Send(Arg.Any<UpdatePlanCuentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro la cuenta con ID 9."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Update(9, new UpdatePlanCuentaRequest("Caja", true, "A", 'D'), CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "No se encontro la cuenta con ID 9.");
    }

    [Fact]
    public async Task Update_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        mediator.Send(Arg.Any<UpdatePlanCuentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La denominacion es obligatoria."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Update(9, new UpdatePlanCuentaRequest(" ", true, "A", 'D'), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La denominacion es obligatoria.");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandConIdDeRuta()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPlanCuentasRepository>();
        var db = BuildDb();
        mediator.Send(Arg.Any<UpdatePlanCuentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Update(15, new UpdatePlanCuentaRequest("Caja general", false, "P", 'A'), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Cuenta actualizada correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<UpdatePlanCuentaCommand>(command =>
                command.Id == 15 &&
                command.Denominacion == "Caja general" &&
                !command.Imputable &&
                command.Tipo == "P" &&
                command.SaldoNormal == 'A'),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetParametros_AplicaFiltrosYNormalizaTablaSinTrim()
    {
        PlanCuentaParametro[] parametros =
        [
            BuildPlanCuentaParametro(1, 2026, 10, "iva", 100),
            BuildPlanCuentaParametro(2, 2026, 11, "ventas", 100),
            BuildPlanCuentaParametro(3, 2025, 10, "iva", 200)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), Substitute.For<IPlanCuentasRepository>(), BuildDb(parametros: parametros));

        var result = await controller.GetParametros(2026, "IVA", 100, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Tabla", "iva");
    }

    [Fact]
    public async Task CreateParametro_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IPlanCuentasRepository>(), BuildDb());
        mediator.Send(Arg.Any<CreatePlanCuentaParametroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La cuenta indicada no existe para el ejercicio especificado."));

        var result = await controller.CreateParametro(new PlanCuentaParametroRequest(2026, 10, "IVA", 100), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La cuenta indicada no existe para el ejercicio especificado.");
    }

    [Fact]
    public async Task CreateParametro_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IPlanCuentasRepository>(), BuildDb());
        mediator.Send(Arg.Any<CreatePlanCuentaParametroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(8L));
        var request = new PlanCuentaParametroRequest(2026, 10, "IVA", 100);

        var result = await controller.CreateParametro(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PlanCuentasController.GetParametros));
        created.RouteValues!["ejercicioId"].Should().Be(2026L);
        AssertAnonymousProperty(created.Value!, "Id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<CreatePlanCuentaParametroCommand>(command =>
                command.EjercicioId == 2026 &&
                command.CuentaId == 10 &&
                command.Tabla == "IVA" &&
                command.IdRegistro == 100),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteParametro_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IPlanCuentasRepository>(), BuildDb());
        mediator.Send(Arg.Any<DeletePlanCuentaParametroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Parametro 9 no encontrado."));

        var result = await controller.DeleteParametro(9, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Parametro 9 no encontrado.");
    }

    [Fact]
    public async Task DeleteParametro_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Substitute.For<IPlanCuentasRepository>(), BuildDb());
        mediator.Send(Arg.Any<DeletePlanCuentaParametroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.DeleteParametro(9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static PlanCuentasController CreateController(IMediator mediator, IPlanCuentasRepository repo, IApplicationDbContext db)
    {
        return new PlanCuentasController(mediator, repo, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(IEnumerable<PlanCuenta>? planCuentas = null, IEnumerable<PlanCuentaParametro>? parametros = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var planCuentasDbSet = MockDbSetHelper.CreateMockDbSet(planCuentas);
        var parametrosDbSet = MockDbSetHelper.CreateMockDbSet(parametros);
        db.PlanCuentas.Returns(planCuentasDbSet);
        db.PlanCuentasParametros.Returns(parametrosDbSet);
        return db;
    }

    private static CreatePlanCuentaCommand BuildCreateCommand()
    {
        return new CreatePlanCuentaCommand(2026, 1, "1.1.1", "Caja", 3, "001001001", true, "A", 'D');
    }

    private static PlanCuenta BuildPlanCuenta(long id, long ejercicioId, long? integradoraId, string codigoCuenta, string denominacion, short nivel, string ordenNivel, bool imputable, string? tipo, char? saldoNormal)
    {
        var entity = PlanCuenta.Crear(ejercicioId, integradoraId, codigoCuenta, denominacion, nivel, ordenNivel, imputable, tipo, saldoNormal);
        typeof(PlanCuenta).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static PlanCuentaParametro BuildPlanCuentaParametro(long id, long ejercicioId, long cuentaId, string tabla, long idRegistro)
    {
        var entity = PlanCuentaParametro.Crear(ejercicioId, cuentaId, tabla, idRegistro);
        typeof(PlanCuentaParametro).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}