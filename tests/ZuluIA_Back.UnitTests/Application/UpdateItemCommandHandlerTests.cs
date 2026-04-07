using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application;

public class UpdateItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenStockChanges_UpdatesStockInBaseFlow()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(99L);

        var item = Item.Crear(
            "PROD001",
            "Producto original",
            1,
            1,
            1,
            true,
            false,
            false,
            true,
            100m,
            150m,
            null,
            null,
            5m,
            50m,
            null,
            null,
            null,
            1L,
            true,
            true,
            null,
            null,
            false,
            false,
            1L);

        repo.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var handler = new UpdateItemCommandHandler(repo, db, uow, currentUser);
        var command = new UpdateItemCommand(
            Id: item.Id,
            Descripcion: "Producto actualizado",
            DescripcionAdicional: null,
            CodigoBarras: null,
            UnidadMedidaId: 1,
            AlicuotaIvaId: 1,
            AlicuotaIvaCompraId: null,
            MonedaId: 1,
            EsProducto: true,
            EsServicio: false,
            EsFinanciero: false,
            ManejaStock: true,
            CategoriaId: null,
            PrecioCosto: 100m,
            PrecioVenta: 150m,
            StockMinimo: 10m,
            StockMaximo: 100m,
            PuntoReposicion: 15m,
            StockSeguridad: 4m,
            Peso: 2m,
            Volumen: 1m,
            CodigoAfip: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.StockMinimo.Should().Be(10m);
        item.StockMaximo.Should().Be(100m);
        item.PuntoReposicion.Should().Be(15m);
        item.StockSeguridad.Should().Be(4m);
        item.Peso.Should().Be(2m);
        item.Volumen.Should().Be(1m);
        repo.Received(1).Update(item);
        await uow.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenItemIsSistema_ReturnsFailure()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(99L);

        var item = Item.Crear(
            "SIST001",
            "Item sistema",
            1,
            1,
            1,
            true,
            false,
            false,
            true,
            100m,
            150m,
            null,
            null,
            5m,
            50m,
            null,
            null,
            null,
            1L,
            true,
            true,
            null,
            null,
            false,
            true,
            1L);

        repo.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var handler = new UpdateItemCommandHandler(repo, db, uow, currentUser);
        var command = new UpdateItemCommand(
            Id: item.Id,
            Descripcion: "No importa",
            DescripcionAdicional: null,
            CodigoBarras: null,
            UnidadMedidaId: 1,
            AlicuotaIvaId: 1,
            AlicuotaIvaCompraId: null,
            MonedaId: 1,
            EsProducto: true,
            EsServicio: false,
            EsFinanciero: false,
            ManejaStock: true,
            CategoriaId: null,
            PrecioCosto: 100m,
            PrecioVenta: 150m,
            StockMinimo: 5m,
            StockMaximo: 50m,
            PuntoReposicion: null,
            StockSeguridad: null,
            Peso: null,
            Volumen: null,
            CodigoAfip: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("No se puede modificar un item del sistema.");
        repo.DidNotReceive().Update(Arg.Any<Item>());
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenItemHasPorcentajeGananciaAndCostoChanges_RecalculatesPrecioVenta()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(99L);

        var item = Item.Crear(
            "PROD002",
            "Producto con margen",
            1,
            1,
            1,
            true,
            false,
            false,
            true,
            100m,
            0m,
            null,
            null,
            5m,
            50m,
            null,
            null,
            null,
            1L,
            true,
            true,
            20m,
            null,
            false,
            false,
            1L);

        repo.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var handler = new UpdateItemCommandHandler(repo, db, uow, currentUser);
        var command = new UpdateItemCommand(
            Id: item.Id,
            Descripcion: "Producto con margen",
            DescripcionAdicional: null,
            CodigoBarras: null,
            UnidadMedidaId: 1,
            AlicuotaIvaId: 1,
            AlicuotaIvaCompraId: null,
            MonedaId: 1,
            EsProducto: true,
            EsServicio: false,
            EsFinanciero: false,
            ManejaStock: true,
            CategoriaId: null,
            PrecioCosto: 200m,
            PrecioVenta: 0m,
            StockMinimo: 5m,
            StockMaximo: 50m,
            PuntoReposicion: null,
            StockSeguridad: null,
            Peso: null,
            Volumen: null,
            CodigoAfip: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.PrecioCosto.Should().Be(200m);
        item.PrecioVenta.Should().Be(240m);
    }
}
