using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cotizaciones.Commands;
using ZuluIA_Back.Application.Features.Cotizaciones.DTOs;
using ZuluIA_Back.Application.Features.Cotizaciones.Queries;
using ZuluIA_Back.Application.Features.Retenciones.Commands;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;
using ZuluIA_Back.Application.Features.Retenciones.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateTipoRetencionCommandHandler ─────────────────────────────────────────

public class CreateTipoRetencionCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private CreateTipoRetencionCommandHandler Sut() => new(_db, _uow);

    [Fact]
    public async Task Handle_DatosValidos_CreaYRetornaId()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>();
        _db.TiposRetencion.Returns(mockDbSet);

        var cmd = new CreateTipoRetencionCommand(
            "Ganancias", "GANANCIAS", 0m, false, null, null,
            new List<EscalaRetencionInputDto>().AsReadOnly());

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpdateTipoRetencionCommandHandler ─────────────────────────────────────────

public class UpdateTipoRetencionCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private UpdateTipoRetencionCommandHandler Sut() => new(_db, _uow);

    [Fact]
    public async Task Handle_TipoNoExiste_RetornaFailure()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>();
        _db.TiposRetencion.Returns(mockDbSet);

        var escalasDbSet = MockDbSetHelper.CreateMockDbSet<EscalaRetencion>();
        _db.EscalasRetencion.Returns(escalasDbSet);

        var cmd = new UpdateTipoRetencionCommand(
            99, "Test", "GANANCIAS", 0m, false, null, null,
            new List<EscalaRetencionInputDto>().AsReadOnly());

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TipoExiste_ActualizaYRetornaSuccess()
    {
        var tipo = TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>(new[] { tipo });
        _db.TiposRetencion.Returns(mockDbSet);

        var escalasDbSet = MockDbSetHelper.CreateMockDbSet<EscalaRetencion>();
        _db.EscalasRetencion.Returns(escalasDbSet);

        var cmd = new UpdateTipoRetencionCommand(
            tipo.Id, "Ganancias 2", "GANANCIAS", 0m, false, null, null,
            new List<EscalaRetencionInputDto>().AsReadOnly());

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeleteTipoRetencionCommandHandler ─────────────────────────────────────────

public class DeleteTipoRetencionCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private DeleteTipoRetencionCommandHandler Sut() => new(_db, _uow);

    [Fact]
    public async Task Handle_TipoNoExiste_RetornaFailure()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>();
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new DeleteTipoRetencionCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TipoExiste_DesactivaYPersiste()
    {
        var tipo = TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>(new[] { tipo });
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new DeleteTipoRetencionCommand(tipo.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetTiposRetencionQueryHandler ─────────────────────────────────────────────

public class GetTiposRetencionQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetTiposRetencionQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_SinRetenciones_RetornaListaVacia()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>();
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new GetTiposRetencionQuery(false), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ConRetenciones_RetornaListaConDatos()
    {
        var tipo = TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>(new[] { tipo });
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new GetTiposRetencionQuery(false), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Descripcion.Should().Be("Ganancias");
    }
}

// ── GetTipoRetencionByIdQueryHandler ──────────────────────────────────────────

public class GetTipoRetencionByIdQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private GetTipoRetencionByIdQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_TipoNoExiste_RetornaNull()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>();
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new GetTipoRetencionByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_TipoExiste_RetornaDto()
    {
        var tipo = TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>(new[] { tipo });
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new GetTipoRetencionByIdQuery(tipo.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Descripcion.Should().Be("Ganancias");
    }
}

// ── CalcularRetencionQueryHandler ─────────────────────────────────────────────

public class CalcularRetencionQueryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private CalcularRetencionQueryHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_TipoNoExiste_RetornaFailure()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>();
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new CalcularRetencionQuery(99, 1000m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TipoActivo_RetornaImporteCalculado()
    {
        var tipo = TipoRetencion.Crear("Ganancias", "GANANCIAS", 0m, false, null, null, null);
        tipo.AgregarEscala("Tramo 1", 0m, 99999m, 5m);

        var mockDbSet = MockDbSetHelper.CreateMockDbSet<TipoRetencion>(new[] { tipo });
        _db.TiposRetencion.Returns(mockDbSet);

        var result = await Sut().Handle(new CalcularRetencionQuery(tipo.Id, 1000m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0m);
    }
}

// ── RegistrarCotizacionCommandHandler ────────────────────────────────────────

public class RegistrarCotizacionCommandHandlerTests
{
    private readonly ICotizacionMonedaRepository _repo = Substitute.For<ICotizacionMonedaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private RegistrarCotizacionCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_CotizacionNueva_CreaYRetornaId()
    {
        _repo.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<CotizacionMoneda, bool>>>(), Arg.Any<CancellationToken>())
             .Returns((CotizacionMoneda?)null);

        var cmd = new RegistrarCotizacionCommand(1, DateOnly.FromDateTime(DateTime.Today), 950m);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<CotizacionMoneda>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CotizacionExistente_ActualizaYRetornaId()
    {
        var existente = CotizacionMoneda.Crear(1, DateOnly.FromDateTime(DateTime.Today), 900m);
        _repo.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<CotizacionMoneda, bool>>>(), Arg.Any<CancellationToken>())
             .Returns(existente);

        var cmd = new RegistrarCotizacionCommand(1, DateOnly.FromDateTime(DateTime.Today), 950m);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<CotizacionMoneda>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class ImportarCotizacionesCommandHandlerTests
{
    private readonly ICotizacionMonedaRepository _repo = Substitute.For<ICotizacionMonedaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private ImportarCotizacionesCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_ItemsNuevosYExistentes_CreaActualizaYRetornaResumen()
    {
        var existente = CotizacionMoneda.Crear(1, new DateOnly(2026, 3, 20), 900m);
        _repo.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<CotizacionMoneda, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existente, (CotizacionMoneda?)null);

        var cmd = new ImportarCotizacionesCommand(1,
        [
            new ImportarCotizacionItemInput(new DateOnly(2026, 3, 20), 950m),
            new ImportarCotizacionItemInput(new DateOnly(2026, 3, 21), 975m)
        ]);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Procesadas.Should().Be(2);
        result.Value.Creadas.Should().Be(1);
        result.Value.Actualizadas.Should().Be(1);
        _repo.Received(1).Update(Arg.Any<CotizacionMoneda>());
        await _repo.Received(1).AddAsync(Arg.Any<CotizacionMoneda>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoHayCotizacionInvalida_RetornaFailure()
    {
        _repo.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<CotizacionMoneda, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CotizacionMoneda?)null);

        var cmd = new ImportarCotizacionesCommand(1,
        [
            new ImportarCotizacionItemInput(new DateOnly(2026, 3, 20), 0m)
        ]);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetCotizacionVigenteQueryHandler ─────────────────────────────────────────

public class GetCotizacionVigenteQueryHandlerTests
{
    private readonly ICotizacionMonedaRepository _repo = Substitute.For<ICotizacionMonedaRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetCotizacionVigenteQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_SinCotizacion_RetornaNull()
    {
        _repo.GetVigenteAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns((CotizacionMoneda?)null);

        var result = await Sut().Handle(new GetCotizacionVigenteQuery(1, DateOnly.MinValue), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ConCotizacion_RetornaMappedDto()
    {
        var cotizacion = CotizacionMoneda.Crear(1, DateOnly.FromDateTime(DateTime.Today), 950m);
        var dto = new CotizacionMonedaDto { MonedaId = 1, Cotizacion = 950m };
        _repo.GetVigenteAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(cotizacion);
        _mapper.Map<CotizacionMonedaDto>(cotizacion).Returns(dto);

        var result = await Sut().Handle(new GetCotizacionVigenteQuery(1, DateOnly.MinValue), CancellationToken.None);

        result.Should().BeSameAs(dto);
    }
}

// ── GetHistoricoCotizacionesQueryHandler ──────────────────────────────────────

public class GetHistoricoCotizacionesQueryHandlerTests
{
    private readonly ICotizacionMonedaRepository _repo = Substitute.For<ICotizacionMonedaRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetHistoricoCotizacionesQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaHistoricoMapeado()
    {
        var historico = new List<CotizacionMoneda>().AsReadOnly();
        var dtos = new List<CotizacionMonedaDto>().AsReadOnly();
        _repo.GetHistoricoAsync(Arg.Any<long>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(historico);
        _mapper.Map<IReadOnlyList<CotizacionMonedaDto>>(historico).Returns(dtos);

        var result = await Sut().Handle(new GetHistoricoCotizacionesQuery(1, null, null), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
