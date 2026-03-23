using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cajas.Commands;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Application.Features.Cajas.Queries;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CajasControllerCoreTests
{
    [Fact]
    public async Task GetBySucursal_EnviaQueryCorrectaYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        IReadOnlyList<CajaListDto> cajas =
        [
            new CajaListDto { Id = 1, Descripcion = "Caja principal", SucursalId = 10, MonedaId = 1, EsCaja = true, Activa = true }
        ];
        mediator.Send(Arg.Any<GetCajasBySucursalQuery>(), Arg.Any<CancellationToken>())
            .Returns(cajas);
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.GetBySucursal(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(cajas);
        await mediator.Received(1).Send(Arg.Is<GetCajasBySucursalQuery>(q => q.SucursalId == 10), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCajaByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((CajaDto?)null);
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.GetById(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new CajaDto { Id = 8, SucursalId = 10, TipoId = 2, Descripcion = "Caja", MonedaId = 1, EsCaja = true, Activa = true };
        mediator.Send(Arg.Any<GetCajaByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.GetById(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task GetTipos_OrdenaPorDescripcion()
    {
        TipoCajaCuenta[] tipos = [BuildTipoCajaCuenta(2, "Banco", false), BuildTipoCajaCuenta(1, "Caja", true)];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(tipos: tipos));

        var result = await controller.GetTipos(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Banco");
        AssertAnonymousProperty(items[1], "Descripcion", "Caja");
    }

    [Fact]
    public async Task GetFormasPago_FiltraPorCajaYHabilitado()
    {
        FormaPagoCaja[] formas =
        [
            BuildFormaPagoCaja(1, 8, 100, 1, true),
            BuildFormaPagoCaja(2, 8, 101, 1, false),
            BuildFormaPagoCaja(3, 9, 102, 1, true)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(formasPago: formas));

        var result = await controller.GetFormasPago(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "FormaPagoId", 100L);
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Create(new CreateCajaCommand(10, 2, "Caja", 1, true, null, null, null, 99), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCajaById");
        created.RouteValues!["id"].Should().Be(12L);
        AssertAnonymousProperty(created.Value!, "id", 12L);
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequest()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.Update(8, new UpdateCajaCommand(9, "Caja", 2, 1, true, null, null, null, 99), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "El ID de la URL no coincide con el del body.");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Update(8, new UpdateCajaCommand(8, "Caja", 2, 1, true, null, null, null, 99), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Caja no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Delete(8, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Delete(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteCajaCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CerrarArqueo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarArqueoCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<int>("No se encontro la caja."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CerrarArqueo(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CerrarArqueo_CuandoTieneExito_DevuelveOkConNumero()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarArqueoCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CerrarArqueo(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "cajaId", 8L);
        AssertAnonymousProperty(ok.Value!, "nroCierre", 15);
    }

    [Fact]
    public async Task AbrirCaja_CuandoFallaValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AbrirCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Saldo inicial invalido."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AbrirCaja(8, new AbrirCajaRequest(new DateOnly(2026, 3, 21), 100), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AbrirCaja_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AbrirCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AbrirCaja(8, new AbrirCajaRequest(new DateOnly(2026, 3, 21), 100), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "cajaId", 8L);
        AssertAnonymousProperty(ok.Value!, "saldoInicial", 100m);
    }

    [Fact]
    public async Task Transferencia_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarTransferenciaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(30L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Transferencia(new RegistrarTransferenciaCommand(10, 8, 9, new DateOnly(2026, 3, 21), 150, 1, 1, "Traspaso"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 30L);
    }

    [Fact]
    public async Task GetTransferencias_AplicaFiltrosYTipoRelativoALaCaja()
    {
        TransferenciaCaja[] transferencias =
        [
            BuildTransferencia(1, 10, 8, 9, new DateOnly(2026, 3, 22), 100, 1, 1, "Egreso", false),
            BuildTransferencia(2, 10, 9, 8, new DateOnly(2026, 3, 21), 90, 1, 1, "Ingreso", false),
            BuildTransferencia(3, 10, 8, 9, new DateOnly(2026, 3, 20), 80, 1, 1, "Anulada", true),
            BuildTransferencia(4, 10, 8, 9, new DateOnly(2026, 2, 20), 70, 1, 1, "Fuera", false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(transferencias: transferencias));

        var result = await controller.GetTransferencias(8, new DateOnly(2026, 3, 21), new DateOnly(2026, 3, 22), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Tipo", "EGRESO");
        AssertAnonymousProperty(items[1], "Id", 2L);
        AssertAnonymousProperty(items[1], "Tipo", "INGRESO");
    }

    [Fact]
    public async Task GetBancos_OrdenaPorDescripcion()
    {
        Banco[] bancos = [BuildBanco(2, "Santander"), BuildBanco(1, "BBVA")];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(bancos: bancos));

        var result = await controller.GetBancos(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "BBVA");
        AssertAnonymousProperty(items[1], "Descripcion", "Santander");
    }

    [Fact]
    public async Task GetBancoById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetBancoById(5, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateBanco_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(5L));
        var controller = CreateController(mediator, BuildDb(bancos: [BuildBanco(5, "Macro")]));

        var result = await controller.CreateBanco(new CreateBancoRequest("Macro"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetBancoById");
        created.RouteValues!["id"].Should().Be(5L);
        AssertAnonymousProperty(created.Value!, "Id", 5L);
        AssertAnonymousProperty(created.Value!, "Descripcion", "Macro");
    }

    [Fact]
    public async Task UpdateBanco_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UpdateBancoResult>("Banco no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateBanco(5, new CreateBancoRequest("Macro"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateBanco_CuandoTieneExito_DevuelveOkConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new UpdateBancoResult(5, "Macro")));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateBanco(5, new CreateBancoRequest("Macro"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Macro");
    }

    [Fact]
    public async Task DeleteBanco_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteBancoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeleteBanco(5, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static CajasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new CajasController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<TipoCajaCuenta>? tipos = null,
        IEnumerable<FormaPagoCaja>? formasPago = null,
        IEnumerable<TransferenciaCaja>? transferencias = null,
        IEnumerable<Banco>? bancos = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var tiposDbSet = MockDbSetHelper.CreateMockDbSet(tipos);
        var formasPagoDbSet = MockDbSetHelper.CreateMockDbSet(formasPago);
        var transferenciasDbSet = MockDbSetHelper.CreateMockDbSet(transferencias);
        var bancosDbSet = MockDbSetHelper.CreateMockDbSet(bancos);

        db.TiposCajaCuenta.Returns(tiposDbSet);
        db.FormasPagoCaja.Returns(formasPagoDbSet);
        db.TransferenciasCaja.Returns(transferenciasDbSet);
        db.Bancos.Returns(bancosDbSet);
        return db;
    }

    private static TipoCajaCuenta BuildTipoCajaCuenta(long id, string descripcion, bool esCaja)
    {
        var entity = (TipoCajaCuenta)Activator.CreateInstance(typeof(TipoCajaCuenta), nonPublic: true)!;
        SetProperty(entity, nameof(TipoCajaCuenta.Id), id);
        SetProperty(entity, nameof(TipoCajaCuenta.Descripcion), descripcion);
        SetProperty(entity, nameof(TipoCajaCuenta.EsCaja), esCaja);
        return entity;
    }

    private static FormaPagoCaja BuildFormaPagoCaja(long id, long cajaId, long formaPagoId, long monedaId, bool habilitado)
    {
        var entity = FormaPagoCaja.Crear(cajaId, formaPagoId, monedaId);
        SetProperty(entity, nameof(FormaPagoCaja.Id), id);
        if (!habilitado)
            entity.Deshabilitar();
        return entity;
    }

    private static TransferenciaCaja BuildTransferencia(long id, long sucursalId, long cajaOrigenId, long cajaDestinoId, DateOnly fecha, decimal importe, long monedaId, decimal cotizacion, string? concepto, bool anulada)
    {
        var entity = TransferenciaCaja.Registrar(sucursalId, cajaOrigenId, cajaDestinoId, fecha, importe, monedaId, cotizacion, concepto, null);
        SetProperty(entity, nameof(TransferenciaCaja.Id), id);
        if (anulada)
            entity.Anular(null);
        return entity;
    }

    private static Banco BuildBanco(long id, string descripcion)
    {
        var entity = Banco.Crear(descripcion);
        SetProperty(entity, nameof(Banco.Id), id);
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