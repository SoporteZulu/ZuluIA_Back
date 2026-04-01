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
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class EmpleadosReadControllerTests
{
    [Fact]
    public async Task GetAll_CuandoEstadoEsValido_MapeaFiltroYDevuelvePaginadoConDatosNormalizados()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEmpleadoRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var empleados = new[]
        {
            BuildEmpleado(7, 21, 2, "E-001", "Analista", "RRHH", 1, EstadoEmpleado.Activo),
            BuildEmpleado(8, 22, 3, "E-002", "Supervisor", "Ventas", 2, EstadoEmpleado.Inactivo)
        };
        repo.GetPagedAsync(2, 5, 9, EstadoEmpleado.Activo, "ada", Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Empleado>(empleados, 2, 5, 12));

        var terceros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTercero(21, "EMP001", "Ada Lovelace", "20123456789"),
            BuildTercero(22, "EMP002", "Grace Hopper", "20987654321")
        });
        var monedas = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.Moneda>(Array.Empty<ZuluIA_Back.Domain.Entities.Referencia.Moneda>());
        db.Terceros.Returns(terceros);
        db.Monedas.Returns(monedas);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetAll(2, 5, 9, "Activo", "ada", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "page", 2);
        AssertAnonymousProperty(ok.Value!, "pageSize", 5);
        AssertAnonymousProperty(ok.Value!, "totalCount", 12);
        AssertAnonymousProperty(ok.Value!, "totalPages", 3);
        var data = ((IEnumerable)ok.Value!.GetType().GetProperty("data")!.GetValue(ok.Value!)!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "TerceroRazonSocial", "Ada Lovelace");
        AssertAnonymousProperty(data[0], "TerceroCuit", "20123456789");
        AssertAnonymousProperty(data[0], "MonedaSimbolo", "$");
        AssertAnonymousProperty(data[0], "Estado", "ACTIVO");
        AssertAnonymousProperty(data[1], "Estado", "INACTIVO");
    }

    [Fact]
    public async Task GetAll_CuandoEstadoEsInvalido_NoAplicaFiltroEnumYMantieneRespuesta()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEmpleadoRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.GetPagedAsync(1, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Empleado>([], 1, 10, 0));
        var terceros = MockDbSetHelper.CreateMockDbSet<Tercero>(Array.Empty<Tercero>());
        var monedas = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.Moneda>(Array.Empty<ZuluIA_Back.Domain.Entities.Referencia.Moneda>());
        db.Terceros.Returns(terceros);
        db.Monedas.Returns(monedas);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetAll(1, 10, null, "desconocido", null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "totalCount", 0);
        await repo.Received(1).GetPagedAsync(1, 10, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEmpleadoRepository>();
        repo.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns((Empleado?)null);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el empleado con ID 7");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDatosEnriquecidosYFallbackMoneda()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IEmpleadoRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.GetByIdAsync(7, Arg.Any<CancellationToken>())
            .Returns(BuildEmpleado(7, 21, 2, "E-001", "Analista", "RRHH", 1, EstadoEmpleado.Activo));
        var terceros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTercero(21, "EMP001", "Ada Lovelace", "20123456789")
        });
        var monedas = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.Moneda>(Array.Empty<ZuluIA_Back.Domain.Entities.Referencia.Moneda>());
        var liquidaciones = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildLiquidacion(31, 7, 2, 2026, 3, 100000m, 120000m, 20000m, 1, "Marzo"),
            BuildLiquidacion(32, 7, 2, 2026, 2, 100000m, 118000m, 18000m, 1, "Febrero")
        });
        db.Terceros.Returns(terceros);
        db.Monedas.Returns(monedas);
        db.LiquidacionesSueldo.Returns(liquidaciones);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "TerceroRazonSocial", "Ada Lovelace");
        AssertAnonymousProperty(ok.Value!, "TerceroCuit", "20123456789");
        AssertAnonymousProperty(ok.Value!, "MonedaSimbolo", "$");
        AssertAnonymousProperty(ok.Value!, "Estado", "ACTIVO");
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
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ZuluIA_Back.Domain.Entities.RRHH.ComprobanteEmpleado>>(),
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ZuluIA_Back.Domain.Entities.RRHH.ImputacionEmpleado>>(),
            Substitute.For<ZuluIA_Back.Domain.Interfaces.IRepository<ZuluIA_Back.Domain.Entities.RRHH.LiquidacionSueldo>>(),
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
        long monedaId,
        EstadoEmpleado estado)
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
        if (estado == EstadoEmpleado.Inactivo)
            entity.Egresar(new DateOnly(2025, 12, 31));
        SetEntityId(entity, id);
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