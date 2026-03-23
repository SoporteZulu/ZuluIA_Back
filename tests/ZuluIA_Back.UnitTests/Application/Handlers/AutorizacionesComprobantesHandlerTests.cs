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

public class CreateAutorizacionComprobanteCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly CreateAutorizacionComprobanteCommandHandler _handler;

    public CreateAutorizacionComprobanteCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _handler = new CreateAutorizacionComprobanteCommandHandler(_db, _currentUser);
    }

    [Fact]
    public async Task Handle_ComandoValido_CreaAutorizacionYRetornaSuccess()
    {
        var autorizaciones = MockDbSetHelper.CreateMockDbSet<AutorizacionComprobante>();
        _db.AutorizacionesComprobantes.Returns(autorizaciones);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _currentUser.UserId.Returns((long?)7L);

        var result = await _handler.Handle(new CreateAutorizacionComprobanteCommand(10, 2, "venta"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        autorizaciones.Received(1).Add(Arg.Is<AutorizacionComprobante>(x =>
            x.ComprobanteId == 10 &&
            x.SucursalId == 2 &&
            x.TipoOperacion == "VENTA" &&
            x.Estado == EstadoAutorizacion.Pendiente));
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AutorizarComprobanteCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly AutorizarComprobanteCommandHandler _handler;

    public AutorizarComprobanteCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _handler = new AutorizarComprobanteCommandHandler(_db, _currentUser);
    }

    [Fact]
    public async Task Handle_AutorizacionNoExiste_RetornaFailure()
    {
        var autorizaciones = MockDbSetHelper.CreateMockDbSet<AutorizacionComprobante>();
        _db.AutorizacionesComprobantes.Returns(autorizaciones);

        var result = await _handler.Handle(new AutorizarComprobanteCommand(99, 4, "ok"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_AutorizacionPendiente_AutorizaYRetornaSuccess()
    {
        var autorizacion = AutorizacionComprobanteTestHelper.CrearAutorizacion();
        var autorizaciones = MockDbSetHelper.CreateMockDbSet(new[] { autorizacion });
        _db.AutorizacionesComprobantes.Returns(autorizaciones);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _currentUser.UserId.Returns((long?)9L);

        var result = await _handler.Handle(new AutorizarComprobanteCommand(autorizacion.Id, 5, " aprobado "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        autorizacion.Estado.Should().Be(EstadoAutorizacion.Autorizado);
        autorizacion.AutorizadoPor.Should().Be(5);
        autorizacion.Motivo.Should().Be("aprobado");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AutorizacionNoPendiente_RetornaFailure()
    {
        var autorizacion = AutorizacionComprobanteTestHelper.CrearAutorizacion();
        autorizacion.Autorizar(5, "ok", 1);
        var autorizaciones = MockDbSetHelper.CreateMockDbSet(new[] { autorizacion });
        _db.AutorizacionesComprobantes.Returns(autorizaciones);

        var result = await _handler.Handle(new AutorizarComprobanteCommand(autorizacion.Id, 6, "nuevo intento"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("pendientes");
    }
}

public class RechazarAutorizacionComprobanteCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly RechazarAutorizacionComprobanteCommandHandler _handler;

    public RechazarAutorizacionComprobanteCommandHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _handler = new RechazarAutorizacionComprobanteCommandHandler(_db, _currentUser);
    }

    [Fact]
    public async Task Handle_AutorizacionNoExiste_RetornaFailure()
    {
        var autorizaciones = MockDbSetHelper.CreateMockDbSet<AutorizacionComprobante>();
        _db.AutorizacionesComprobantes.Returns(autorizaciones);

        var result = await _handler.Handle(new RechazarAutorizacionComprobanteCommand(77, 4, "rechazo"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("77");
    }

    [Fact]
    public async Task Handle_AutorizacionPendiente_RechazaYRetornaSuccess()
    {
        var autorizacion = AutorizacionComprobanteTestHelper.CrearAutorizacion();
        var autorizaciones = MockDbSetHelper.CreateMockDbSet(new[] { autorizacion });
        _db.AutorizacionesComprobantes.Returns(autorizaciones);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _currentUser.UserId.Returns((long?)8L);

        var result = await _handler.Handle(new RechazarAutorizacionComprobanteCommand(autorizacion.Id, 11, " fuera de politica "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        autorizacion.Estado.Should().Be(EstadoAutorizacion.Rechazado);
        autorizacion.AutorizadoPor.Should().Be(11);
        autorizacion.Motivo.Should().Be("fuera de politica");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AutorizacionComprobanteCommandValidatorTests
{
    [Fact]
    public void CreateValidator_ComandoInvalido_ReportaErrores()
    {
        var validator = new CreateAutorizacionComprobanteCommandValidator();

        var result = validator.Validate(new CreateAutorizacionComprobanteCommand(0, 0, string.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAutorizacionComprobanteCommand.ComprobanteId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAutorizacionComprobanteCommand.SucursalId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAutorizacionComprobanteCommand.TipoOperacion));
    }

    [Fact]
    public void AutorizarYRechazarValidators_ComandosInvalidos_ReportanErrores()
    {
        var autorizarValidator = new AutorizarComprobanteCommandValidator();
        var rechazarValidator = new RechazarAutorizacionComprobanteCommandValidator();

        autorizarValidator.Validate(new AutorizarComprobanteCommand(0, 0, null)).IsValid.Should().BeFalse();
        rechazarValidator.Validate(new RechazarAutorizacionComprobanteCommand(0, 0, null)).IsValid.Should().BeFalse();
    }
}

internal static class AutorizacionComprobanteTestHelper
{
    public static AutorizacionComprobante CrearAutorizacion(long id = 1)
    {
        var autorizacion = AutorizacionComprobante.Crear(10, 2, "VENTA", 1);
        SetProperty(autorizacion, nameof(AutorizacionComprobante.Id), id);
        return autorizacion;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }
}