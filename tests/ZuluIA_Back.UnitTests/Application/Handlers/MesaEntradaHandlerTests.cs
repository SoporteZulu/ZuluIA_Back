using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Documentos.Commands;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CreateMesaEntradaCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly CreateMesaEntradaCommandHandler _handler;

    public CreateMesaEntradaCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new CreateMesaEntradaCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_ComandoValido_CreaRegistroYRetornaSuccess()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateMesaEntradaCommand(
            1,
            2,
            3,
            "DOC-001",
            "Alta de proveedor",
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 25),
            "Observacion");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _db.MesasEntrada.Received(1).Add(Arg.Is<MesaEntrada>(x =>
            x.SucursalId == 1 &&
            x.TipoId == 2 &&
            x.TerceroId == 3 &&
            x.NroDocumento == "DOC-001" &&
            x.Asunto == "Alta de proveedor" &&
            x.EstadoFlow == EstadoMesaEntrada.Pendiente));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AssignMesaEntradaCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly AssignMesaEntradaCommandHandler _handler;

    public AssignMesaEntradaCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new AssignMesaEntradaCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_RegistroNoExiste_RetornaFailure()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new AssignMesaEntradaCommand(55, 9), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("55");
    }

    [Fact]
    public async Task Handle_RegistroExiste_AsignaResponsableYRetornaSuccess()
    {
        var mesa = MesaEntradaTestHelper.CrearMesaEntrada();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { mesa });
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new AssignMesaEntradaCommand(mesa.Id, 9), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AsignadoA.Should().Be(9);
        mesa.AsignadoA.Should().Be(9);
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class ChangeMesaEntradaEstadoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ChangeMesaEntradaEstadoCommandHandler _handler;

    public ChangeMesaEntradaEstadoCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new ChangeMesaEntradaEstadoCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_RegistroNoExiste_RetornaFailure()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new ChangeMesaEntradaEstadoCommand(77, 4, "EnProceso", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("77");
    }

    [Fact]
    public async Task Handle_EstadoFlowInvalido_RetornaFailure()
    {
        var mesa = MesaEntradaTestHelper.CrearMesaEntrada();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { mesa });
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new ChangeMesaEntradaEstadoCommand(mesa.Id, 4, "NoExiste", "obs"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("inválido");
    }

    [Fact]
    public async Task Handle_DatosValidos_CambiaEstadoYRetornaSuccess()
    {
        var mesa = MesaEntradaTestHelper.CrearMesaEntrada();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { mesa });
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new ChangeMesaEntradaEstadoCommand(mesa.Id, 4, "EnProceso", "revisando"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EstadoId.Should().Be(4);
        result.Value.EstadoFlow.Should().Be(nameof(EstadoMesaEntrada.EnProceso));
        mesa.EstadoId.Should().Be(4);
        mesa.EstadoFlow.Should().Be(EstadoMesaEntrada.EnProceso);
        mesa.Observacion.Should().Be("revisando");
    }
}

public class ArchiveMesaEntradaCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ArchiveMesaEntradaCommandHandler _handler;

    public ArchiveMesaEntradaCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new ArchiveMesaEntradaCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_RegistroNoExiste_RetornaFailure()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new ArchiveMesaEntradaCommand(88), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("88");
    }

    [Fact]
    public async Task Handle_RegistroExiste_ArchivaYRetornaSuccess()
    {
        var mesa = MesaEntradaTestHelper.CrearMesaEntrada();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { mesa });
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new ArchiveMesaEntradaCommand(mesa.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        mesa.EstadoFlow.Should().Be(EstadoMesaEntrada.Archivado);
    }
}

public class CancelMesaEntradaCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly CancelMesaEntradaCommandHandler _handler;

    public CancelMesaEntradaCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new CancelMesaEntradaCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_RegistroNoExiste_RetornaFailure()
    {
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        _db.MesasEntrada.Returns(mesas);

        var result = await _handler.Handle(new CancelMesaEntradaCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_RegistroExiste_AnulaYRetornaSuccess()
    {
        var mesa = MesaEntradaTestHelper.CrearMesaEntrada();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[] { mesa });
        _db.MesasEntrada.Returns(mesas);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new CancelMesaEntradaCommand(mesa.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        mesa.EstadoFlow.Should().Be(EstadoMesaEntrada.Anulado);
    }
}

public class MesaEntradaCommandValidatorTests
{
    [Fact]
    public void CreateMesaEntradaValidator_CommandInvalido_ReportaErrores()
    {
        var validator = new CreateMesaEntradaCommandValidator();

        var result = validator.Validate(new CreateMesaEntradaCommand(0, 0, null, string.Empty, string.Empty, new DateOnly(2026, 3, 20), null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMesaEntradaCommand.SucursalId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMesaEntradaCommand.TipoId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMesaEntradaCommand.NroDocumento));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMesaEntradaCommand.Asunto));
    }

    [Fact]
    public void AssignMesaEntradaValidator_CommandInvalido_ReportaErrores()
    {
        var validator = new AssignMesaEntradaCommandValidator();

        var result = validator.Validate(new AssignMesaEntradaCommand(0, 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignMesaEntradaCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignMesaEntradaCommand.UsuarioId));
    }

    [Fact]
    public void ChangeMesaEntradaEstadoValidator_CommandInvalido_ReportaErrores()
    {
        var validator = new ChangeMesaEntradaEstadoCommandValidator();

        var result = validator.Validate(new ChangeMesaEntradaEstadoCommand(0, -1, string.Empty, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangeMesaEntradaEstadoCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangeMesaEntradaEstadoCommand.EstadoId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangeMesaEntradaEstadoCommand.EstadoFlow));
    }

    [Fact]
    public void ArchiveAndCancelValidators_IdInvalido_ReportanError()
    {
        var archiveValidator = new ArchiveMesaEntradaCommandValidator();
        var cancelValidator = new CancelMesaEntradaCommandValidator();

        archiveValidator.Validate(new ArchiveMesaEntradaCommand(0)).IsValid.Should().BeFalse();
        cancelValidator.Validate(new CancelMesaEntradaCommand(0)).IsValid.Should().BeFalse();
    }
}

internal static class MesaEntradaTestHelper
{
    public static MesaEntrada CrearMesaEntrada()
        => MesaEntrada.Crear(
            1,
            2,
            3,
            "DOC-001",
            "Alta de proveedor",
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 25),
            "Observacion",
            1);
}