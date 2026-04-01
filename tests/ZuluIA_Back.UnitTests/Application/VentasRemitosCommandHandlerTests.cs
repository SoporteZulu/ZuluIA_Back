using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class VentasRemitosCommandHandlerTests
{
    [Fact]
    public async Task UpsertRemitoCot_CuandoComprobanteNoEsRemito_DebeRetornarFailure()
    {
        var db = CreateDb();
        var currentUser = Substitute.For<ICurrentUserService>();
        var uow = Substitute.For<IUnitOfWork>();

        var comprobante = CreateComprobante(10L, 80L);
        var tipo = CreateTipoComprobante(80L, "FAC", "Factura", true, false);

        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet([comprobante]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([tipo]));
        db.ComprobantesCot.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteCot>([]));

        var handler = new UpsertRemitoCotCommandHandler(db, currentUser, uow);

        var result = await handler.Handle(
            new UpsertRemitoCotCommand(10L, "123456", DateOnly.FromDateTime(DateTime.Today), null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no es un remito");
    }

    [Fact]
    public async Task ReplaceRemitoAtributos_CuandoComprobanteNoEsRemito_DebeRetornarFailure()
    {
        var db = CreateDb();
        var currentUser = Substitute.For<ICurrentUserService>();
        var uow = Substitute.For<IUnitOfWork>();

        var comprobante = CreateComprobante(11L, 81L);
        var tipo = CreateTipoComprobante(81L, "FAC", "Factura", true, false);

        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet([comprobante]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([tipo]));
        db.ComprobantesAtributos.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteAtributo>([]));

        var handler = new ReplaceRemitoAtributosCommandHandler(db, currentUser, uow);

        var result = await handler.Handle(
            new ReplaceRemitoAtributosCommand(11L, [new RemitoAtributoInput("Chofer", "Juan", "TEXTO")]),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no es un remito");
    }

    [Fact]
    public async Task EmitirRemitoValorizado_CuandoTienePrecioCero_DebeRetornarFailure()
    {
        var repo = Substitute.For<IComprobanteRepository>();
        var db = CreateDb();
        var uow = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var sender = Substitute.For<ISender>();
        var validationService = new ZuluIA_Back.Application.Features.Terceros.Services.TerceroOperacionValidationService(db);
        var stockService = new ZuluIA_Back.Application.Features.Items.Services.ItemCommercialStockService(db);

        var tercero = CreateCliente(1L);
        var tipo = CreateTipoComprobante(82L, "REM", "Remito", true, true);
        var item = Item.Crear("IT900", "Item remito", 10, 21, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        SetId(item, 120L);
        var stock = StockItem.Crear(120L, 1L, 10m);

        var comprobante = CreateComprobante(12L, 82L);
        var linea = ComprobanteItem.Crear(12L, 120L, "Item remito", 2m, 0, 0, 0, 21L, 21L, null, 0);
        comprobante.AgregarItem(linea);

        repo.GetByIdConItemsAsync(12L, Arg.Any<CancellationToken>()).Returns(comprobante);
        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>([]));
        db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>([]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet([tipo]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([item]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet([stock]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet([comprobante]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet([linea]));

        var handler = new EmitirDocumentoVentaCommandHandler(
            repo,
            db,
            uow,
            currentUser,
            null!,
            validationService,
            sender,
            stockService);

        var result = await handler.Handle(
            new EmitirDocumentoVentaCommand(12L, OperacionStockVenta.Egreso, OperacionCuentaCorrienteVenta.Debito),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("precio mayor a cero");
        repo.DidNotReceive().Update(Arg.Any<Comprobante>());
    }

    private static IApplicationDbContext CreateDb() => Substitute.For<IApplicationDbContext>();

    private static Comprobante CreateComprobante(long id, long tipoComprobanteId)
    {
        var comprobante = Comprobante.Crear(1L, null, tipoComprobanteId, 1, 123, DateOnly.FromDateTime(DateTime.Today), null, 1L, 1L, 1m, null, 1L);
        SetId(comprobante, id);
        return comprobante;
    }

    private static Tercero CreateCliente(long id)
    {
        var tercero = Tercero.Crear("CLI001", "Cliente Remito", 1, "30-11111111-1", 1, true, false, false, null, null);
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
