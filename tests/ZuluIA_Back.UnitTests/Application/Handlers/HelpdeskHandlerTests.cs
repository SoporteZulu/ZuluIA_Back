using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Documentos.Commands;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CreateHelpdeskTicketCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly CreateHelpdeskTicketCommandHandler _handler;

    public CreateHelpdeskTicketCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new CreateHelpdeskTicketCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_ComandoValido_CreaTicketYRetornaSuccess()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateHelpdeskTicketCommand(
            1,
            2,
            3,
            "HD-001",
            "Error en impresion",
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 22),
            "Observacion");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _db.MesasEntrada.Received(1).Add(Arg.Is<MesaEntrada>(x =>
            x.SucursalId == 1 &&
            x.TipoId == 2 &&
            x.TerceroId == 3 &&
            x.NroDocumento == "HD-001" &&
            x.Asunto == "Error en impresion" &&
            x.EstadoFlow == EstadoMesaEntrada.Pendiente));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AssignHelpdeskTicketCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly AssignHelpdeskTicketCommandHandler _handler;

    public AssignHelpdeskTicketCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new AssignHelpdeskTicketCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_TicketNoExiste_RetornaFailure()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new AssignHelpdeskTicketCommand(10, 5), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("10");
    }

    [Fact]
    public async Task Handle_TicketExiste_AsignaYRetornaSuccess()
    {
        var ticket = HelpdeskTestHelper.CrearTicket();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { ticket });
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new AssignHelpdeskTicketCommand(ticket.Id, 5), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AsignadoA.Should().Be(5);
        ticket.AsignadoA.Should().Be(5);
    }
}

public class ChangeHelpdeskTicketStateCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ChangeHelpdeskTicketStateCommandHandler _handler;

    public ChangeHelpdeskTicketStateCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new ChangeHelpdeskTicketStateCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_TicketNoExiste_RetornaFailure()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new ChangeHelpdeskTicketStateCommand(10, 2, "EnProceso", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("10");
    }

    [Fact]
    public async Task Handle_EstadoFlowInvalido_RetornaFailure()
    {
        var ticket = HelpdeskTestHelper.CrearTicket();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { ticket });
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new ChangeHelpdeskTicketStateCommand(ticket.Id, 2, "XXX", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("inválido");
    }

    [Fact]
    public async Task Handle_DatosValidos_CambiaEstadoYRetornaSuccess()
    {
        var ticket = HelpdeskTestHelper.CrearTicket();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { ticket });
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new ChangeHelpdeskTicketStateCommand(ticket.Id, 2, "EnProceso", "tomado"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EstadoFlow.Should().Be(nameof(EstadoMesaEntrada.EnProceso));
        ticket.EstadoFlow.Should().Be(EstadoMesaEntrada.EnProceso);
        ticket.EstadoId.Should().Be(2);
    }
}

public class CloseHelpdeskTicketCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly CloseHelpdeskTicketCommandHandler _handler;

    public CloseHelpdeskTicketCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new CloseHelpdeskTicketCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_TicketNoExiste_RetornaFailure()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new CloseHelpdeskTicketCommand(10), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("10");
    }

    [Fact]
    public async Task Handle_TicketExiste_CierraYRetornaSuccess()
    {
        var ticket = HelpdeskTestHelper.CrearTicket();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { ticket });
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new CloseHelpdeskTicketCommand(ticket.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ticket.EstadoFlow.Should().Be(EstadoMesaEntrada.Resuelto);
        ticket.Observacion.Should().Be("Ticket cerrado");
    }
}

public class HelpdeskTicketCommandValidatorTests
{
    [Fact]
    public void CreateHelpdeskValidator_CommandInvalido_ReportaErrores()
    {
        var validator = new CreateHelpdeskTicketCommandValidator();

        var result = validator.Validate(new CreateHelpdeskTicketCommand(0, 0, null, string.Empty, string.Empty, new DateOnly(2026, 3, 20), null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHelpdeskTicketCommand.SucursalId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHelpdeskTicketCommand.TipoId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHelpdeskTicketCommand.NroDocumento));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHelpdeskTicketCommand.Titulo));
    }

    [Fact]
    public void AssignHelpdeskValidator_CommandInvalido_ReportaErrores()
    {
        var validator = new AssignHelpdeskTicketCommandValidator();

        var result = validator.Validate(new AssignHelpdeskTicketCommand(0, 0));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ChangeStateAndCloseValidators_CommandInvalido_ReportanErrores()
    {
        var stateValidator = new ChangeHelpdeskTicketStateCommandValidator();
        var closeValidator = new CloseHelpdeskTicketCommandValidator();

        stateValidator.Validate(new ChangeHelpdeskTicketStateCommand(0, -1, string.Empty, null)).IsValid.Should().BeFalse();
        closeValidator.Validate(new CloseHelpdeskTicketCommand(0)).IsValid.Should().BeFalse();
    }
}

internal static class HelpdeskTestHelper
{
    public static MesaEntrada CrearTicket()
        => MesaEntrada.Crear(
            1,
            2,
            3,
            "HD-001",
            "Error en impresion",
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 22),
            "Observacion",
            1);
}