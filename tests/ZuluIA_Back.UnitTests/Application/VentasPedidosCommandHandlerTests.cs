using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class VentasPedidosCommandHandlerTests
{
    [Fact]
    public async Task CrearBorrador_CuandoItemNoEsVendible_DebeRetornarFailure()
    {
        var repo = Substitute.For<IComprobanteRepository>();
        var db = CreateDb();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var validationService = new TerceroOperacionValidationService(db);
        var stockService = new ItemCommercialStockService(db);

        var tercero = CreateCliente(1L, "CLI001", "Cliente Ventas");
        var item = Item.Crear("IT001", "Item bloqueado", 10, 21, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        SetId(item, 100L);
        item.ActualizarConfiguracionVentas(false, true, null, false, null);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([CreateTipoComprobante(50L, "NP", "Nota de pedido", true, false)]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([item]));
        db.AlicuotasIva.Returns(MockDbSetHelper.CreateMockDbSet([CreateAlicuota(21L, 21, "IVA 21%")]));
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet<TerceroPerfilComercial>([]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<StockItem>([]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>([]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>([]));

        var handler = new CrearBorradorVentaCommandHandler(repo, db, uow, currentUser, validationService, stockService);

        var result = await handler.Handle(CreatePedidoCommand(100L), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no está habilitado para ventas");
        await repo.DidNotReceive().AddAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearBorrador_CuandoStockInsuficiente_DebeRetornarFailure()
    {
        var repo = Substitute.For<IComprobanteRepository>();
        var db = CreateDb();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var validationService = new TerceroOperacionValidationService(db);
        var stockService = new ItemCommercialStockService(db);

        var tercero = CreateCliente(1L, "CLI001", "Cliente Ventas");
        var item = Item.Crear("IT002", "Item con stock", 10, 21, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        SetId(item, 101L);
        var stock = StockItem.Crear(101L, 1L, 2m);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([CreateTipoComprobante(50L, "NP", "Nota de pedido", true, false)]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([item]));
        db.AlicuotasIva.Returns(MockDbSetHelper.CreateMockDbSet([CreateAlicuota(21L, 21, "IVA 21%")]));
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet<TerceroPerfilComercial>([]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet([stock]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>([]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>([]));

        var handler = new CrearBorradorVentaCommandHandler(repo, db, uow, currentUser, validationService, stockService);

        var result = await handler.Handle(CreatePedidoCommand(101L, 3m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Stock insuficiente");
        await repo.DidNotReceive().AddAsync(Arg.Any<Comprobante>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EmitirPedido_CuandoStockInsuficiente_DebeRetornarFailure()
    {
        var repo = Substitute.For<IComprobanteRepository>();
        var db = CreateDb();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var sender = Substitute.For<ISender>();
        var validationService = new TerceroOperacionValidationService(db);
        var stockService = new ItemCommercialStockService(db);

        var tercero = CreateCliente(1L, "CLI001", "Cliente Ventas");
        var tipo = CreateTipoComprobante(50L, "NP", "Nota de pedido", true, false);
        var item = Item.Crear("IT003", "Item emitible", 10, 21, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        SetId(item, 102L);
        var stock = StockItem.Crear(102L, 1L, 2m);

        var comprobante = Comprobante.Crear(1L, null, 50L, 1, 123, DateOnly.FromDateTime(DateTime.Today), null, 1L, 1L, 1m, null, 1L);
        SetId(comprobante, 900L);
        var linea = ComprobanteItem.Crear(900L, 102L, "Item emitible", 3m, 0, 20, 0, 21L, 21L, null, 0);
        comprobante.AgregarItem(linea);

        repo.GetByIdConItemsAsync(900L, Arg.Any<CancellationToken>()).Returns(comprobante);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([tipo]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([item]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet([stock]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet([comprobante]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet([linea]));

        var emitirValidationService = new EmitirDocumentoVentaValidationService(db, validationService, stockService);

        var handler = new EmitirDocumentoVentaCommandHandler(
            repo,
            db,
            uow,
            currentUser,
            null!,
            sender,
            emitirValidationService);

        var result = await handler.Handle(
            new EmitirDocumentoVentaCommand(900L, OperacionStockVenta.Ninguna, OperacionCuentaCorrienteVenta.Ninguna),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Stock insuficiente");
        repo.DidNotReceive().Update(Arg.Any<Comprobante>());
    }

    private static CrearBorradorVentaCommand CreatePedidoCommand(long itemId, decimal cantidad = 1m)
        => new(
            1L,
            null,
            50L,
            DateOnly.FromDateTime(DateTime.Today),
            null,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            1L,
            1L,
            1m,
            0m,
            null,
            null,
            [new ComprobanteItemInput(itemId, null, cantidad, 0, 20, 0, 21L, null, 0)]);

    private static IApplicationDbContext CreateDb()
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.PuntosFacturacion.Returns(MockDbSetHelper.CreateMockDbSet<PuntoFacturacion>([]));
        return db;
    }

    private static Tercero CreateCliente(long id, string legajo, string razonSocial)
    {
        var tercero = Tercero.Crear(legajo, razonSocial, 1, "30-11111111-1", 1, true, false, false, null, null);
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

    private static AlicuotaIva CreateAlicuota(long id, long porcentaje, string descripcion)
    {
        var entity = (AlicuotaIva)Activator.CreateInstance(typeof(AlicuotaIva), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Codigo")!.SetValue(entity, (short)1);
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, descripcion);
        entity.GetType().GetProperty("Porcentaje")!.SetValue(entity, porcentaje);
        return entity;
    }

    private static void SetId<T>(T entity, long id)
        where T : class
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }
}
