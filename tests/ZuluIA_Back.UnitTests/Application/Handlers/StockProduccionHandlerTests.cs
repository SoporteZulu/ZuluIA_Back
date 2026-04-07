using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.Commands;
using ZuluIA_Back.Application.Features.Produccion.Queries;
using ZuluIA_Back.Application.Features.Stock.Commands;
using ZuluIA_Back.Application.Features.Stock.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── AjusteStockCommandHandler ─────────────────────────────────────────────────

public class AjusteStockCommandHandlerTests
{
    private readonly StockService _stockService = Substitute.For<StockService>(
        Substitute.For<IStockRepository>(),
        Substitute.For<IMovimientoStockRepository>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private AjusteStockCommandHandler Sut() => new(_stockService, _uow, _user);

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaIdMovimiento()
    {
        _user.UserId.Returns((long?)1L);
        var movimiento = MovimientoStock.Crear(1, 1,
            TipoMovimientoStock.AjustePositivo,
            100m, 100m, null, null, null, null);
        _stockService
            .AjustarAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<decimal>(),
                Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(movimiento);

        var cmd = new AjusteStockCommand(1, 1, 100m, "Ajuste inicial");
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── StockInicialCommandHandler ────────────────────────────────────────────────

public class StockInicialCommandHandlerTests
{
    private readonly StockService _stockService = Substitute.For<StockService>(
        Substitute.For<IStockRepository>(),
        Substitute.For<IMovimientoStockRepository>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private StockInicialCommandHandler Sut() => new(_stockService, _uow, _user);

    [Fact]
    public async Task Handle_SinItems_RetornaFailure()
    {
        var cmd = new StockInicialCommand([], null);
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConItems_LlamaAjustarYRetornaCantidad()
    {
        _user.UserId.Returns((long?)1L);
        var movimiento = MovimientoStock.Crear(1, 1,
            TipoMovimientoStock.StockInicial,
            50m, 50m, null, null, null, null);
        _stockService
            .AjustarAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<decimal>(),
                Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(movimiento);

        var items = new List<StockInicialItemDto>
        {
            new(1, 1, 50m),
            new(2, 1, 30m)
        };
        var cmd = new StockInicialCommand(items, "Carga inicial");
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── TransferenciaStockCommandHandler ─────────────────────────────────────────

public class TransferenciaStockCommandHandlerTests
{
    private readonly StockService _stockService = Substitute.For<StockService>(
        Substitute.For<IStockRepository>(),
        Substitute.For<IMovimientoStockRepository>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private TransferenciaStockCommandHandler Sut() => new(_stockService, _uow, _user);

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaSuccess()
    {
        _user.UserId.Returns((long?)1L);
        _stockService
            .TransferirAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<long>(),
                Arg.Any<decimal>(), Arg.Any<string?>(), Arg.Any<long?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var cmd = new TransferenciaStockCommand(1, 1, 2, 10m, "Transferencia");
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetMovimientosStockPagedQueryHandler ──────────────────────────────────────

public class GetMovimientosStockPagedQueryHandlerTests
{
    private readonly IMovimientoStockRepository _repo = Substitute.For<IMovimientoStockRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetMovimientosStockPagedQueryHandler Sut() => new(_repo, _db);

    [Fact]
    public async Task Handle_ConResultadoVacio_RetornaPagedResultVacio()
    {
        _repo.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<long?>(), Arg.Any<long?>(),
                Arg.Any<TipoMovimientoStock?>(),
                Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<MovimientoStock>([], 1, 20, 0));

        var mockItems49 = MockDbSetHelper.CreateMockDbSet<Item>();

        _db.Items.Returns(mockItems49);
        var mockDepositos50 = MockDbSetHelper.CreateMockDbSet<Deposito>();
        _db.Depositos.Returns(mockDepositos50);

        var query = new GetMovimientosStockPagedQuery(1, 20, null, null, null, null, null);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}

// ── CrearOrdenTrabajoCommandHandler ──────────────────────────────────────────

public class CrearOrdenTrabajoCommandHandlerTests
{
    private readonly IOrdenTrabajoRepository _repo = Substitute.For<IOrdenTrabajoRepository>();
    private readonly IFormulaProduccionRepository _formulaRepo = Substitute.For<IFormulaProduccionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private CrearOrdenTrabajoCommandHandler Sut() => new(_repo, _formulaRepo, _uow, _user);

    private static CrearOrdenTrabajoCommand CmdValido() => new(
        SucursalId: 1,
        FormulaId: 1,
        DepositoOrigenId: 1,
        DepositoDestinoId: 2,
        Fecha: DateOnly.FromDateTime(DateTime.Today),
        FechaFinPrevista: null,
        Cantidad: 5m,
        Observacion: null);

    [Fact]
    public async Task Handle_FormulaNoEncontrada_RetornaFailure()
    {
        _formulaRepo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((FormulaProduccion?)null);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FormulaInactiva_RetornaFailure()
    {
        var formula = FormulaProduccion.Crear("FP001", "Formula Test", 1, 1m, null, null, null);
        formula.Desactivar(null);
        _formulaRepo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(formula);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FormulaActiva_CreaOrdenYRetornaId()
    {
        _user.UserId.Returns((long?)1L);
        var formula = FormulaProduccion.Crear("FP001", "Formula Test", 1, 1m, null, null, null);
        _formulaRepo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(formula);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<OrdenTrabajo>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CreateFormulaProduccionCommandHandler ─────────────────────────────────────

public class CreateFormulaProduccionCommandHandlerTests
{
    private readonly IFormulaProduccionRepository _repo = Substitute.For<IFormulaProduccionRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private CreateFormulaProduccionCommandHandler Sut() => new(
        _repo,
        new ZuluIA_Back.Application.Features.Produccion.Services.FormulaProduccionHistorialService(
            _db,
            Substitute.For<IRepository<FormulaProduccionHistorial>>(),
            _user),
        _uow,
        _user);

    private static CreateFormulaProduccionCommand CmdValido() => new(
        Codigo: "FP001",
        Descripcion: "Fórmula Test",
        ItemResultadoId: 1,
        CantidadResultado: 10m,
        UnidadMedidaId: null,
        Observacion: null,
        Ingredientes: [new IngredienteInput(1, 2m, null, false, 1)]);

    [Fact]
    public async Task Handle_CodigoDuplicado_RetornaFailure()
    {
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SinIngredientes_RetornaFailure()
    {
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var cmd = CmdValido() with { Ingredientes = [] };
        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_CreaFormulaYRetornaId()
    {
        _user.UserId.Returns((long?)1L);
        _repo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await Sut().Handle(CmdValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<FormulaProduccion>(), Arg.Any<CancellationToken>());
        await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── FinalizarOrdenTrabajoCommandHandler ──────────────────────────────────────

public class IniciarOrdenTrabajoCommandHandlerTests
{
    private readonly IOrdenTrabajoRepository _repo = Substitute.For<IOrdenTrabajoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private IniciarOrdenTrabajoCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_OrdenNoEncontrada_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((OrdenTrabajo?)null);

        var result = await Sut().Handle(new IniciarOrdenTrabajoCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OrdenPendiente_IniciaYGuarda()
    {
        _user.UserId.Returns((long?)1L);
        var ot = OrdenTrabajo.Crear(1, 1, 1, 2,
            DateOnly.FromDateTime(DateTime.Today), null,
            5m, null, null);

        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(ot);

        var result = await Sut().Handle(new IniciarOrdenTrabajoCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ot.Estado.Should().Be(EstadoOrdenTrabajo.EnProceso);
        _repo.Received(1).Update(Arg.Any<OrdenTrabajo>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class CancelarOrdenTrabajoCommandHandlerTests
{
    private readonly IOrdenTrabajoRepository _repo = Substitute.For<IOrdenTrabajoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private CancelarOrdenTrabajoCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_OrdenNoEncontrada_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((OrdenTrabajo?)null);

        var result = await Sut().Handle(new CancelarOrdenTrabajoCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OrdenPendiente_CancelaYGuarda()
    {
        _user.UserId.Returns((long?)1L);
        var ot = OrdenTrabajo.Crear(1, 1, 1, 2,
            DateOnly.FromDateTime(DateTime.Today), null,
            5m, null, null);

        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(ot);

        var result = await Sut().Handle(new CancelarOrdenTrabajoCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ot.Estado.Should().Be(EstadoOrdenTrabajo.Cancelada);
        _repo.Received(1).Update(Arg.Any<OrdenTrabajo>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class FinalizarOrdenTrabajoCommandHandlerTests
{
    private readonly IOrdenTrabajoRepository _repo = Substitute.For<IOrdenTrabajoRepository>();
    private readonly IFormulaProduccionRepository _formulaRepo = Substitute.For<IFormulaProduccionRepository>();
    private readonly IRepository<OrdenTrabajoConsumo> _consumoRepo = Substitute.For<IRepository<OrdenTrabajoConsumo>>();
    private readonly ProduccionService _produccionService = Substitute.For<ProduccionService>(
        Substitute.For<IFormulaProduccionRepository>(),
        Substitute.For<StockService>(
            Substitute.For<IStockRepository>(),
            Substitute.For<IMovimientoStockRepository>()),
        Substitute.For<IRepository<OrdenTrabajoConsumo>>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private FinalizarOrdenTrabajoCommandHandler Sut() => new(_repo, _produccionService, _uow, _user);

    [Fact]
    public async Task Handle_OrdenNoEncontrada_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((OrdenTrabajo?)null);

        var result = await Sut().Handle(
            new FinalizarOrdenTrabajoCommand(1, DateOnly.FromDateTime(DateTime.Today)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OrdenEncontrada_FinalizaYGuarda()
    {
        _user.UserId.Returns((long?)1L);
        var formula = FormulaProduccion.Crear("FP001", "Formula Test", 1, 1m, null, null, null);
        var ot = OrdenTrabajo.Crear(1, 1, 1, 2,
            DateOnly.FromDateTime(DateTime.Today), null,
            5m, null, null);
        ot.Iniciar(null);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(ot);
        _produccionService
            .EjecutarProduccionAsync(Arg.Any<OrdenTrabajo>(), Arg.Any<decimal?>(), Arg.Any<IReadOnlyDictionary<long, decimal>?>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await Sut().Handle(
            new FinalizarOrdenTrabajoCommand(1, DateOnly.FromDateTime(DateTime.Today)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<OrdenTrabajo>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetFormulasProduccionQueryHandler ────────────────────────────────────────

public class GetFormulasProduccionQueryHandlerTests
{
    private readonly IFormulaProduccionRepository _repo = Substitute.For<IFormulaProduccionRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetFormulasProduccionQueryHandler Sut() => new(_repo, _db);

    [Fact]
    public async Task Handle_SoloActivas_LlamaGetActivasAsync()
    {
        _repo.GetActivasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<FormulaProduccion>().AsReadOnly() as IReadOnlyList<FormulaProduccion>);

        var mockItems51 = MockDbSetHelper.CreateMockDbSet<Item>();

        _db.Items.Returns(mockItems51);
        var mockUnidadesMedida52 = MockDbSetHelper.CreateMockDbSet<UnidadMedida>();
        _db.UnidadesMedida.Returns(mockUnidadesMedida52);

        var result = await Sut().Handle(new GetFormulasProduccionQuery(true), CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.Received(1).GetActivasAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TodasFormulas_UsaDbContext()
    {
        var mockFormulasProduccion53 = MockDbSetHelper.CreateMockDbSet<FormulaProduccion>();
        _db.FormulasProduccion.Returns(mockFormulasProduccion53);
        var mockItems54 = MockDbSetHelper.CreateMockDbSet<Item>();
        _db.Items.Returns(mockItems54);
        var mockUnidadesMedida55 = MockDbSetHelper.CreateMockDbSet<UnidadMedida>();
        _db.UnidadesMedida.Returns(mockUnidadesMedida55);

        var result = await Sut().Handle(new GetFormulasProduccionQuery(false), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetOrdenesTrabajoPagedQueryHandler ───────────────────────────────────────

public class GetOrdenesTrabajoPagedQueryHandlerTests
{
    private readonly IOrdenTrabajoRepository _repo = Substitute.For<IOrdenTrabajoRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private GetOrdenesTrabajoPagedQueryHandler Sut() => new(_repo, _db);

    [Fact]
    public async Task Handle_ConResultadoVacio_RetornaPagedResultVacio()
    {
        _repo.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<long?>(), Arg.Any<long?>(),
                Arg.Any<EstadoOrdenTrabajo?>(),
                Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<OrdenTrabajo>([], 1, 20, 0));

        var mockFormulasProduccion56 = MockDbSetHelper.CreateMockDbSet<FormulaProduccion>();

        _db.FormulasProduccion.Returns(mockFormulasProduccion56);
        var mockDepositos57 = MockDbSetHelper.CreateMockDbSet<Deposito>();
        _db.Depositos.Returns(mockDepositos57);

        var query = new GetOrdenesTrabajoPagedQuery(1, 20, null, null, null, null, null);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}

// ── GetStockBajoMinimoQueryHandler ────────────────────────────────────────────

public class GetStockBajoMinimoQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetStockBajoMinimoQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_SinStockBajoMinimo_RetornaListaVacia()
    {
        var mockStock58 = MockDbSetHelper.CreateMockDbSet<StockItem>();
        _db.Stock.Returns(mockStock58);
        var mockItems59 = MockDbSetHelper.CreateMockDbSet<Item>();
        _db.Items.Returns(mockItems59);
        var mockDepositos60 = MockDbSetHelper.CreateMockDbSet<Deposito>();
        _db.Depositos.Returns(mockDepositos60);

        var result = await Sut().Handle(
            new GetStockBajoMinimoQuery(null, null),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FiltrandoPorDeposito_RetornaListaVacia()
    {
        var mockStock61 = MockDbSetHelper.CreateMockDbSet<StockItem>();
        _db.Stock.Returns(mockStock61);
        var mockItems62 = MockDbSetHelper.CreateMockDbSet<Item>();
        _db.Items.Returns(mockItems62);
        var mockDepositos63 = MockDbSetHelper.CreateMockDbSet<Deposito>();
        _db.Depositos.Returns(mockDepositos63);

        var result = await Sut().Handle(
            new GetStockBajoMinimoQuery(null, 1L),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetStockByDepositoQueryHandler ────────────────────────────────────────────

public class GetStockByDepositoQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetStockByDepositoQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_DepositoSinStock_RetornaListaVacia()
    {
        var mockStock64 = MockDbSetHelper.CreateMockDbSet<StockItem>();
        _db.Stock.Returns(mockStock64);
        var mockItems65 = MockDbSetHelper.CreateMockDbSet<Item>();
        _db.Items.Returns(mockItems65);
        var mockDepositos66 = MockDbSetHelper.CreateMockDbSet<Deposito>();
        _db.Depositos.Returns(mockDepositos66);

        var result = await Sut().Handle(
            new GetStockByDepositoQuery(1L),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetStockByItemQueryHandler ────────────────────────────────────────────────

public class GetStockByItemQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetStockByItemQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_ItemNoExiste_RetornaNull()
    {
        var mockItems67 = MockDbSetHelper.CreateMockDbSet<Item>();
        _db.Items.Returns(mockItems67);

        var result = await Sut().Handle(
            new GetStockByItemQuery(99L),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ItemExiste_RetornaStockResumen()
    {
        // Item with Id = 0 (default), query uses ItemId = 0
        var item = Item.Crear("PROD001", "Producto Test", 1, 1, 1,
                              true, false, false, true, 100m, 150m, null, 0, null,
                              null, null, null, null, null);
        var mockItems68 = MockDbSetHelper.CreateMockDbSet<Item>(new[] { item });
        _db.Items.Returns(mockItems68);
        var mockStock69 = MockDbSetHelper.CreateMockDbSet<StockItem>();
        _db.Stock.Returns(mockStock69);
        var mockDepositos70 = MockDbSetHelper.CreateMockDbSet<Deposito>();
        _db.Depositos.Returns(mockDepositos70);

        var result = await Sut().Handle(
            new GetStockByItemQuery(0L),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.StockTotal.Should().Be(0);
    }
}
