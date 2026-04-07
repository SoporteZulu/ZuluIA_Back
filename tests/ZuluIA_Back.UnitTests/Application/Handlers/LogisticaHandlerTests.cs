using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Logistica.Commands;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.UnitTests.Helpers;
using LogisticaOrdenEmpaque = ZuluIA_Back.Domain.Entities.Logistica.OrdenEmpaque;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CreateOrdenEmpaqueCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private CreateOrdenEmpaqueCommandHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_DatosValidos_CreaOrdenYPersiste()
    {
        var ordenes = MockDbSetHelper.CreateMockDbSet<LogisticaOrdenEmpaque>();
        _db.OrdenesEmpaquesLogistica.Returns(ordenes);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateOrdenEmpaqueCommand(
            1,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            1m,
            new DateOnly(2026, 3, 20),
            null,
            null,
            null,
            null,
            200m,
            "Obs",
            [new CreateOrdenEmpaqueDetalleInput(1, "Item A", 2m, 100m, 10m, null)]);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ordenes.Should().ContainSingle();
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class ConfirmOrdenEmpaqueCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private ConfirmOrdenEmpaqueCommandHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_OrdenNoEncontrada_RetornaFailure()
    {
        var ordenes = MockDbSetHelper.CreateMockDbSet<LogisticaOrdenEmpaque>();
        _db.OrdenesEmpaquesLogistica.Returns(ordenes);

        var result = await Sut().Handle(new ConfirmOrdenEmpaqueCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_OrdenPendiente_ConfirmaYPersiste()
    {
        var orden = LogisticaHandlerTestHelper.CrearOrdenValida();
        LogisticaHandlerTestHelper.SetId(orden, 10L);
        var ordenes = MockDbSetHelper.CreateMockDbSet(new[] { orden });
        _db.OrdenesEmpaquesLogistica.Returns(ordenes);

        var result = await Sut().Handle(new ConfirmOrdenEmpaqueCommand(10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Estado.Should().Be("CONFIRMADO");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class CancelOrdenEmpaqueCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private CancelOrdenEmpaqueCommandHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_OrdenNoEncontrada_RetornaFailure()
    {
        var ordenes = MockDbSetHelper.CreateMockDbSet<LogisticaOrdenEmpaque>();
        _db.OrdenesEmpaquesLogistica.Returns(ordenes);

        var result = await Sut().Handle(new CancelOrdenEmpaqueCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_OrdenPendiente_AnulaYPersiste()
    {
        var orden = LogisticaHandlerTestHelper.CrearOrdenValida();
        LogisticaHandlerTestHelper.SetId(orden, 10L);
        var ordenes = MockDbSetHelper.CreateMockDbSet(new[] { orden });
        _db.OrdenesEmpaquesLogistica.Returns(ordenes);

        var result = await Sut().Handle(new CancelOrdenEmpaqueCommand(10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Estado.Should().Be("ANULADO");
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class OrdenEmpaqueCommandValidatorTests
{
    [Fact]
    public void CreateValidator_TerceroInvalido_RetornaError()
    {
        var validator = new CreateOrdenEmpaqueCommandValidator();

        var result = validator.Validate(new CreateOrdenEmpaqueCommand(
            0, null, null, null, null, null, null, null, null,
            1m, new DateOnly(2026, 3, 20), null, null, null, null,
            100m, null,
            [new CreateOrdenEmpaqueDetalleInput(1, "Item", 1m, 100m, null, null)]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "TerceroId");
    }

    [Fact]
    public void CreateValidator_SinDetalles_RetornaError()
    {
        var validator = new CreateOrdenEmpaqueCommandValidator();

        var result = validator.Validate(new CreateOrdenEmpaqueCommand(
            1, null, null, null, null, null, null, null, null,
            1m, new DateOnly(2026, 3, 20), null, null, null, null,
            100m, null,
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Detalles");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConfirmValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new ConfirmOrdenEmpaqueCommandValidator();

        var result = validator.Validate(new ConfirmOrdenEmpaqueCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CancelValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new CancelOrdenEmpaqueCommandValidator();

        var result = validator.Validate(new CancelOrdenEmpaqueCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void Validators_DatosValidos_Pasan()
    {
        var createValidator = new CreateOrdenEmpaqueCommandValidator();
        var confirmValidator = new ConfirmOrdenEmpaqueCommandValidator();
        var cancelValidator = new CancelOrdenEmpaqueCommandValidator();

        var createResult = createValidator.Validate(new CreateOrdenEmpaqueCommand(
            1, null, null, null, null, null, null, null, null,
            1m, new DateOnly(2026, 3, 20), null, null, null, null,
            100m, null,
            [new CreateOrdenEmpaqueDetalleInput(1, "Item", 1m, 100m, null, null)]));

        createResult.IsValid.Should().BeTrue();
        confirmValidator.Validate(new ConfirmOrdenEmpaqueCommand(1)).IsValid.Should().BeTrue();
        cancelValidator.Validate(new CancelOrdenEmpaqueCommand(1)).IsValid.Should().BeTrue();
    }
}

internal static class LogisticaHandlerTestHelper
{
    public static LogisticaOrdenEmpaque CrearOrdenValida()
        => LogisticaOrdenEmpaque.Crear(
            1, null, null, null, null, null, null, null, null,
            1m, new DateOnly(2026, 3, 20), null, null, null, null,
            100m, null);

    public static void SetId(object target, long id)
    {
        var property = target.GetType().BaseType?.GetProperty("Id") ?? target.GetType().GetProperty("Id");
        property.Should().NotBeNull();
        property!.SetValue(target, id);
    }
}