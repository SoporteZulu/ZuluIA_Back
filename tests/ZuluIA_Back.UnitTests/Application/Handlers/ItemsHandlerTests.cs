using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.ListasPrecios.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateItemCommandHandler ──────────────────────────────────────────────────

public class CreateItemCommandHandlerTests
{
    private readonly IItemRepository _repo = Substitute.For<IItemRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateItemCommandHandler Sut() => new(_repo, _db, _uow, _user);

    private static CreateItemCommand ValidCommand() => new(
        Codigo: "PROD001",
        Descripcion: "Producto Test",
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
        StockMinimo: 0m,
        StockMaximo: null,
        PuntoReposicion: null,
        StockSeguridad: null,
        Peso: null,
        Volumen: null,
        CodigoAfip: null,
        SucursalId: null);

    [Fact]
    public async Task Handle_CodigoDuplicado_RetornaFailure()
    {
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("código");
    }

    [Fact]
    public async Task Handle_DatosValidos_CreaItemYRetornaId()
    {
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Item>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConComponentes_PersistePackDelItem()
    {
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _user.UserId.Returns((long?)1L);

        var componente = Item.Crear("COMP001", "Componente", 1, 1, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        componente.GetType().GetProperty("Id")!.SetValue(componente, 50L);
        var itemsDbSet = MockDbSetHelper.CreateMockDbSet<Item>([componente]);
        var componentesDbSet = MockDbSetHelper.CreateMockDbSet<ItemComponente>();
        _db.Items.Returns(itemsDbSet);
        _db.ItemsComponentes.Returns(componentesDbSet);

        _repo.When(x => x.AddAsync(Arg.Any<Item>(), Arg.Any<CancellationToken>()))
            .Do(call => call.Arg<Item>().GetType().GetProperty("Id")!.SetValue(call.Arg<Item>(), 100L));

        var command = ValidCommand() with
        {
            Componentes = [new ItemComponenteInput(50L, 2m)]
        };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        componentesDbSet.Should().ContainSingle();
        componentesDbSet.Single().ItemPadreId.Should().Be(100L);
        componentesDbSet.Single().ComponenteId.Should().Be(50L);
        componentesDbSet.Single().Cantidad.Should().Be(2m);
    }
}

// ── UpdateItemCommandHandler ──────────────────────────────────────────────────

public class UpdateItemCommandHandlerTests
{
    private readonly IItemRepository _repo = Substitute.For<IItemRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateItemCommandHandler Sut() => new(_repo, _db, _uow, _user);

    private static UpdateItemCommand ValidCommand(long id = 1) => new(
        Id: id,
        Descripcion: "Producto Actualizado",
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
        StockMinimo: 0m,
        StockMaximo: null,
        PuntoReposicion: null,
        StockSeguridad: null,
        Peso: null,
        Volumen: null,
        CodigoAfip: null);

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Item?)null);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ItemExiste_ActualizaYRetornaSuccess()
    {
        var item = Item.Crear("PROD001", "Producto Test", 1, 1, 1,
                               true, false, false, true, 100m, 150m, null, 0, null,
                               null, null, null, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(ValidCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Item>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConComponentes_ReemplazaPackDelItem()
    {
        var item = Item.Crear("PROD001", "Producto Test", 1, 1, 1,
                               true, false, false, true, 100m, 150m, null, 0, null,
                               null, null, null, null, null);
        item.GetType().GetProperty("Id")!.SetValue(item, 1L);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);
        _user.UserId.Returns((long?)1L);

        var componente = Item.Crear("COMP001", "Componente", 1, 1, 1, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        componente.GetType().GetProperty("Id")!.SetValue(componente, 50L);
        var componenteAnterior = ItemComponente.Crear(1L, 60L, 1m);
        componenteAnterior.GetType().GetProperty("Id")!.SetValue(componenteAnterior, 500L);

        _db.Items.Returns(MockDbSetHelper.CreateMockDbSet<Item>([item, componente]));
        var componentesDbSet = MockDbSetHelper.CreateMockDbSet<ItemComponente>([componenteAnterior]);
        _db.ItemsComponentes.Returns(componentesDbSet);

        var command = ValidCommand(1) with
        {
            Componentes = [new ItemComponenteInput(50L, 3m)]
        };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        componentesDbSet.Should().ContainSingle();
        componentesDbSet.Single().ItemPadreId.Should().Be(1L);
        componentesDbSet.Single().ComponenteId.Should().Be(50L);
        componentesDbSet.Single().Cantidad.Should().Be(3m);
    }
}

// ── DeleteItemCommandHandler ──────────────────────────────────────────────────

public class DeleteItemCommandHandlerTests
{
    private readonly IItemRepository _repo = Substitute.For<IItemRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private DeleteItemCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Item?)null);

        var result = await Sut().Handle(new DeleteItemCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ItemExiste_DesactivaYPersiste()
    {
        var item = Item.Crear("PROD001", "Producto Test", 1, 1, 1,
                               true, false, false, true, 100m, 150m, null, 0, null,
                               null, null, null, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new DeleteItemCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Item>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CreateCategoriaItemCommandHandler ────────────────────────────────────────

public class CreateCategoriaItemCommandHandlerTests
{
    private readonly ICategoriaItemRepository _repo = Substitute.For<ICategoriaItemRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateCategoriaItemCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_CodigoDuplicado_RetornaFailure()
    {
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(
            new CreateCategoriaItemCommand(null, "CAT01", "Categoría", 1, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("categoría");
    }

    [Fact]
    public async Task Handle_DatosValidos_CreaCategoriaYRetornaId()
    {
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(
            new CreateCategoriaItemCommand(null, "CAT01", "Categoría", 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<CategoriaItem>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CreateDepositoCommandHandler ──────────────────────────────────────────────

public class CreateDepositoCommandHandlerTests
{
    private readonly IDepositoRepository _repo = Substitute.For<IDepositoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private CreateDepositoCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_SinDefault_CreaDepositoYRetornaId()
    {
        var result = await Sut().Handle(
            new CreateDepositoCommand(1, "Depósito Principal", false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Deposito>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EstableciendoDefault_UnsetaDefaultAnteriorYCrea()
    {
        var depositoExistente = Deposito.Crear(1, "Depositó Anterior", true);
        _repo.GetDefaultBySucursalAsync(1, Arg.Any<CancellationToken>())
             .Returns(depositoExistente);

        var result = await Sut().Handle(
            new CreateDepositoCommand(1, "Nuevo Default", true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(depositoExistente);
        await _repo.Received(1).AddAsync(Arg.Any<Deposito>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetCategoriasItemsQueryHandler ────────────────────────────────────────────

public class GetCategoriasItemsQueryHandlerTests
{
    private readonly ICategoriaItemRepository _repo = Substitute.For<ICategoriaItemRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetCategoriasItemsQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_SinCategorias_RetornaListaVacia()
    {
        var empty = new List<CategoriaItem>().AsReadOnly();
        _repo.GetArbolCompletoAsync(Arg.Any<CancellationToken>()).Returns(empty);
        _mapper.Map<IReadOnlyList<CategoriaItemDto>>(Arg.Any<IReadOnlyList<CategoriaItem>>())
               .Returns(new List<CategoriaItemDto>().AsReadOnly());

        var result = await Sut().Handle(new GetCategoriasItemsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetDepositosBySucursalQueryHandler ────────────────────────────────────────

public class GetDepositosBySucursalQueryHandlerTests
{
    private readonly IDepositoRepository _repo = Substitute.For<IDepositoRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetDepositosBySucursalQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaMappedList()
    {
        var depositos = new List<Deposito> { Deposito.Crear(1, "Depósito A", false) }.AsReadOnly();
        var dtos = new List<DepositoDto> { new() { Id = 1, Descripcion = "Depósito A" } }.AsReadOnly();

        _repo.GetActivosBySucursalAsync(1, Arg.Any<CancellationToken>()).Returns(depositos);
        _mapper.Map<IReadOnlyList<DepositoDto>>(depositos).Returns(dtos);

        var result = await Sut().Handle(new GetDepositosBySucursalQuery(1), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Descripcion.Should().Be("Depósito A");
    }
}

// ── UpdateItemPreciosCommandHandler ───────────────────────────────────────────

public class UpdateItemPreciosCommandHandlerTests
{
    private readonly IItemRepository _repo = Substitute.For<IItemRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateItemPreciosCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Item?)null);

        var result = await Sut().Handle(
            new UpdateItemPreciosCommand(99, 100m, 150m),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ItemExiste_ActualizaPreciosYRetornaSuccess()
    {
        var item = Item.Crear("PROD001", "Producto Test", 1, 1, 1,
                               true, false, false, true, 100m, 150m, null, 0, null,
                               null, null, null, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(
            new UpdateItemPreciosCommand(1, 120m, 180m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Item>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetItemByIdQueryHandler ───────────────────────────────────────────────────

public class GetItemByIdQueryHandlerTests
{
    private readonly IItemRepository _repo = Substitute.For<IItemRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetItemByIdQueryHandler Sut() => new(_repo, _db, new ItemCommercialStockService(_db));

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaNull()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Item?)null);

        var result = await Sut().Handle(new GetItemByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ItemExiste_RetornaDto()
    {
        var item = Item.Crear("PROD001", "Producto Test", 1, 1, 1,
                               true, false, false, true, 100m, 150m, null, 0, null,
                               null, null, null, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);
        _db.CategoriasItems.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaItem>());
        _db.MarcasComerciales.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comercial.MarcaComercial>());
        var mockUnidadesMedida40 = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.UnidadMedida>();
        _db.UnidadesMedida.Returns(mockUnidadesMedida40);
        var mockAlicuotasIva41 = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.AlicuotaIva>();
        _db.AlicuotasIva.Returns(mockAlicuotasIva41);
        _db.Impuestos.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Impuestos.Impuesto>());
        _db.Depositos.Returns(MockDbSetHelper.CreateMockDbSet<Deposito>());
        _db.ListaPreciosItems.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Precios.ListaPreciosItem>());
        _db.ListasPrecios.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Precios.ListaPrecios>());
        _db.ItemsAtributosComerciales.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comercial.ItemAtributoComercial>());
        _db.AtributosComerciales.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comercial.AtributoComercial>());
        _db.ItemsComponentes.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Items.ItemComponente>());
        _db.Items.Returns(MockDbSetHelper.CreateMockDbSet<Item>([item]));
        _db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Stock.StockItem>());
        _db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comprobantes.ComprobanteItem>());
        _db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comprobantes.Comprobante>());
        _db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.TipoComprobante>());
        _db.TransferenciasDepositoDetalles.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Logistica.TransferenciaDepositoDetalle>());
        _db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Logistica.TransferenciaDeposito>());

        var result = await Sut().Handle(new GetItemByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Codigo.Should().Be("PROD001");
    }
}

// ── GetItemPrecioQueryHandler ─────────────────────────────────────────────────

public class GetItemPrecioQueryHandlerTests
{
    private readonly IItemRepository _itemRepo = Substitute.For<IItemRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetItemPrecioQueryHandler Sut() => new(_itemRepo, _db, new ZuluIA_Back.Application.Features.ListasPrecios.Services.PrecioListaResolutionService(_db), new ItemCommercialStockService(_db));

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaNull()
    {
        _itemRepo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
                 .Returns((Item?)null);

        var result = await Sut().Handle(
            new GetItemPrecioQuery(99, null, null, null),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ItemExiste_RetornaDto()
    {
        var item = Item.Crear("PROD001", "Producto Test", 1, 1, 1,
                               true, false, false, true, 100m, 150m, null, 0, null,
                               null, null, null, null, null);
        _itemRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(item);
        var mockAlicuotasIva42 = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.AlicuotaIva>();
        _db.AlicuotasIva.Returns(mockAlicuotasIva42);
        _db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Stock.StockItem>());
        _db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comprobantes.ComprobanteItem>());
        _db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Comprobantes.Comprobante>());
        _db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.TipoComprobante>());

        var result = await Sut().Handle(
            new GetItemPrecioQuery(1, null, null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Codigo.Should().Be("PROD001");
        result.PrecioVenta.Should().Be(150m);
    }
}

// ── GetItemsPagedQueryHandler ─────────────────────────────────────────────────

public class GetItemsPagedQueryHandlerTests
{
    private readonly IItemRepository _repo = Substitute.For<IItemRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetItemsPagedQueryHandler Sut() => new(_repo, _db, new ItemCommercialStockService(_db));

    [Fact]
    public async Task Handle_SinItems_RetornaPaginaVacia()
    {
        var empty = new PagedResult<Item>([], 1, 20, 0);
        _repo.GetPagedAsync(
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<long?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<bool?>(),
            Arg.Any<CancellationToken>())
            .Returns(empty);
        var mockCategoriasItems43 = MockDbSetHelper.CreateMockDbSet<CategoriaItem>();
        _db.CategoriasItems.Returns(mockCategoriasItems43);
        var mockUnidadesMedida44 = MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Referencia.UnidadMedida>();
        _db.UnidadesMedida.Returns(mockUnidadesMedida44);

        var result = await Sut().Handle(
            new GetItemsPagedQuery(1, 20, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
