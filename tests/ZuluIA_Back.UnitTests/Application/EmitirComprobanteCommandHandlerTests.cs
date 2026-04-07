using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class EmitirComprobanteCommandHandlerTests
{
    [Fact]
    public async Task Handle_CuandoItemNoEsVendible_DebeRetornarFailure()
    {
        var repo = Substitute.For<IComprobanteRepository>();
        var periodoRepo = Substitute.For<IPeriodoIvaRepository>();
        var afip = Substitute.For<IAfipCaeComprobanteService>();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var db = Substitute.For<IApplicationDbContext>();
        var validationService = new TerceroOperacionValidationService(db);
        var itemCommercialStockService = new ItemCommercialStockService(db);
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(TerceroOperacionValidationService)).Returns(validationService);
        serviceProvider.GetService(typeof(ItemCommercialStockService)).Returns(itemCommercialStockService);
        serviceProvider.GetService(typeof(IAfipCaeComprobanteService)).Returns(afip);
        serviceProvider.GetService(typeof(StockService)).Returns((StockService)null!);

        periodoRepo.EstaAbiertoPeriodoAsync(1L, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(true);

        var tercero = CreateCliente(1L);
        var tipo = CreateTipoComprobante(90L, "FAC", "Factura", true, true);
        var item = Item.Crear("IT001", "Item bloqueado", 10, 21, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        SetId(item, 100L);
        item.ActualizarConfiguracionVentas(false, true, null, false, null);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([tipo]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([item]));

        var handler = new EmitirComprobanteCommandHandler(
            repo,
            periodoRepo,
            uow,
            currentUser,
            db,
            serviceProvider);

        var result = await handler.Handle(CreateCommand(90L, 100L), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no está habilitado para ventas");
        await repo.DidNotReceive().AddAsync(Arg.Any<ZuluIA_Back.Domain.Entities.Comprobantes.Comprobante>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoStockInsuficiente_DebeRetornarFailure()
    {
        var repo = Substitute.For<IComprobanteRepository>();
        var periodoRepo = Substitute.For<IPeriodoIvaRepository>();
        var afip = Substitute.For<IAfipCaeComprobanteService>();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var db = Substitute.For<IApplicationDbContext>();
        var validationService = new TerceroOperacionValidationService(db);
        var itemCommercialStockService = new ItemCommercialStockService(db);
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(TerceroOperacionValidationService)).Returns(validationService);
        serviceProvider.GetService(typeof(ItemCommercialStockService)).Returns(itemCommercialStockService);
        serviceProvider.GetService(typeof(IAfipCaeComprobanteService)).Returns(afip);
        serviceProvider.GetService(typeof(StockService)).Returns((StockService)null!);

        periodoRepo.EstaAbiertoPeriodoAsync(1L, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(true);

        var tercero = CreateCliente(1L);
        var tipo = CreateTipoComprobante(91L, "FAC", "Factura", true, true);
        var item = Item.Crear("IT002", "Item con stock", 10, 21, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        SetId(item, 101L);
        var stock = StockItem.Crear(101L, 1L, 1m);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([tipo]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([item]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet([stock]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comprobantes.ComprobanteItem>([]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comprobantes.Comprobante>([]));

        var handler = new EmitirComprobanteCommandHandler(
            repo,
            periodoRepo,
            uow,
            currentUser,
            db,
            serviceProvider);

        var result = await handler.Handle(CreateCommand(91L, 101L, cantidad: 2m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Stock insuficiente");
        await repo.DidNotReceive().AddAsync(Arg.Any<ZuluIA_Back.Domain.Entities.Comprobantes.Comprobante>(), Arg.Any<CancellationToken>());
    }

    private static EmitirComprobanteCommand CreateCommand(long tipoComprobanteId, long itemId, decimal cantidad = 1m)
        => new(
            null,
            1L,
            null,
            tipoComprobanteId,
            DateOnly.FromDateTime(DateTime.Today),
            null,
            1L,
            1L,
            1m,
            0m,
            null,
            [new ComprobanteItemInput(itemId, null, cantidad, 0, 20, 0, 21L, null, 0)],
            true);

    private static Tercero CreateCliente(long id)
    {
        var tercero = Tercero.Crear("CLI001", "Cliente Factura", 1, "30-11111111-1", 1, true, false, false, null, null);
        SetId(tercero, id);
        return tercero;
    }

    private static TipoComprobante CreateTipoComprobante(long id, string codigo, string descripcion, bool esVenta, bool afectaStock)
    {
        var entity = (TipoComprobante)Activator.CreateInstance(typeof(TipoComprobante), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Codigo")!.SetValue(entity, codigo);
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, descripcion);
        entity.GetType().GetProperty("EsVenta")!.SetValue(entity, esVenta);
        entity.GetType().GetProperty("AfectaStock")!.SetValue(entity, afectaStock);
        return entity;
    }

    private static void SetId<T>(T entity, long id)
        where T : class
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }
}
