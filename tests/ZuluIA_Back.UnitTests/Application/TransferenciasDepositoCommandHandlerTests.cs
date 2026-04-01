using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class TransferenciasDepositoCommandHandlerTests
{
    [Fact]
    public async Task CreateHandle_SinDetalles_RetornaFailure()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var ordenRepo = Substitute.For<IRepository<OrdenPreparacion>>();
        var eventoRepo = Substitute.For<IRepository<LogisticaInternaEvento>>();
        var transferenciaRepo = Substitute.For<IRepository<TransferenciaDeposito>>();
        var stockRepo = Substitute.For<IStockRepository>();
        var movimientoRepo = Substitute.For<IMovimientoStockRepository>();
        var stockService = Substitute.For<StockService>(stockRepo, movimientoRepo);
        var currentUser = Substitute.For<ICurrentUserService>();
        var uow = Substitute.For<IUnitOfWork>();
        var service = new LogisticaInternaService(db, ordenRepo, eventoRepo, transferenciaRepo, stockService, currentUser);
        var handler = new CreateTransferenciaDepositoCommandHandler(service, uow);
        var command = new CreateTransferenciaDepositoCommand(
            SucursalId: 1,
            DepositoOrigenId: 10,
            DepositoDestinoId: 20,
            Fecha: new DateOnly(2025, 1, 1),
            Observacion: null,
            Detalles: []);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("al menos un detalle");
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DespacharHandle_TransferenciaExistente_DespachaYPersiste()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var ordenRepo = Substitute.For<IRepository<OrdenPreparacion>>();
        var eventoRepo = Substitute.For<IRepository<LogisticaInternaEvento>>();
        var transferenciaRepo = Substitute.For<IRepository<TransferenciaDeposito>>();
        var stockRepo = Substitute.For<IStockRepository>();
        var movimientoRepo = Substitute.For<IMovimientoStockRepository>();
        var stockService = Substitute.For<StockService>(stockRepo, movimientoRepo);
        var currentUser = Substitute.For<ICurrentUserService>();
        var uow = Substitute.For<IUnitOfWork>();
        currentUser.UserId.Returns(9L);

        var transferencia = CrearTransferenciaValida();
        db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet([transferencia]));

        var service = new LogisticaInternaService(db, ordenRepo, eventoRepo, transferenciaRepo, stockService, currentUser);
        var handler = new DespacharTransferenciaDepositoCommandHandler(service, uow);

        var result = await handler.Handle(new DespacharTransferenciaDepositoCommand(100), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        transferencia.Estado.Should().Be(ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito.EnTransito);
        transferenciaRepo.Received(1).Update(transferencia);
        await stockService.Received(1).EgresarAsync(
            5,
            10,
            2m,
            ZuluIA_Back.Domain.Enums.TipoMovimientoStock.TransferenciaSalida,
            "transferencias_deposito",
            transferencia.Id,
            transferencia.Observacion,
            9L,
            false,
            Arg.Any<CancellationToken>());
        await uow.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ConfirmarHandle_TransferenciaEnTransito_ConfirmaYPersiste()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var ordenRepo = Substitute.For<IRepository<OrdenPreparacion>>();
        var eventoRepo = Substitute.For<IRepository<LogisticaInternaEvento>>();
        var transferenciaRepo = Substitute.For<IRepository<TransferenciaDeposito>>();
        var stockRepo = Substitute.For<IStockRepository>();
        var movimientoRepo = Substitute.For<IMovimientoStockRepository>();
        var stockService = Substitute.For<StockService>(stockRepo, movimientoRepo);
        var currentUser = Substitute.For<ICurrentUserService>();
        var uow = Substitute.For<IUnitOfWork>();
        currentUser.UserId.Returns(9L);

        var transferencia = CrearTransferenciaValida();
        transferencia.Despachar(DateOnly.FromDateTime(DateTime.Today), null);
        db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet([transferencia]));

        var service = new LogisticaInternaService(db, ordenRepo, eventoRepo, transferenciaRepo, stockService, currentUser);
        var handler = new ConfirmarTransferenciaDepositoCommandHandler(service, uow);

        var result = await handler.Handle(new ConfirmarTransferenciaDepositoCommand(100), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        transferencia.Estado.Should().Be(ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito.Confirmada);
        transferenciaRepo.Received(1).Update(transferencia);
        await stockService.Received(1).IngresarAsync(
            5,
            20,
            2m,
            ZuluIA_Back.Domain.Enums.TipoMovimientoStock.TransferenciaEntrada,
            "transferencias_deposito",
            transferencia.Id,
            transferencia.Observacion,
            9L,
            Arg.Any<CancellationToken>());
        await uow.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task AnularHandle_TransferenciaEnTransito_RevierteYPersiste()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var ordenRepo = Substitute.For<IRepository<OrdenPreparacion>>();
        var eventoRepo = Substitute.For<IRepository<LogisticaInternaEvento>>();
        var transferenciaRepo = Substitute.For<IRepository<TransferenciaDeposito>>();
        var stockRepo = Substitute.For<IStockRepository>();
        var movimientoRepo = Substitute.For<IMovimientoStockRepository>();
        var stockService = Substitute.For<StockService>(stockRepo, movimientoRepo);
        var currentUser = Substitute.For<ICurrentUserService>();
        var uow = Substitute.For<IUnitOfWork>();
        currentUser.UserId.Returns(9L);

        var transferencia = CrearTransferenciaValida();
        transferencia.Despachar(DateOnly.FromDateTime(DateTime.Today), null);
        db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet([transferencia]));

        var service = new LogisticaInternaService(db, ordenRepo, eventoRepo, transferenciaRepo, stockService, currentUser);
        var handler = new AnularTransferenciaDepositoCommandHandler(service, uow);

        var result = await handler.Handle(new AnularTransferenciaDepositoCommand(100), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        transferencia.Estado.Should().Be(ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito.Anulada);
        transferenciaRepo.Received(1).Update(transferencia);
        await stockService.Received(1).IngresarAsync(
            5,
            10,
            2m,
            ZuluIA_Back.Domain.Enums.TipoMovimientoStock.TransferenciaEntrada,
            "transferencias_deposito",
            transferencia.Id,
            Arg.Is<string?>(x => x != null && x.Contains("Reverso por anulación")),
            9L,
            Arg.Any<CancellationToken>());
        await uow.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    private static TransferenciaDeposito CrearTransferenciaValida()
    {
        var transferencia = TransferenciaDeposito.Crear(
            sucursalId: 1,
            depositoOrigenId: 10,
            depositoDestinoId: 20,
            fecha: DateOnly.FromDateTime(DateTime.Today),
            observacion: "Transferencia de prueba",
            userId: null);

        transferencia.AgregarDetalle(5, 2m);
        SetId(transferencia, 100L);
        return transferencia;
    }

    private static void SetId(object entity, long id)
    {
        var property = entity.GetType().BaseType?.GetProperty("Id") ?? entity.GetType().GetProperty("Id");
        property.Should().NotBeNull();
        property!.SetValue(entity, id);
    }
}
