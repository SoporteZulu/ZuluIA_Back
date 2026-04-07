using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class DespacharOrdenPreparacionCommandHandlerTests
{
    [Fact]
    public async Task Handle_OrdenCompletada_CreaTransferenciaYPersiste()
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
        currentUser.UserId.Returns(7L);

        var orden = CrearOrdenCompletada();
        db.OrdenesPreparacion.Returns(MockDbSetHelper.CreateMockDbSet([orden]));
        db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet<TransferenciaDeposito>());

        var service = new LogisticaInternaService(db, ordenRepo, eventoRepo, transferenciaRepo, stockService, currentUser);
        var handler = new DespacharOrdenPreparacionCommandHandler(service, uow);

        var result = await handler.Handle(
            new DespacharOrdenPreparacionCommand(orden.Id, 20L, DateOnly.FromDateTime(DateTime.Today), "Despacho interno"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        transferenciaRepo.Received(1).Update(Arg.Is<TransferenciaDeposito>(x =>
            x.OrdenPreparacionId == orden.Id &&
            x.DepositoOrigenId == 15L &&
            x.DepositoDestinoId == 20L &&
            x.Detalles.Count == 1));
        await uow.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_OrdenConTransferenciaActiva_RetornaFailure()
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

        var orden = CrearOrdenCompletada();
        var transferenciaExistente = TransferenciaDeposito.Crear(1, 15L, 20L, DateOnly.FromDateTime(DateTime.Today), null, null);
        SetId(transferenciaExistente, 50L);
        transferenciaExistente.VincularOrdenPreparacion(orden.Id, null);

        db.OrdenesPreparacion.Returns(MockDbSetHelper.CreateMockDbSet([orden]));
        db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet([transferenciaExistente]));

        var service = new LogisticaInternaService(db, ordenRepo, eventoRepo, transferenciaRepo, stockService, currentUser);
        var handler = new DespacharOrdenPreparacionCommandHandler(service, uow);

        var result = await handler.Handle(
            new DespacharOrdenPreparacionCommand(orden.Id, 20L, DateOnly.FromDateTime(DateTime.Today), null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ya posee una transferencia interna asociada");
        transferenciaRepo.DidNotReceive().Update(Arg.Any<TransferenciaDeposito>());
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static OrdenPreparacion CrearOrdenCompletada()
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today);
        var orden = OrdenPreparacion.Crear(1, null, null, fecha, null, null);
        SetId(orden, 10L);
        orden.AgregarDetalle(5L, 15L, 2m, null);
        orden.IniciarPreparacion(null);
        orden.RegistrarPicking(0L, 2m, null);
        orden.Confirmar(fecha, null);
        return orden;
    }

    private static void SetId(object target, long id)
    {
        var property = target.GetType().BaseType?.GetProperty("Id") ?? target.GetType().GetProperty("Id");
        property.Should().NotBeNull();
        property!.SetValue(target, id);
    }
}
