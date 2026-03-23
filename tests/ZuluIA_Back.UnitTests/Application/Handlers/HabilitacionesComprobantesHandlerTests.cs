using FluentAssertions;
using NSubstitute;
using System.Reflection;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CreateHabilitacionComprobanteCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly CreateHabilitacionComprobanteCommandHandler _handler;

    public CreateHabilitacionComprobanteCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _handler = new CreateHabilitacionComprobanteCommandHandler(_db, _currentUser);
    }

    [Fact]
    public async Task Handle_ComandoValido_CreaHabilitacionYRetornaSuccess()
    {
        var habilitaciones = MockDbSetHelper.CreateMockDbSet<HabilitacionComprobante>();
        _db.HabilitacionesComprobantes.Returns(habilitaciones);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _currentUser.UserId.Returns((long?)7L);

        var result = await _handler.Handle(new CreateHabilitacionComprobanteCommand(12, 3, "comprobante", " retencion "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        habilitaciones.Received(1).Add(Arg.Is<HabilitacionComprobante>(x =>
            x.ComprobanteId == 12 &&
            x.SucursalId == 3 &&
            x.TipoDocumento == "COMPROBANTE" &&
            x.MotivoBloqueo == "retencion" &&
            x.Estado == EstadoHabilitacion.Pendiente));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class HabilitarComprobanteCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly HabilitarComprobanteCommandHandler _handler;

    public HabilitarComprobanteCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _handler = new HabilitarComprobanteCommandHandler(_db, _currentUser);
    }

    [Fact]
    public async Task Handle_HabilitacionNoExiste_RetornaFailure()
    {
        var habilitaciones = MockDbSetHelper.CreateMockDbSet<HabilitacionComprobante>();
        _db.HabilitacionesComprobantes.Returns(habilitaciones);

        var result = await _handler.Handle(new HabilitarComprobanteCommand(101, 4, "ok"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("101");
    }

    [Fact]
    public async Task Handle_HabilitacionPendiente_HabilitaYRetornaSuccess()
    {
        var habilitacion = HabilitacionComprobanteTestHelper.CrearHabilitacion();
        var habilitaciones = MockDbSetHelper.CreateMockDbSet(new[] { habilitacion });
        _db.HabilitacionesComprobantes.Returns(habilitaciones);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _currentUser.UserId.Returns((long?)15L);

        var result = await _handler.Handle(new HabilitarComprobanteCommand(habilitacion.Id, 6, " liberado "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        habilitacion.Estado.Should().Be(EstadoHabilitacion.Habilitado);
        habilitacion.HabilitadoPor.Should().Be(6);
        habilitacion.ObservacionHabilitacion.Should().Be("liberado");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HabilitacionNoPendiente_RetornaFailure()
    {
        var habilitacion = HabilitacionComprobanteTestHelper.CrearHabilitacion();
        habilitacion.Habilitar(6, "ok", 1);
        var habilitaciones = MockDbSetHelper.CreateMockDbSet(new[] { habilitacion });
        _db.HabilitacionesComprobantes.Returns(habilitaciones);

        var result = await _handler.Handle(new HabilitarComprobanteCommand(habilitacion.Id, 7, "nuevo intento"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("pendientes");
    }
}

public class RechazarHabilitacionComprobanteCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly RechazarHabilitacionComprobanteCommandHandler _handler;

    public RechazarHabilitacionComprobanteCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _handler = new RechazarHabilitacionComprobanteCommandHandler(_db, _currentUser);
    }

    [Fact]
    public async Task Handle_HabilitacionNoExiste_RetornaFailure()
    {
        var habilitaciones = MockDbSetHelper.CreateMockDbSet<HabilitacionComprobante>();
        _db.HabilitacionesComprobantes.Returns(habilitaciones);

        var result = await _handler.Handle(new RechazarHabilitacionComprobanteCommand(88, 4, "rechazo"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("88");
    }

    [Fact]
    public async Task Handle_HabilitacionPendiente_RechazaYRetornaSuccess()
    {
        var habilitacion = HabilitacionComprobanteTestHelper.CrearHabilitacion();
        var habilitaciones = MockDbSetHelper.CreateMockDbSet(new[] { habilitacion });
        _db.HabilitacionesComprobantes.Returns(habilitaciones);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _currentUser.UserId.Returns((long?)21L);

        var result = await _handler.Handle(new RechazarHabilitacionComprobanteCommand(habilitacion.Id, 8, " documentacion incompleta "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        habilitacion.Estado.Should().Be(EstadoHabilitacion.Rechazado);
        habilitacion.HabilitadoPor.Should().Be(8);
        habilitacion.ObservacionHabilitacion.Should().Be("documentacion incompleta");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class HabilitacionComprobanteCommandValidatorTests
{
    [Fact]
    public void CreateValidator_ComandoInvalido_ReportaErrores()
    {
        var validator = new CreateHabilitacionComprobanteCommandValidator();

        var result = validator.Validate(new CreateHabilitacionComprobanteCommand(0, 0, string.Empty, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHabilitacionComprobanteCommand.ComprobanteId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHabilitacionComprobanteCommand.SucursalId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHabilitacionComprobanteCommand.TipoDocumento));
    }

    [Fact]
    public void HabilitarYRechazarValidators_ComandosInvalidos_ReportanErrores()
    {
        var habilitarValidator = new HabilitarComprobanteCommandValidator();
        var rechazarValidator = new RechazarHabilitacionComprobanteCommandValidator();

        habilitarValidator.Validate(new HabilitarComprobanteCommand(0, 0, null)).IsValid.Should().BeFalse();
        rechazarValidator.Validate(new RechazarHabilitacionComprobanteCommand(0, 0, null)).IsValid.Should().BeFalse();
    }
}

internal static class HabilitacionComprobanteTestHelper
{
    public static HabilitacionComprobante CrearHabilitacion(long id = 1)
    {
        var habilitacion = HabilitacionComprobante.Crear(12, 3, "COMPROBANTE", "retencion", 1);
        SetProperty(habilitacion, nameof(HabilitacionComprobante.Id), id);
        return habilitacion;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }
}