using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Pagos.Commands;
using ZuluIA_Back.Application.Features.Pagos.DTOs;
using ZuluIA_Back.Application.Features.PlanesPago.Commands;
using ZuluIA_Back.Application.Features.PlanesPago.DTOs;
using ZuluIA_Back.Application.Features.PlanesPago.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── AnularCobroCommandHandler ─────────────────────────────────────────────────

public class AnularCobroCommandHandlerTests
{
    private readonly ICobroRepository _repo = Substitute.For<ICobroRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private AnularCobroCommandHandler Sut() => new(_repo, _db, _uow, _user);

    [Fact]
    public async Task Handle_CobroNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Cobro?)null);

        var result = await Sut().Handle(new AnularCobroCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CobroExiste_AnulaYPersiste()
    {
        var cobro = Cobro.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, 1m, null, null);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(cobro);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new AnularCobroCommand(cobro.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Cobro>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CreatePagoCommandHandler ──────────────────────────────────────────────────

public class CreatePagoCommandHandlerTests
{
    private readonly IRepository<Pago> _repo = Substitute.For<IRepository<Pago>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreatePagoCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_SinMediosDePago_RetornaFailure()
    {
        var cmd = new CreatePagoCommand(
            1, 1, DateOnly.FromDateTime(DateTime.Today), 1, 1m, null,
            new List<CreatePagoMedioDto>());

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConMedios_CreaPagoYRetornaId()
    {
        _user.UserId.Returns((long?)1L);

        var medios = new List<CreatePagoMedioDto>
        {
            new(1, 1, 1000m, 1, 1m, null)
        };
        var cmd = new CreatePagoCommand(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, 1m, null, medios);

        var result = await Sut().Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Pago>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── AnularPagoCommandHandler ──────────────────────────────────────────────────

public class AnularPagoCommandHandlerTests
{
    private readonly IRepository<Pago> _repo = Substitute.For<IRepository<Pago>>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private AnularPagoCommandHandler Sut() => new(_repo, _db, _uow, _user);

    [Fact]
    public async Task Handle_PagoNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns((Pago?)null);

        var result = await Sut().Handle(new AnularPagoCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PagoExiste_AnulaYPersiste()
    {
        var pago = Pago.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, 1m, null, null);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(pago);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new AnularPagoCommand(pago.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Pago>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── CreatePlanPagoCommandHandler ──────────────────────────────────────────────

public class CreatePlanPagoCommandHandlerTests
{
    private readonly IPlanPagoRepository _repo = Substitute.For<IPlanPagoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private CreatePlanPagoCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_DatosValidos_CreaYRetornaId()
    {
        var result = await Sut().Handle(
            new CreatePlanPagoCommand("Contado", 1, 0m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<PlanPago>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpdatePlanPagoCommandHandler ──────────────────────────────────────────────

public class UpdatePlanPagoCommandHandlerTests
{
    private readonly IPlanPagoRepository _repo = Substitute.For<IPlanPagoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private UpdatePlanPagoCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_PlanNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns((PlanPago?)null);

        var result = await Sut().Handle(
            new UpdatePlanPagoCommand(99, "Nuevo", 3, 5m),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PlanExiste_ActualizaYPersiste()
    {
        var plan = PlanPago.Crear("Contado", 1, 0m);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(plan);

        var result = await Sut().Handle(
            new UpdatePlanPagoCommand(1, "3 cuotas", 3, 5m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<PlanPago>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── DeletePlanPagoCommandHandler ──────────────────────────────────────────────

public class DeletePlanPagoCommandHandlerTests
{
    private readonly IPlanPagoRepository _repo = Substitute.For<IPlanPagoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private DeletePlanPagoCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_PlanNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns((PlanPago?)null);

        var result = await Sut().Handle(new DeletePlanPagoCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PlanExiste_DesactivaYPersiste()
    {
        var plan = PlanPago.Crear("Contado", 1, 0m);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(plan);

        var result = await Sut().Handle(new DeletePlanPagoCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        plan.Activo.Should().BeFalse();
        _repo.Received(1).Update(Arg.Any<PlanPago>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetPlanesPagoQueryHandler ─────────────────────────────────────────────────

public class GetPlanesPagoQueryHandlerTests
{
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IPlanPagoRepository _repo = Substitute.For<IPlanPagoRepository>();
    private GetPlanesPagoQueryHandler Sut() => new(_mapper, _repo);

    [Fact]
    public async Task Handle_SoloActivos_LlamaGetActivosAsync()
    {
        var planes = new List<PlanPago>().AsReadOnly();
        var dtos = new List<PlanPagoDto>().AsReadOnly();
        _repo.GetActivosAsync(Arg.Any<CancellationToken>())
             .Returns(planes);
        _mapper.Map<IReadOnlyList<PlanPagoDto>>(planes).Returns(dtos);

        var result = await Sut().Handle(new GetPlanesPagoQuery(true), CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.Received(1).GetActivosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TodosLosPlanes_LlamaGetAllAsync()
    {
        var planes = new List<PlanPago>().AsReadOnly();
        var dtos = new List<PlanPagoDto>().AsReadOnly();
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns(planes);
        _mapper.Map<IReadOnlyList<PlanPagoDto>>(planes).Returns(dtos);

        var result = await Sut().Handle(new GetPlanesPagoQuery(false), CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }
}
