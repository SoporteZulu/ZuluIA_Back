using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Application.Features.RRHH.Commands;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class EmpleadosLiquidacionesControllerTests
{
    [Fact]
    public async Task Egresar_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EgresarEmpleadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro el empleado con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Egresar(7, new EgresarEmpleadoRequest(new DateOnly(2026, 3, 20)), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el empleado con ID 7");
    }

    [Fact]
    public async Task Egresar_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EgresarEmpleadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Egresar(7, new EgresarEmpleadoRequest(new DateOnly(2026, 3, 20)), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().Contain("Egreso registrado correctamente");
    }

    [Fact]
    public async Task CreateLiquidacion_CuandoIdNoCoincide_DevuelveBadRequestConError()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.CreateLiquidacion(
            7,
            new CreateLiquidacionSueldoCommand(8, 2, 2026, 3, 100000m, 120000m, 20000m, 1, "Marzo"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ID del empleado no coincide");
    }

    [Fact]
    public async Task CreateLiquidacion_CuandoFalla_DevuelveBadRequestConError()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateLiquidacionSueldoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una liquidacion para ese periodo."));
        var controller = CreateController(mediator);

        var result = await controller.CreateLiquidacion(
            7,
            new CreateLiquidacionSueldoCommand(7, 2, 2026, 3, 100000m, 120000m, 20000m, 1, "Marzo"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe una liquidacion para ese periodo");
    }

    [Fact]
    public async Task CreateLiquidacion_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateLiquidacionSueldoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator);

        var result = await controller.CreateLiquidacion(
            7,
            new CreateLiquidacionSueldoCommand(7, 2, 2026, 3, 100000m, 120000m, 20000m, 1, "Marzo"),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 15L);
    }

    [Fact]
    public async Task GetLiquidaciones_CuandoHayDatos_DevuelveOkFiltradoYOrdenado()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEmpleadoRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(BuildEmpleado(7, 21, 2, "E-001", "Analista", "RRHH", 1));

        var terceros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTercero(21, "EMP001", "Ada Lovelace", "20123456789")
        });
        var empleados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildEmpleado(7, 21, 2, "E-001", "Analista", "RRHH", 1)
        });
        var monedas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMoneda(1, "$")
        });
        var liquidaciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildLiquidacion(31, 7, 2, 2026, 3, 100000m, 120000m, 20000m, 1, "Marzo"),
            BuildLiquidacion(32, 7, 2, 2026, 2, 100000m, 118000m, 18000m, 1, "Febrero"),
            BuildLiquidacion(33, 8, 2, 2026, 3, 100000m, 118000m, 18000m, 1, "Otro")
        });
        db.Empleados.Returns(empleados);
        db.Monedas.Returns(monedas);
        db.Terceros.Returns(terceros);
        db.LiquidacionesSueldo.Returns(liquidaciones);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetLiquidaciones(7, null, 2026, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 31L);
        AssertAnonymousProperty(items[0], "Periodo", "2026/03");
        AssertAnonymousProperty(items[0], "EmpleadoNombre", "Ada Lovelace");
        AssertAnonymousProperty(items[1], "Id", 32L);
    }

    private static EmpleadosController CreateController(
        IMediator mediator,
        IEmpleadoRepository? repo = null,
        IApplicationDbContext? db = null)
    {
        var dbInstance = db ?? Substitute.For<IApplicationDbContext>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var tesoreriaService = new ZuluIA_Back.Application.Features.Tesoreria.Services.TesoreriaService(
            dbInstance,
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ZuluIA_Back.Domain.Entities.Finanzas.TesoreriaMovimiento>>(),
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ZuluIA_Back.Domain.Entities.Finanzas.TesoreriaCierre>>(),
            currentUser);
        var rrhhService = new RrhhService(
            dbInstance,
            repo ?? Substitute.For<IEmpleadoRepository>(),
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ComprobanteEmpleado>>(),
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ImputacionEmpleado>>(),
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<LiquidacionSueldo>>(),
            tesoreriaService,
            new ReporteExportacionService(),
            currentUser);
        var controller = new EmpleadosController(
            mediator,
            repo ?? Substitute.For<IEmpleadoRepository>(),
            dbInstance,
            rrhhService,
            new ReporteExportacionService())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Empleado BuildEmpleado(
        long id,
        long terceroId,
        long sucursalId,
        string legajo,
        string cargo,
        string? area,
        long monedaId)
    {
        var entity = Empleado.Crear(
            terceroId,
            sucursalId,
            legajo,
            cargo,
            area,
            new DateOnly(2024, 1, 10),
            100000m,
            monedaId);
        SetEntityId(entity, id);
        return entity;
    }

    private static Moneda BuildMoneda(long id, string simbolo)
    {
        var entity = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        SetEntityId(entity, id);
        typeof(Moneda).GetProperty(nameof(Moneda.Simbolo))!.SetValue(entity, simbolo);
        return entity;
    }

    private static Tercero BuildTercero(long id, string legajo, string razonSocial, string nroDocumento)
    {
        var entity = Tercero.Crear(legajo, razonSocial, 1, nroDocumento, 1, false, false, true, 2, null);
        SetEntityId(entity, id);
        return entity;
    }

    private static LiquidacionSueldo BuildLiquidacion(
        long id,
        long empleadoId,
        long sucursalId,
        int anio,
        int mes,
        decimal sueldoBasico,
        decimal totalHaberes,
        decimal totalDescuentos,
        long monedaId,
        string? observacion)
    {
        var entity = LiquidacionSueldo.Crear(
            empleadoId,
            sucursalId,
            anio,
            mes,
            sueldoBasico,
            totalHaberes,
            totalDescuentos,
            monedaId,
            observacion);
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