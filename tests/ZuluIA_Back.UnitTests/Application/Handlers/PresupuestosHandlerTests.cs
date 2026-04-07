using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CreatePresupuestoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly CreatePresupuestoCommandHandler _handler;

    public CreatePresupuestoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new CreatePresupuestoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_SinItems_CreaPresupuestoYRetornaSuccess()
    {
        var presupuestos = MockDbSetHelper.CreateMockDbSet<Presupuesto>();
        _db.Presupuestos.Returns(presupuestos);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = PresupuestoTestHelper.CrearComando(items: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        presupuestos.Should().ContainSingle(x =>
            x.SucursalId == 1 &&
            x.TerceroId == 2 &&
            x.MonedaId == 1 &&
            x.Estado == "PENDIENTE");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConItems_CreaLineasYActualizaTotal()
    {
        var presupuestos = MockDbSetHelper.CreateMockDbSet<Presupuesto>();
        var items = MockDbSetHelper.CreateMockDbSet<PresupuestoItem>();
        _db.Presupuestos.Returns(presupuestos);
        _db.PresupuestosItems.Returns(items);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = PresupuestoTestHelper.CrearComando(new[]
        {
            new CreatePresupuestoItemInput(10, "Prod A", 2m, 100m, 0m),
            new CreatePresupuestoItemInput(20, "Prod B", 1m, 50m, 10m)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        items.Should().HaveCount(2);
        await _db.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AprobarPresupuestoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly AprobarPresupuestoCommandHandler _handler;

    public AprobarPresupuestoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new AprobarPresupuestoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_PresupuestoNoEncontrado_RetornaFailure()
    {
        var presupuestos = MockDbSetHelper.CreateMockDbSet<Presupuesto>();
        _db.Presupuestos.Returns(presupuestos);

        var result = await _handler.Handle(new AprobarPresupuestoCommand(99, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_PresupuestoExiste_ApruebaYRetornaSuccess()
    {
        var presupuesto = PresupuestoTestHelper.CrearPresupuesto();
        var presupuestos = MockDbSetHelper.CreateMockDbSet(new[] { presupuesto });
        _db.Presupuestos.Returns(presupuestos);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new AprobarPresupuestoCommand(presupuesto.Id, 3), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        presupuesto.Estado.Should().Be("APROBADO");
    }
}

public class RechazarPresupuestoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly RechazarPresupuestoCommandHandler _handler;

    public RechazarPresupuestoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new RechazarPresupuestoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_PresupuestoNoEncontrado_RetornaFailure()
    {
        var presupuestos = MockDbSetHelper.CreateMockDbSet<Presupuesto>();
        _db.Presupuestos.Returns(presupuestos);

        var result = await _handler.Handle(new RechazarPresupuestoCommand(99, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_PresupuestoExiste_RechazaYRetornaSuccess()
    {
        var presupuesto = PresupuestoTestHelper.CrearPresupuesto();
        var presupuestos = MockDbSetHelper.CreateMockDbSet(new[] { presupuesto });
        _db.Presupuestos.Returns(presupuestos);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new RechazarPresupuestoCommand(presupuesto.Id, 3), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        presupuesto.Estado.Should().Be("RECHAZADO");
    }
}

public class DeletePresupuestoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly DeletePresupuestoCommandHandler _handler;

    public DeletePresupuestoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new DeletePresupuestoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_PresupuestoNoEncontrado_RetornaFailure()
    {
        var presupuestos = MockDbSetHelper.CreateMockDbSet<Presupuesto>();
        _db.Presupuestos.Returns(presupuestos);

        var result = await _handler.Handle(new DeletePresupuestoCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_PresupuestoExiste_EliminaYRetornaSuccess()
    {
        var presupuesto = PresupuestoTestHelper.CrearPresupuesto();
        var presupuestos = MockDbSetHelper.CreateMockDbSet(new[] { presupuesto });
        _db.Presupuestos.Returns(presupuestos);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new DeletePresupuestoCommand(presupuesto.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        presupuesto.DeletedAt.Should().NotBeNull();
    }
}

public class PresupuestoCommandValidatorTests
{
    [Fact]
    public void CreatePresupuestoValidator_CommandInvalido_ReportaErrores()
    {
        var validator = new CreatePresupuestoCommandValidator();

        var result = validator.Validate(new CreatePresupuestoCommand(0, 0, new DateOnly(2026, 3, 20), null, 0, 0, null, null, [
            new CreatePresupuestoItemInput(0, string.Empty, 0, -1, 101)
        ]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePresupuestoCommand.SucursalId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePresupuestoCommand.TerceroId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePresupuestoCommand.MonedaId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePresupuestoCommand.Cotizacion));
    }

    [Fact]
    public void AprobarRechazarDeleteValidators_IdInvalido_ReportanError()
    {
        new AprobarPresupuestoCommandValidator().Validate(new AprobarPresupuestoCommand(0, null)).IsValid.Should().BeFalse();
        new RechazarPresupuestoCommandValidator().Validate(new RechazarPresupuestoCommand(0, null)).IsValid.Should().BeFalse();
        new DeletePresupuestoCommandValidator().Validate(new DeletePresupuestoCommand(0)).IsValid.Should().BeFalse();
    }
}

internal static class PresupuestoTestHelper
{
    public static CreatePresupuestoCommand CrearComando(IReadOnlyCollection<CreatePresupuestoItemInput>? items)
        => new(
            1,
            2,
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 31),
            1,
            1m,
            "Observacion",
            7,
            items);

    public static Presupuesto CrearPresupuesto()
        => Presupuesto.Crear(
            1,
            2,
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 31),
            1,
            1m,
            "Observacion",
            7);
}