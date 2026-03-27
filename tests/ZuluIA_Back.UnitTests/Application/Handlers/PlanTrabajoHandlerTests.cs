using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.PlanTrabajo.Commands;
using ZuluIA_Back.Domain.Entities.Franquicias;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CrearPlanTrabajoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly CrearPlanTrabajoCommandHandler _handler;

    public CrearPlanTrabajoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new CrearPlanTrabajoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_ComandoValido_CreaPlanYRetornaId()
    {
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CrearPlanTrabajoCommand(
            "Plan marzo",
            1,
            202603,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            "Descripcion",
            7);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        _db.PlanesTrabajo.Received(1).Add(Arg.Is<PlanTrabajo>(p =>
            p.Nombre == "Plan marzo" &&
            p.SucursalId == 1 &&
            p.Periodo == 202603));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AgregarKpiCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly AgregarKpiCommandHandler _handler;

    public AgregarKpiCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new AgregarKpiCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_PlanNoExiste_RetornaFailure()
    {
        var planes = MockDbSetHelper.CreateMockDbSet<PlanTrabajo>();
        _db.PlanesTrabajo.Returns(planes);

        var result = await _handler.Handle(new AgregarKpiCommand(99, null, null, "Ventas", 100m, 30m, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task Handle_PlanActivo_AgregaKpiYRetornaSuccess()
    {
        var plan = PlanTrabajoTestHelper.CrearPlan();
        var planes = MockDbSetHelper.CreateMockDbSet(new[] { plan });
        _db.PlanesTrabajo.Returns(planes);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new AgregarKpiCommand(plan.Id, 1, 2, "Ventas", 100m, 30m, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        plan.Kpis.Should().ContainSingle(k => k.Descripcion == "Ventas");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PlanCerrado_RetornaFailure()
    {
        var plan = PlanTrabajoTestHelper.CrearPlan();
        plan.Cerrar(1);
        var planes = MockDbSetHelper.CreateMockDbSet(new[] { plan });
        _db.PlanesTrabajo.Returns(planes);

        var result = await _handler.Handle(new AgregarKpiCommand(plan.Id, null, null, "Ventas", 100m, 30m, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("planes activos");
    }
}

public class CerrarPlanTrabajoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly CerrarPlanTrabajoCommandHandler _handler;

    public CerrarPlanTrabajoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new CerrarPlanTrabajoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_PlanNoExiste_RetornaFailure()
    {
        var planes = MockDbSetHelper.CreateMockDbSet<PlanTrabajo>();
        planes.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<PlanTrabajo?>((PlanTrabajo?)null));
        _db.PlanesTrabajo.Returns(planes);

        var result = await _handler.Handle(new CerrarPlanTrabajoCommand(55, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task Handle_PlanActivo_CierraYRetornaSuccess()
    {
        var plan = PlanTrabajoTestHelper.CrearPlan();
        var planes = MockDbSetHelper.CreateMockDbSet(new[] { plan });
        planes.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<PlanTrabajo?>(plan));
        _db.PlanesTrabajo.Returns(planes);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new CerrarPlanTrabajoCommand(plan.Id, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        plan.Estado.Should().Be(EstadoPlanTrabajo.Cerrado);
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PlanYaCerrado_RetornaFailure()
    {
        var plan = PlanTrabajoTestHelper.CrearPlan();
        plan.Cerrar(1);
        var planes = MockDbSetHelper.CreateMockDbSet(new[] { plan });
        planes.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<PlanTrabajo?>(plan));
        _db.PlanesTrabajo.Returns(planes);

        var result = await _handler.Handle(new CerrarPlanTrabajoCommand(plan.Id, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("planes activos");
    }
}

public class AnularPlanTrabajoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly AnularPlanTrabajoCommandHandler _handler;

    public AnularPlanTrabajoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new AnularPlanTrabajoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_PlanNoExiste_RetornaFailure()
    {
        var planes = MockDbSetHelper.CreateMockDbSet<PlanTrabajo>();
        planes.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<PlanTrabajo?>((PlanTrabajo?)null));
        _db.PlanesTrabajo.Returns(planes);

        var result = await _handler.Handle(new AnularPlanTrabajoCommand(55, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task Handle_PlanActivo_AnulaYRetornaSuccess()
    {
        var plan = PlanTrabajoTestHelper.CrearPlan();
        var planes = MockDbSetHelper.CreateMockDbSet(new[] { plan });
        planes.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<PlanTrabajo?>(plan));
        _db.PlanesTrabajo.Returns(planes);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new AnularPlanTrabajoCommand(plan.Id, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        plan.Estado.Should().Be(EstadoPlanTrabajo.Anulado);
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PlanYaAnulado_RetornaFailure()
    {
        var plan = PlanTrabajoTestHelper.CrearPlan();
        plan.Anular(1);
        var planes = MockDbSetHelper.CreateMockDbSet(new[] { plan });
        planes.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<PlanTrabajo?>(plan));
        _db.PlanesTrabajo.Returns(planes);

        var result = await _handler.Handle(new AnularPlanTrabajoCommand(plan.Id, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ya está anulado");
    }
}

public class PlanTrabajoCommandValidatorTests
{
    [Fact]
    public void AgregarKpiValidator_CommandInvalido_ReportaErrores()
    {
        var validator = new AgregarKpiCommandValidator();

        var result = validator.Validate(new AgregarKpiCommand(0, null, null, string.Empty, 0, 0, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarKpiCommand.PlanTrabajoId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarKpiCommand.Descripcion));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarKpiCommand.ValorObjetivo));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarKpiCommand.Peso));
    }

    [Fact]
    public void AgregarKpiValidator_CommandValido_NoReportaErrores()
    {
        var validator = new AgregarKpiCommandValidator();

        var result = validator.Validate(new AgregarKpiCommand(1, 1, 2, "Ventas", 100m, 25m, 9));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CerrarPlanTrabajoValidator_IdInvalido_ReportaError()
    {
        var validator = new CerrarPlanTrabajoCommandValidator();

        var result = validator.Validate(new CerrarPlanTrabajoCommand(0, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CerrarPlanTrabajoCommand.Id));
    }

    [Fact]
    public void AnularPlanTrabajoValidator_IdInvalido_ReportaError()
    {
        var validator = new AnularPlanTrabajoCommandValidator();

        var result = validator.Validate(new AnularPlanTrabajoCommand(0, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(AnularPlanTrabajoCommand.Id));
    }
}

internal static class PlanTrabajoTestHelper
{
    public static PlanTrabajo CrearPlan()
        => PlanTrabajo.Crear(
            "Plan marzo",
            1,
            202603,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            "Descripcion",
            1);
}