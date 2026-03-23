using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;
using AutoMapper;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateAsientoCommandHandler ───────────────────────────────────────────────

public class CreateAsientoCommandHandlerTests
{
    private readonly IAsientoRepository _repo = Substitute.For<IAsientoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateAsientoCommandHandler Sut() => new(_repo, _uow, _user);

    private static CreateAsientoCommand CmdBalanceado() => new(
        1, 1,
        DateOnly.FromDateTime(DateTime.Today),
        "Venta",
        null, null,
        [
            new CreateAsientoLineaDto(1, 1000m, 0m,   "Clientes", 1),
            new CreateAsientoLineaDto(2, 0m,    826m, "Ventas",   2),
            new CreateAsientoLineaDto(3, 0m,    174m, "IVA",      3)
        ]);

    [Fact]
    public async Task Handle_AsientoBalanceado_CreaYRetornaId()
    {
        _repo.GetProximoNumeroAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns(1L);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(CmdBalanceado(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Asiento>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AsientoNoBalanceado_RetornaFailure()
    {
        _repo.GetProximoNumeroAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns(1L);
        _user.UserId.Returns((long?)1L);

        var cmdNoBalanceado = new CreateAsientoCommand(
            1, 1,
            DateOnly.FromDateTime(DateTime.Today),
            "Venta desbalanceada",
            null, null,
            [
                new CreateAsientoLineaDto(1, 1000m, 0m,  "Debe", 1),
                new CreateAsientoLineaDto(2, 0m,    500m, "Haber", 2)
            ]);

        var result = await Sut().Handle(cmdNoBalanceado, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

// ── AnularAsientoCommandHandler ───────────────────────────────────────────────

public class AnularAsientoCommandHandlerTests
{
    private readonly IAsientoRepository _repo = Substitute.For<IAsientoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private AnularAsientoCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_AsientoNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns((Asiento?)null);

        var result = await Sut().Handle(new AnularAsientoCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AsientoExiste_AnulaYPersiste()
    {
        var asiento = Asiento.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, "Desc", null, null, null);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(asiento);

        var result = await Sut().Handle(new AnularAsientoCommand(asiento.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Asiento>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CreateEjercicioCommandHandler ─────────────────────────────────────────────

public class CreateEjercicioCommandHandlerTests
{
    private readonly IEjercicioRepository _repo = Substitute.For<IEjercicioRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private CreateEjercicioCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_FechasValidas_CreaYRetornaId()
    {
        var inicio = new DateOnly(2024, 1, 1);
        var fin = new DateOnly(2024, 12, 31);

        var result = await Sut().Handle(
            new CreateEjercicioCommand("2024", inicio, fin, "00.00.00.00", [1L]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Ejercicio>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CreatePlanCuentaCommandHandler ────────────────────────────────────────────

public class CreatePlanCuentaCommandHandlerTests
{
    private readonly IPlanCuentasRepository _repo = Substitute.For<IPlanCuentasRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private CreatePlanCuentaCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_CodigoExistente_RetornaFailure()
    {
        _repo.ExisteCodigoAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(
            new CreatePlanCuentaCommand(1, null, "1.1.1.1", "Caja", 1, "01", true, "Activo", 'D'),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CodigoNuevo_CreaYRetornaId()
    {
        _repo.ExisteCodigoAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);

        var result = await Sut().Handle(
            new CreatePlanCuentaCommand(1, null, "1.1.1.1", "Caja", 1, "01", true, "Activo", 'D'),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<PlanCuenta>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── RegistrarAsientoCommandHandler ───────────────────────────────────────────

public class RegistrarAsientoCommandHandlerTests
{
    private readonly ContabilidadService _service = Substitute.For<ContabilidadService>(
        Substitute.For<IAsientoRepository>(),
        Substitute.For<IEjercicioRepository>());
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private RegistrarAsientoCommandHandler Sut() => new(_service, _uow, _user);

    [Fact]
    public async Task Handle_ServicioRetornaAsiento_RetornaIdExitoso()
    {
        _user.UserId.Returns((long?)1L);
        var asiento = Asiento.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, "Desc", null, null, null);
        _service.RegistrarAsientoAsync(
            Arg.Any<long>(), Arg.Any<long>(), Arg.Any<DateOnly>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<long?>(),
            Arg.Any<IReadOnlyList<(long, decimal, decimal, string?, long?)>>(),
            Arg.Any<long?>(), Arg.Any<CancellationToken>())
            .Returns(asiento);

        var cmd = new RegistrarAsientoCommand(
            1, 1, DateOnly.FromDateTime(DateTime.Today), "Desc", null, null,
            [new LineaAsientoInput(1, 1000m, 0m, null, null),
             new LineaAsientoInput(2, 0m, 1000m, null, null)]);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetAsientoByIdQueryHandler ────────────────────────────────────────────────

public class GetAsientoByIdQueryHandlerTests
{
    private readonly IAsientoRepository _repo = Substitute.For<IAsientoRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAsientoByIdQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_AsientoNoExiste_RetornaNull()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns((Asiento?)null);

        var result = await Sut().Handle(new GetAsientoByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AsientoExiste_RetornaDto()
    {
        var asiento = Asiento.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, "Desc", null, null, null);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(asiento);
        _mapper.Map<AsientoDto>(Arg.Any<Asiento>()).Returns(new AsientoDto());

        var result = await Sut().Handle(new GetAsientoByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
    }
}

// ── GetAsientosPagedQueryHandler ──────────────────────────────────────────────

public class GetAsientosPagedQueryHandlerTests
{
    private readonly IAsientoRepository _repo = Substitute.For<IAsientoRepository>();
    private GetAsientosPagedQueryHandler Sut() => new(_repo);

    [Fact]
    public async Task Handle_RetornaPaginado()
    {
        var paged = new PagedResult<Asiento>([], 1, 20, 0);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<long?>(),
            Arg.Any<ZuluIA_Back.Domain.Enums.EstadoAsiento?>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            Arg.Any<CancellationToken>())
            .Returns(paged);

        var result = await Sut().Handle(
            new GetAsientosPagedQuery(1, 1, 20, null, null, null, null),
            CancellationToken.None);

        result.TotalCount.Should().Be(0);
    }
}

// ── GetEjerciciosQueryHandler ─────────────────────────────────────────────────

public class GetEjerciciosQueryHandlerTests
{
    private readonly IEjercicioRepository _repo = Substitute.For<IEjercicioRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetEjerciciosQueryHandler Sut() => new(_repo, _db);

    [Fact]
    public async Task Handle_SinEjercicios_RetornaListaVacia()
    {
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns((IReadOnlyList<Ejercicio>)[]);

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Sucursal>());
        _db.Sucursales.Returns(mockSet);

        var result = await Sut().Handle(new GetEjerciciosQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetPlanCuentasQueryHandler ────────────────────────────────────────────────

public class GetPlanCuentasQueryHandlerTests
{
    private readonly IPlanCuentasRepository _repo = Substitute.For<IPlanCuentasRepository>();
    private GetPlanCuentasQueryHandler Sut() => new(_repo);

    [Fact]
    public async Task Handle_RetornaPlanCuentas()
    {
        _repo.GetByEjercicioAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((IReadOnlyList<PlanCuenta>)[]);

        var result = await Sut().Handle(new GetPlanCuentasQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── GetBalanceSumasYSaldosQueryHandler ────────────────────────────────────────

public class GetBalanceSumasYSaldosQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetBalanceSumasYSaldosQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_SinAsientos_RetornaBalanceConLineasVacias()
    {
        var mockEjercicios29 = MockDbSetHelper.CreateMockDbSet<Ejercicio>();
        _db.Ejercicios.Returns(mockEjercicios29);
        var mockAsientos30 = MockDbSetHelper.CreateMockDbSet<Asiento>();
        _db.Asientos.Returns(mockAsientos30);
        var mockAsientosLineas31 = MockDbSetHelper.CreateMockDbSet<AsientoLinea>();
        _db.AsientosLineas.Returns(mockAsientosLineas31);
        var mockPlanCuentas32 = MockDbSetHelper.CreateMockDbSet<PlanCuenta>();
        _db.PlanCuentas.Returns(mockPlanCuentas32);

        var desde = new DateOnly(2024, 1, 1);
        var hasta = new DateOnly(2024, 12, 31);

        var result = await Sut().Handle(
            new GetBalanceSumasYSaldosQuery(1L, null, desde, hasta),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Lineas.Should().BeEmpty();
        result.TotalDebe.Should().Be(0);
        result.TotalHaber.Should().Be(0);
    }
}

// ── GetAsientoDetalleQueryHandler ─────────────────────────────────────────────

public class GetAsientoDetalleQueryHandlerTests
{
    private readonly IAsientoRepository _repo = Substitute.For<IAsientoRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetAsientoDetalleQueryHandler Sut() => new(_repo, _db);

    [Fact]
    public async Task Handle_AsientoNoExiste_RetornaNull()
    {
        _repo.GetByIdConLineasAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Asiento?)null);

        var result = await Sut().Handle(new GetAsientoDetalleQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AsientoExiste_RetornaDto()
    {
        var asiento = Asiento.Crear(1L, 1L, DateOnly.FromDateTime(DateTime.Today), 1L, "Test asiento", null, null, null);
        _repo.GetByIdConLineasAsync(1L, Arg.Any<CancellationToken>()).Returns(asiento);
        var mockEjercicios33 = MockDbSetHelper.CreateMockDbSet<Ejercicio>();
        _db.Ejercicios.Returns(mockEjercicios33);
        var mockPlanCuentas34 = MockDbSetHelper.CreateMockDbSet<PlanCuenta>();
        _db.PlanCuentas.Returns(mockPlanCuentas34);
        var mockCentrosCosto35 = MockDbSetHelper.CreateMockDbSet<CentroCosto>();
        _db.CentrosCosto.Returns(mockCentrosCosto35);

        var result = await Sut().Handle(new GetAsientoDetalleQuery(1L), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Descripcion.Should().Be("Test asiento");
    }
}
