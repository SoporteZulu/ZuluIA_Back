using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cajas.Commands;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Application.Features.Cajas.Queries;
using ZuluIA_Back.Application.Features.Cheques.Commands;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Application.Features.Cheques.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateCajaCommandHandler ──────────────────────────────────────────────────

public class CreateCajaCommandHandlerTests
{
    private readonly ICajaRepository _repo = Substitute.For<ICajaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateCajaCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_DatosValidos_CreaYRetornaId()
    {
        _user.UserId.Returns((long?)1L);

        var cmd = new CreateCajaCommand(1, 1, "Caja Principal", 1, true, null, null, null, null);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<CajaCuentaBancaria>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpdateCajaCommandHandler ──────────────────────────────────────────────────

public class UpdateCajaCommandHandlerTests
{
    private readonly ICajaRepository _repo = Substitute.For<ICajaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateCajaCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_CajaNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((CajaCuentaBancaria?)null);

        var result = await Sut().Handle(
            new UpdateCajaCommand(99, "Test", 1, 1, true, null, null, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CajaExiste_ActualizaYRetornaSuccess()
    {
        var caja = CajaCuentaBancaria.Crear(1, 1, "Caja", 1, true, null, null);
        _repo.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(caja);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(
            new UpdateCajaCommand(1, "Caja Actualizada", 1, 1, true, null, null, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<CajaCuentaBancaria>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeleteCajaCommandHandler ──────────────────────────────────────────────────

public class DeleteCajaCommandHandlerTests
{
    private readonly ICajaRepository _repo = Substitute.For<ICajaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private DeleteCajaCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_CajaNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((CajaCuentaBancaria?)null);

        var result = await Sut().Handle(new DeleteCajaCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CajaExiste_DesactivaYPersiste()
    {
        var caja = CajaCuentaBancaria.Crear(1, 1, "Caja", 1, true, null, null);
        _repo.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(caja);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new DeleteCajaCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<CajaCuentaBancaria>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── RegistrarTransferenciaCommandHandler ─────────────────────────────────────

public class RegistrarTransferenciaCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private RegistrarTransferenciaCommandHandler Sut() => new(_db, _uow);

    [Fact]
    public async Task Handle_CajaOrigenNoExiste_RetornaFailure()
    {
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<CajaCuentaBancaria>();
        _db.CajasCuentasBancarias.Returns(mockDbSet);

        var cmd = new RegistrarTransferenciaCommand(
            1, 99, 2, DateOnly.FromDateTime(DateTime.Today), 500m, 1, 1m, null);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CajasExisten_CreaTransferenciaYPersiste()
    {
        var cajaOrigen = CajaCuentaBancaria.Crear(1, 1, "Caja Origen", 1, true, null, null);
        var cajaDestino = CajaCuentaBancaria.Crear(1, 1, "Caja Destino", 1, true, null, null);
        typeof(ZuluIA_Back.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(cajaOrigen, 1L);
        typeof(ZuluIA_Back.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(cajaDestino, 2L);
        var mockDbSet = MockDbSetHelper.CreateMockDbSet<CajaCuentaBancaria>(new[] { cajaOrigen, cajaDestino });
        _db.CajasCuentasBancarias.Returns(mockDbSet);

        var transDbSet = MockDbSetHelper.CreateMockDbSet<TransferenciaCaja>();
        _db.TransferenciasCaja.Returns(transDbSet);

        var cmd = new RegistrarTransferenciaCommand(
            1, 1L, 2L,
            DateOnly.FromDateTime(DateTime.Today), 500m, 1, 1m, null);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetCajaByIdQueryHandler ───────────────────────────────────────────────────

public class GetCajaByIdQueryHandlerTests
{
    private readonly ICajaRepository _repo = Substitute.For<ICajaRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetCajaByIdQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_CajaNoExiste_RetornaNull()
    {
        _repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((CajaCuentaBancaria?)null);

        var result = await Sut().Handle(new GetCajaByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CajaExiste_RetornaMappedDto()
    {
        var caja = CajaCuentaBancaria.Crear(1, 1, "Caja", 1, true, null, null);
        var dto = new CajaDto { Id = 1, Descripcion = "Caja" };
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(caja);
        _mapper.Map<CajaDto>(caja).Returns(dto);

        var result = await Sut().Handle(new GetCajaByIdQuery(1), CancellationToken.None);

        result.Should().BeSameAs(dto);
    }
}

// ── GetCajasBySucursalQueryHandler ────────────────────────────────────────────

public class GetCajasBySucursalQueryHandlerTests
{
    private readonly ICajaRepository _repo = Substitute.For<ICajaRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetCajasBySucursalQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaListaMapeada()
    {
        var cajas = new List<CajaCuentaBancaria>().AsReadOnly();
        var dtos = new List<CajaListDto>().AsReadOnly();
        _repo.GetActivasBySucursalAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(cajas);
        _mapper.Map<IReadOnlyList<CajaListDto>>(cajas).Returns(dtos);

        var result = await Sut().Handle(new GetCajasBySucursalQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── CreateChequeCommandHandler ────────────────────────────────────────────────

public class CreateChequeCommandHandlerTests
{
    private readonly IChequeRepository _repo = Substitute.For<IChequeRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateChequeCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_DatosValidos_CreaYRetornaId()
    {
        _user.UserId.Returns((long?)1L);

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var cmd = new CreateChequeCommand(1, null, "00001", "BancoPrueba", hoy, hoy.AddDays(30), 1000m, 1, null);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Cheque>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CambiarEstadoChequeCommandHandler ─────────────────────────────────────────

public class CambiarEstadoChequeCommandHandlerTests
{
    private readonly IChequeRepository _repo = Substitute.For<IChequeRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CambiarEstadoChequeCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_ChequeNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Cheque?)null);

        var result = await Sut().Handle(
            new CambiarEstadoChequeCommand(99, AccionCheque.Depositar, DateOnly.FromDateTime(DateTime.Today), null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DepositarSinFecha_RetornaFailure()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var cheque = Cheque.Crear(1, null, "00001", "BancoPrueba", hoy, hoy.AddDays(30), 1000m, 1, null, null);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(cheque);

        var result = await Sut().Handle(
            new CambiarEstadoChequeCommand(cheque.Id, AccionCheque.Depositar, null, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DepositarConFecha_RetornaSuccess()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var cheque = Cheque.Crear(1, null, "00001", "BancoPrueba", hoy, hoy.AddDays(30), 1000m, 1, null, null);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(cheque);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(
            new CambiarEstadoChequeCommand(cheque.Id, AccionCheque.Depositar, hoy, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Cheque>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class CambiarEstadoChequeCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IdInvalido_RetornaError(long id)
    {
        var validator = new CambiarEstadoChequeCommandValidator();

        var result = validator.Validate(new CambiarEstadoChequeCommand(id, AccionCheque.Entregar, null, null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }

    [Fact]
    public void AccionInvalida_RetornaError()
    {
        var validator = new CambiarEstadoChequeCommandValidator();

        var result = validator.Validate(new CambiarEstadoChequeCommand(1, (AccionCheque)999, null, null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Accion");
    }

    [Theory]
    [InlineData(AccionCheque.Depositar)]
    [InlineData(AccionCheque.Acreditar)]
    public void AccionConFechaObligatoria_SinFecha_RetornaError(AccionCheque accion)
    {
        var validator = new CambiarEstadoChequeCommandValidator();

        var result = validator.Validate(new CambiarEstadoChequeCommand(1, accion, null, null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Fecha");
    }

    [Theory]
    [InlineData(AccionCheque.Depositar)]
    [InlineData(AccionCheque.Acreditar)]
    [InlineData(AccionCheque.Rechazar)]
    [InlineData(AccionCheque.Entregar)]
    public void DatosValidos_PasaValidacion(AccionCheque accion)
    {
        var validator = new CambiarEstadoChequeCommandValidator();
        DateOnly? fecha = accion is AccionCheque.Depositar or AccionCheque.Acreditar
            ? DateOnly.FromDateTime(DateTime.Today)
            : null;

        var result = validator.Validate(new CambiarEstadoChequeCommand(1, accion, fecha, null, "obs"));

        result.IsValid.Should().BeTrue();
    }
}

// ── GetChequesPagedQueryHandler ───────────────────────────────────────────────

public class GetChequesPagedQueryHandlerTests
{
    private readonly IChequeRepository _repo = Substitute.For<IChequeRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetChequesPagedQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaPagedResult()
    {
        var pagedResult = new PagedResult<Cheque>(new List<Cheque>(), 1, 10, 0);
        _repo.GetPagedAsync(
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<EstadoCheque?>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            Arg.Any<CancellationToken>())
            .Returns(pagedResult);
        _mapper.Map<IReadOnlyList<ChequeDto>>(Arg.Any<IReadOnlyList<Cheque>>())
               .Returns(new List<ChequeDto>().AsReadOnly());

        var result = await Sut().Handle(
            new GetChequesPagedQuery(1, 10, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
