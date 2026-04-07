using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Application.Features.Finanzas.Queries;
using ZuluIA_Back.Application.Features.TasasInteres.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CuentaCorrienteControllerTests
{
    [Fact]
    public async Task GetSaldos_CuandoHayDatos_DevuelveOkYDelegaAQuery()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var saldos = new List<CuentaCorrienteDto>
        {
            new()
            {
                TerceroId = 10,
                TerceroRazonSocial = "Cliente Uno",
                SucursalId = 3,
                MonedaId = 1,
                MonedaSimbolo = "$",
                Saldo = 125.5m,
                UpdatedAt = new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero)
            }
        };
        mediator.Send(Arg.Any<GetCuentaCorrienteTerceroQuery>(), Arg.Any<CancellationToken>())
            .Returns(saldos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetSaldos(10, 3, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(saldos);
        await mediator.Received(1).Send(
            Arg.Is<GetCuentaCorrienteTerceroQuery>(q => q.TerceroId == 10 && q.SucursalId == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMovimientos_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var paged = new PagedResult<MovimientoCtaCteDto>(
            [
                new MovimientoCtaCteDto
                {
                    Id = 8,
                    TerceroId = 10,
                    SucursalId = 3,
                    MonedaId = 1,
                    MonedaSimbolo = "$",
                    ComprobanteId = 50,
                    Fecha = new DateOnly(2026, 3, 1),
                    Debe = 100m,
                    Haber = 0m,
                    Saldo = 100m,
                    Descripcion = "Factura A",
                    CreatedAt = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero)
                }
            ],
            2,
            20,
            35);
        mediator.Send(Arg.Any<GetMovimientosCtaCteQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator, db);

        var result = await controller.GetMovimientos(
            10,
            2,
            20,
            3,
            1,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<MovimientoCtaCteDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(20);
        payload.TotalCount.Should().Be(35);
        payload.Items.Should().ContainSingle().Which.Id.Should().Be(8);

        await mediator.Received(1).Send(
            Arg.Is<GetMovimientosCtaCteQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 20 &&
                q.TerceroId == 10 &&
                q.SucursalId == 3 &&
                q.MonedaId == 1 &&
                q.Desde == new DateOnly(2026, 3, 1) &&
                q.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDeudores_CuandoFiltraPorSucursalMonedaYSoloDeudores_DevuelveResumenEnriquecido()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cuenta1 = CreateCuentaCorriente(1, 10, 7, 1, 120m, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero));
        var cuenta2 = CreateCuentaCorriente(2, 20, null, 1, 80m, new DateTimeOffset(2026, 3, 20, 9, 0, 0, TimeSpan.Zero));
        var cuentaExcluidaSucursal = CreateCuentaCorriente(3, 30, 9, 1, 200m, new DateTimeOffset(2026, 3, 19, 8, 0, 0, TimeSpan.Zero));
        var cuentaExcluidaMoneda = CreateCuentaCorriente(4, 40, 7, 2, 300m, new DateTimeOffset(2026, 3, 18, 7, 0, 0, TimeSpan.Zero));
        var cuentaExcluidaAcreedora = CreateCuentaCorriente(5, 50, 7, 1, -50m, new DateTimeOffset(2026, 3, 17, 6, 0, 0, TimeSpan.Zero));
        var tercero1 = CreateTercero(10, "Cliente Uno");
        var tercero2 = CreateTercero(20, "Cliente Dos");
        var moneda = CreateMoneda(1, "$", "ARS");
        var cuentasDbSet = MockDbSetHelper.CreateMockDbSet([
            cuenta1,
            cuenta2,
            cuentaExcluidaSucursal,
            cuentaExcluidaMoneda,
            cuentaExcluidaAcreedora
        ]);
        var tercerosDbSet = MockDbSetHelper.CreateMockDbSet([tercero1, tercero2]);
        var monedasDbSet = MockDbSetHelper.CreateMockDbSet([moneda]);
        db.CuentaCorriente.Returns(cuentasDbSet);
        db.Terceros.Returns(tercerosDbSet);
        db.Monedas.Returns(monedasDbSet);
        var controller = CreateController(mediator, db);

        var result = await controller.GetDeudores(7, 1, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ((IEnumerable<object>)ok.Value!).ToList();
        payload.Should().HaveCount(2);
        AssertAnonymousProperty(payload[0], "TerceroId", 10L);
        AssertAnonymousProperty(payload[0], "TerceroRazonSocial", "Cliente Uno");
        AssertAnonymousProperty(payload[0], "SucursalId", 7L);
        AssertAnonymousProperty(payload[0], "MonedaId", 1L);
        AssertAnonymousProperty(payload[0], "MonedaSimbolo", "$");
        AssertAnonymousProperty(payload[0], "Saldo", 120m);

        AssertAnonymousProperty(payload[1], "TerceroId", 20L);
        AssertAnonymousProperty(payload[1], "TerceroRazonSocial", "Cliente Dos");
        AssertAnonymousProperty(payload[1], "SucursalId", null);
        AssertAnonymousProperty(payload[1], "MonedaId", 1L);
        AssertAnonymousProperty(payload[1], "MonedaSimbolo", "$");
        AssertAnonymousProperty(payload[1], "Saldo", 80m);
    }

    [Fact]
    public async Task GetConIntereses_CuandoHayDatos_DevuelveOkYDelegaAQuery()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var fechaCalculo = new DateOnly(2026, 4, 10);
        List<SaldoConInteresDto> saldos =
        [
            new(10, 1, 3, 100m, 15m, 115m, fechaCalculo)
        ];
        mediator.Send(Arg.Any<GetCuentaCorrienteConInteresQuery>(), Arg.Any<CancellationToken>())
            .Returns(saldos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetConIntereses(10, 3, fechaCalculo, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(saldos);
        await mediator.Received(1).Send(
            Arg.Is<GetCuentaCorrienteConInteresQuery>(q =>
                q.TerceroId == 10 &&
                q.SucursalId == 3 &&
                q.FechaCalculo == fechaCalculo),
            Arg.Any<CancellationToken>());
    }

    private static CuentaCorrienteController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        var controller = new CuentaCorrienteController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CuentaCorriente CreateCuentaCorriente(
        long id,
        long terceroId,
        long? sucursalId,
        long monedaId,
        decimal saldo,
        DateTimeOffset updatedAt)
    {
        var entity = CuentaCorriente.Crear(terceroId, sucursalId, monedaId);
        SetProperty(entity, nameof(CuentaCorriente.Id), id);
        SetProperty(entity, nameof(CuentaCorriente.Saldo), saldo);
        SetProperty(entity, nameof(CuentaCorriente.UpdatedAt), updatedAt);
        return entity;
    }

    private static Tercero CreateTercero(long id, string razonSocial)
    {
        var entity = Tercero.Crear("LEG" + id, razonSocial, 1, id.ToString(), 1, true, false, false, null, null);
        SetProperty(entity, nameof(Tercero.Id), id);
        return entity;
    }

    private static Moneda CreateMoneda(long id, string simbolo, string codigo)
    {
        var entity = (Moneda)Activator.CreateInstance(typeof(Moneda), true)!;
        SetProperty(entity, nameof(Moneda.Id), id);
        SetProperty(entity, nameof(Moneda.Simbolo), simbolo);
        SetProperty(entity, nameof(Moneda.Codigo), codigo);
        SetProperty(entity, nameof(Moneda.Descripcion), codigo);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object? expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}